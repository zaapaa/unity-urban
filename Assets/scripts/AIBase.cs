using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIBase : MonoBehaviour {

    List<Vehicle> allVehicles;
    BaseController controller;

    bool commandGiven = false;

    public float maxSummonDistance = 75;

    int counter = 1;

    void Awake() {
        controller = GetComponent<BaseController>();
        allVehicles = new List<Vehicle>();
    }

    void Start() {

    }

    void Update() {
        if (controller.energy > controller.currentVehicle.energyCost && !commandGiven) {
            Vehicle newVeh = controller.SummonCurrentObject();
            if (newVeh != null) {
                allVehicles.Add(newVeh);
                //commandGiven = true;
            }
        }
        if (allVehicles.Count >= 3 && !commandGiven) {
            //commandGiven = true;
            foreach (Vehicle v in allVehicles) {
                Vector3 pos = GameLogic.Instance.player.transform.position;
                Debug.Log("Command " + pos);
                Command newCommand = new Command(Command.commandTypes.AttackMove, null, pos);
                v.vehicleObject.GetComponent<AIMovement>().AddCommand(newCommand);
            }
            allVehicles.Clear();
        }
    }

    public Vehicle SummonObject(Vehicle vehicle) {
        GameObject vehiclePrefab = vehicle.vehicleObject;
        Vector3 summonPoint = Vector3.zero;
        while(summonPoint == Vector3.zero) {
            summonPoint = getPos();
        }
        summonPoint.y += vehiclePrefab.GetComponentInChildren<Renderer>().bounds.size.x / 2;
        Debug.Log("summonPoint: " + summonPoint + ", renderer: " + vehiclePrefab.GetComponentInChildren<Renderer>().gameObject.name);
        Vehicle newVehicle = GameLogic.Instance.SummonVehicle(vehiclePrefab, summonPoint, Quaternion.identity, vehicle.type, controller.side);
        newVehicle.debug = false;
        GameObject newObject = newVehicle.vehicleObject;
        newObject.name = "Enemy " + vehiclePrefab.name + counter;
        GameLogic.Instance.setAllLayers(newObject, GameLogic.Instance.enemyLayer);
        Debug.Log("Summoned " + newObject.name);
        counter++;
        return newVehicle;
    }

    private Vector3 getPos() {
        RaycastHit hit;
        Vector2 randomPos = Random.insideUnitCircle * maxSummonDistance;
        Vector3 newPos = new Vector3(transform.position.x+randomPos.x, 300, transform.position.z+randomPos.y);
        Ray ray = new Ray(newPos, Vector3.down);
        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.tag == "terrain") {
                return hit.point;
            }
        }
        return Vector3.zero;
    }
}
