using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PowerStation : MonoBehaviour {
    List<GameObject> bases;
    float energyRateMax = 20;
    float energyRateAdd = 5;
    float playerBonus = 5;
    float energyRange = 50;

    void Awake() {

    }

    // Use this for initialization
    void Start() {
        bases = GameLogic.Instance.bases;
    }

    // Update is called once per frame
    void Update() {
        GameLogic.Instance.distanceText.text = "Distances from power:";

        foreach (GameObject b in bases) {
            float distance = Vector3.Distance(transform.position, b.transform.position);
            if (distance < energyRange) {
                if(b.GetComponent<BaseController>().side != Vehicle.vehicleSide.Player) {
                    playerBonus = 0;
                }
                float energyRate = ((energyRange / distance) * energyRateMax + energyRateAdd + playerBonus) * Time.deltaTime;
                b.GetComponent<BaseController>().AddEnergy(energyRate);
            }
            if (b.GetComponent<BaseController>().side == Vehicle.vehicleSide.Player) {
                GameLogic.Instance.distanceText.text += "\n" + distance.ToString();
            }
        }
    }

    public void moreRegen() {
        energyRateMax *= 10;
    }

    public void lessRegen() {
        energyRateMax /= 10;
    }
}
