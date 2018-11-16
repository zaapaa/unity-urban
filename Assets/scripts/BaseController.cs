using UnityEngine;
using System.Collections;

public class BaseController : MonoBehaviour, IUnitController {

    Vehicle.vehicleType currentType;
    public Vehicle currentVehicle;

    public Vehicle baseVehicle;
    public Vehicle.vehicleSide side = Vehicle.vehicleSide.Player;

    public float energy=0;
    float maxEnergy = 1.0E+12f;

	void Awake(){
        baseVehicle = new Vehicle(gameObject, Vehicle.vehicleType.Base, side);
    }
	void Start () {
        currentType = Vehicle.vehicleType.Tank;
        currentVehicle = GetVehicleByType(currentType);
    }
	
	// Update is called once per frame
	void Update () {

	}
    public void AddEnergy(float value) {
        if (energy + value < maxEnergy) {
            energy += value;
        } else {
            energy = maxEnergy;
        }
    }

    public Vehicle SummonCurrentObject() {
        if (currentVehicle == null) {
            throw new UnityException();
        }
        if (energy > currentVehicle.energyCost) {
            energy -= currentVehicle.energyCost;
            if (side == Vehicle.vehicleSide.Player) {
                ObjectSummoner summoner = GetComponent<ObjectSummoner>();

                summoner.SummonObject(currentVehicle);
            } else if(side == Vehicle.vehicleSide.Enemy) {
                AIBase ai = GetComponent<AIBase>();
                return ai.SummonObject(currentVehicle);
            }
        }
        return null;
    }

    void changeObject() {
        currentType++;
        if (currentType == Vehicle.vehicleType.Base) {
            currentType = Vehicle.vehicleType.Tank;
        }
        currentVehicle = GetVehicleByType(currentType);
        Debug.Log("Changed object to id:" + currentType + ", name:" + currentVehicle.vehicleObject.name);
    }

    Vehicle GetVehicleByType(Vehicle.vehicleType type) {
        foreach (Vehicle v in GameLogic.Instance.vehicleTypes) {
            if (type == v.type) {
                return v;
            }
        }
        return null;
    }

    public bool isMoving() {
        return false;
    }
    public Vehicle getVehicle() {
        return baseVehicle;
    }
}
