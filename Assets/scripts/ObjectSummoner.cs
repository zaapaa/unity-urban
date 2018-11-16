using UnityEngine;
using System.Collections;

public class ObjectSummoner : MonoBehaviour {
    public float maxSummonDistance;

    int counter=1;
	// Use this for initialization
	void Start () {
        
	}

    public void SummonObject(Vehicle vehicle) {
        GameObject vehiclePrefab = vehicle.vehicleObject;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxSummonDistance)) {
            Vector3 summonPoint = hit.point;
            summonPoint.y += vehiclePrefab.GetComponentInChildren<Renderer>().bounds.size.x / 2;
            Debug.Log("summonPoint: " + summonPoint + "hitpoint: " + hit.point + ", renderer: " + vehiclePrefab.GetComponentInChildren<Renderer>().gameObject.name);
            Vehicle newVehicle = GameLogic.Instance.SummonVehicle(vehiclePrefab, summonPoint, Quaternion.identity, vehicle.type);
            GameObject newObject = newVehicle.vehicleObject;
            newObject.name = "Player " + vehiclePrefab.name + counter;
            GameLogic.Instance.setAllLayers(newObject, GameLogic.Instance.playerLayer);
            Debug.Log("Summoned " + newObject.name);
            counter++;
        }
    }

}
