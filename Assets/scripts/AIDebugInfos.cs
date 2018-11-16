using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AIDebugInfos : MonoBehaviour {

    public List<Vehicle> vehicles;
    public Text AIInfos;

    void Awake() {
    }
    // Use this for initialization
    void Start() {
        vehicles = GameLogic.Instance.vehicles;
        enabled = false;
    }

    // Update is called once per frame
    void Update() {
        AIInfos.text = "";
        if (vehicles.Count > 0) {
            foreach (Vehicle aiVehicle in vehicles) {
                if (aiVehicle.type!=Vehicle.vehicleType.Base && aiVehicle.debug) {
                    GameObject ai = aiVehicle.vehicleObject;
                    AIMovement aim = ai.GetComponent<AIMovement>();
                    string aiTargetName = aim.target == null ? "no target" : aim.target.name;
                    AIInfos.text += ai.name + ":\n";
                    AIInfos.text += "has target: " + aiTargetName + ", ";
                    AIInfos.text += "target point: " + aim.targetPoint + ", ";
                    AIInfos.text += "facing node: " + aim.facingNextNode + "\n";
                    AIInfos.text += aim.distanceInfo + "\n";
                    AIInfos.text += aim.moreInfo + "\n";
                    AIInfos.text += "z-speed: " + ai.GetComponent<VehicleController>().localVel.z + ", ";
                    AIInfos.text += "horizontal: " + ai.GetComponent<VehicleController>().horizontal + ", ";
                    AIInfos.text += "vertical: " + ai.GetComponent<VehicleController>().vertical + "\n";
                    AIInfos.text += "angleDiffToTarget: " + aim.rotationDiffY + "\n";
                    WheelController wc = ai.GetComponentInChildren<WheelController>();
                    AIInfos.text += "acc: " + wc.accelerateTorque + " left: " + wc.leftBias + " right: " + wc.rightBias +
                                           "\nturning: " + wc.turning + " accelerating: " + wc.accelerating + " braking: " + wc.braking +
                                           "\nleftBrake: " + wc.leftBrake + " rightBrake " + wc.rightBrake +
                                           "\ny-rotationspeed: " + ai.GetComponent<Rigidbody>().angularVelocity.y;
                    string currentCommandText = aim.currentCommand == null ? "no command" : aim.currentCommand.ToString();
                    AIInfos.text += "\nCurrent Command: " + currentCommandText + ", command stack:";
                    foreach (Command c in aim.commandQueue) {
                        AIInfos.text += "\n" + c.ToString();
                    }
                    AIInfos.text += "\n\n";
                }



            }
        }

    }
}
