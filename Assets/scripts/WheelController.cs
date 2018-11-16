using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WheelController : MonoBehaviour {

    public List<WheelData> wheels;

    public Text wheelInfos;
    public Text controllerInfos;
    public Text frictionInfos;

    public bool debugEnabled = false;

    public Rigidbody vehiclerb;

    public bool turning;
    public float leftBias = 1;
    public float rightBias = 1;

    public bool leftBrake;
    public bool rightBrake;

    public bool accelerating;
    public bool braking;

    public float accelerateTorque = 0;
    public float brakeTorque = 0;
    float brakeTorqueOriginal;
    float brakeTorqueIfTooFast = 50;

    public float maxRPM = 200;
    float maxTurnSpeed = 1;

    public int wheelCount;

    //WheelFrictionCurve straightForwardFriction = new WheelFrictionCurve();
    //WheelFrictionCurve straightSideFriction = new WheelFrictionCurve();
    //WheelFrictionCurve turnForwardFriction = new WheelFrictionCurve();
    //WheelFrictionCurve turnSideFriction = new WheelFrictionCurve();

    //public float turnForwardExtremumSlip;
    //public float turnForwardExtremumValue;
    //public float turnForwardAsymptoteSlip;
    //public float turnForwardAsymptoteValue;

    //public float turnSideExtremumSlip;
    //public float turnSideExtremumValue;
    //public float turnSideAsymptoteSlip;
    //public float turnSideAsymptoteValue;

    float straightForwardStiffness;
    float straightSideStiffness;
    public float turnForwardStiffness = 1.5f;
    public float turnSideStiffness = 0.2f;

    float forwardStiffness;
    float sideStiffness;


    void Awake() {

        wheelInfos = GameLogic.Instance.wheelInfoText;
        controllerInfos = GameLogic.Instance.controllerInfoText;
        frictionInfos = GameLogic.Instance.frictionInfoText;

        wheels = new List<WheelData>();
        foreach(WheelData d in GetComponentsInChildren<WheelData>()) {
            wheels.Add(d);
        }

        wheelCount = wheels.Count;

        straightForwardStiffness = wheels[0].col.forwardFriction.stiffness;
        straightSideStiffness = wheels[0].col.sidewaysFriction.stiffness;

        forwardStiffness = straightForwardStiffness;
        sideStiffness = straightSideStiffness;

        vehiclerb = GetComponentInParent<Rigidbody>();
    }

    // Use this for initialization
    void Start() {
        brakeTorqueOriginal = GetComponentInParent<VehicleController>().brakeTorque;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (debugEnabled) {
            wheelInfos.text = "";
            foreach (WheelData d in wheels) {
                wheelInfos.text += d.name + " motortorque: " + d.col.motorTorque + ", rpm: " + d.col.rpm + ", braketorque: " + d.col.brakeTorque + "\n";

            }
            controllerInfos.text = "acc: " + accelerateTorque + " left: " + leftBias + " right: " + rightBias +
                                   "\nturning: " + turning + "\naccelerating: " + accelerating + "\nbraking: " + braking +
                                   "\nleftBrake: " + leftBrake + "\nrightBrake " + rightBrake +
                                   "\ny-rotationspeed: " + vehiclerb.angularVelocity.y;
            frictionInfos.text = "stiffness:\tforward: " + forwardStiffness + "\n\t\t\t\tside: " + sideStiffness;
        }

    }

    public void Turn(float turnAmount, float turnTorque, bool inPlace = false) {
        turning = true;
        if (!inPlace) {
            //Debug.Log("Turning while moving");
            if (turnAmount < 0) { //turn left
                rightBias = Mathf.Abs(turnAmount) * 2;
                leftBias = 0f;
                leftBrake = true;
            } else if (turnAmount > 0) { //turn right
                leftBias = Mathf.Abs(turnAmount) * 2;
                rightBias = 0f;
                rightBrake = true;
            }
        } else {
            if (Mathf.Abs(vehiclerb.angularVelocity.y) < maxTurnSpeed) {
                //Debug.Log("Turning in place");
                if (turnAmount < 0) {
                    rightBias = Mathf.Abs(turnAmount);
                    leftBias = -rightBias;
                } else if (turnAmount > 0) {
                    leftBias = Mathf.Abs(turnAmount);
                    rightBias = -leftBias;
                }
                accelerateTorque = turnTorque;
                leftBrake = false;
                rightBrake = false;
            } else {
                leftBias = 0f;
                rightBias = 0f;
            }

        }

        if (accelerating || Mathf.Abs(GetComponentInParent<VehicleController>().localVel.z) > 5f) {
            sideStiffness = (turnSideStiffness + straightSideStiffness) / 2;
            forwardStiffness = (turnForwardStiffness + straightForwardStiffness) / 2;

        } else {
            sideStiffness = turnSideStiffness;
            forwardStiffness = turnForwardStiffness;
        }

        UpdateWheelTorques();
    }

    public void Accelerate(float torqueAmount) {
        accelerating = true;
        accelerateTorque = torqueAmount;

        if (turning) {
            sideStiffness = (turnSideStiffness + straightSideStiffness) / 2;
            forwardStiffness = (turnForwardStiffness + straightForwardStiffness) / 2;

        } else {
            sideStiffness = straightSideStiffness;
            forwardStiffness = straightForwardStiffness;
        }

        UpdateWheelTorques();
    }

    public void Brake(float torqueAmount) {
        braking = true;
        brakeTorque = torqueAmount;
        UpdateWheelTorques();
        brakeTorqueIfTooFast = torqueAmount / 10;
    }

    public void StopTurn() {
        turning = false;
        leftBias = 1f;
        rightBias = 1f;
        leftBrake = false;
        rightBrake = false;
        forwardStiffness = straightForwardStiffness;
        sideStiffness = straightSideStiffness;
        UpdateWheelTorques();

    }

    public void StopAccelerate() {
        accelerating = false;
        if (!turning) {
            accelerateTorque = 0;
        }

        UpdateWheelTorques();
    }

    public void StopBrake() {
        braking = false;
        brakeTorque = 0;

        UpdateWheelTorques();
    }

    void UpdateWheelTorques() {
        //Debug.Log("updating wheel torques");
        foreach(WheelData d in wheels) {
            d.col.brakeTorque = brakeTorque;
            if (d.side == WheelData.WheelSide.Left) {
                if (d.drive) {
                    d.col.motorTorque = accelerateTorque * leftBias;
                }
                if (leftBrake) {
                    d.col.brakeTorque = brakeTorqueOriginal / 2;
                }
            } else if (d.side == WheelData.WheelSide.Right) {
                if (d.drive) {
                    d.col.motorTorque = accelerateTorque * rightBias;
                }
                if (rightBrake) {
                    d.col.brakeTorque = brakeTorqueOriginal / 2;
                }
            }
            float rpm = Mathf.Abs(d.col.rpm);
            if(rpm > maxRPM) {
                d.col.motorTorque = 0;
                if(rpm > 4 * maxRPM) {
                    d.col.brakeTorque = brakeTorqueIfTooFast * (rpm / maxRPM);
                }
            }
            WheelFrictionCurve newForward = new WheelFrictionCurve();
            WheelFrictionCurve newSide = new WheelFrictionCurve();

            newForward = d.col.forwardFriction;
            newForward.stiffness = forwardStiffness;
            d.col.forwardFriction = newForward;

            newSide = d.col.sidewaysFriction;
            newSide.stiffness = sideStiffness;
            d.col.sidewaysFriction = newSide;
        }
    }
}
