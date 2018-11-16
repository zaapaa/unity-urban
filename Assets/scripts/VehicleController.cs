using UnityEngine;
using System.Collections;
using System;

public class VehicleController : MonoBehaviour {

    public GameObject wheels;
    WheelController wheelController;

    public Vehicle vehicle;

    private new Collider collider;
    private new Rigidbody rigidbody;

    bool stopped;

    float stopThreshold = 2f;

    public float accelerateTorque = 50f;
    public float brakeTorque = 200f;
    public float turnTorque = 25f;

    Vector3 origRot;

    public float horizontal;
    public float vertical;

    public Vector3 localVel;




    void Awake() {
        wheels = transform.GetChild(1).gameObject;
        wheelController = wheels.GetComponent<WheelController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponentInChildren<Collider>();
    }

    // Use this for initialization
    void Start() {
        foreach (WheelData d in wheels.GetComponent<WheelController>().wheels) {
            Physics.IgnoreCollision(collider, d.col);
            d.col.ConfigureVehicleSubsteps(5, 12, 15);
        }
        origRot = transform.eulerAngles;

    }

    // Update is called once per frame
    void FixedUpdate() {

        //horizontal = Input.GetAxis("Horizontal");
        //vertical = Input.GetAxis("Vertical");

        localVel = transform.InverseTransformDirection(rigidbody.velocity);

        if (localVel.z < stopThreshold) {
            stopped = true;
        } else {
            stopped = false;
        }
        if (horizontal != 0f) {
            wheelController.Turn(horizontal, turnTorque, stopped);
        } else if (wheelController.turning) {
            wheelController.StopTurn();
        }

        if (vertical > 0f) {
            wheelController.StopBrake();
            wheelController.Accelerate(accelerateTorque*vertical);
        } else if (vertical < 0f) {
            wheelController.Brake(brakeTorque);
        } else {
            wheelController.StopAccelerate();
            wheelController.StopBrake();
        }


        //if no more than 2 wheels on ground, stabilize vehicle (bring x and z rotations smoothly back to original)
        int numGroundedWheels = 0;
        foreach (WheelData d in wheels.GetComponent<WheelController>().wheels) {
            if (d.col.isGrounded) {
                numGroundedWheels++;
            }
        }
        if(numGroundedWheels <= wheels.GetComponent<WheelController>().wheelCount/2) {
            StabilizeVehicle();
        }
        //Debug.Log("stopped: " + stopped);
    }

    private void StabilizeVehicle() {
        Vector3 currentRot = transform.eulerAngles;

        if (currentRot.x != origRot.x) {
            currentRot.x = Mathf.LerpAngle(currentRot.x, origRot.x, Time.deltaTime);
        }
        if(currentRot.z != origRot.z) {
            currentRot.z = Mathf.LerpAngle(currentRot.z, origRot.z, Time.deltaTime);
        }
        transform.eulerAngles = currentRot;
    }


}
