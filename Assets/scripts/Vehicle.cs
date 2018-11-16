using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Vehicle {

    public enum vehicleType {
        Tank,
        Truck,
        Heli,
        Spy,
        Base,
        BaseTurret
    }

    public enum vehicleSide {
        Player,
        Enemy,
        Other
    }

    public GameObject vehicleObject;
    public vehicleType type;
    public vehicleSide side;

    public float maxHealth;
    public bool selected = false;
    public int controlGroup = 0;
    public bool aiControl = true;

    private float health;
    public float damageMax;
    public float damageMinPercentage;
    public bool damageAOE;
    public float damageAOErange;

    public float sightRange;
    public float attackRange;

    public float summonRange = 0;

    public float energyCost;

    public bool debug=true;

    public Vehicle(GameObject v, vehicleType t, vehicleSide s=vehicleSide.Player) {
        vehicleObject = v;
        type = t;
        side = s;
        SetValues();
        health = maxHealth;
    }

    private void SetValues() {
        switch (type) {
            case vehicleType.Tank:
                maxHealth = 500;
                damageMax = 100;
                damageAOE = false;
                sightRange = 100;
                attackRange = 50;
                energyCost = 400;
                break;
            case vehicleType.Truck:
                maxHealth = 300;
                damageMax = 75;
                damageAOE = true;
                damageMinPercentage = 0;
                damageAOErange = 5;
                sightRange = 120;
                attackRange = 50;
                energyCost = 200;
                break;
            case vehicleType.Heli:
                maxHealth = 200;
                damageMax = 150;
                damageMinPercentage = 0.2f;
                damageAOErange = 6;
                sightRange = 120;
                attackRange = 75;
                energyCost = 350;
                break;
            case vehicleType.Spy:
                maxHealth = 100;
                damageMax = 0;
                damageAOE = false;
                sightRange = 200;
                attackRange = 0;
                energyCost = 100;
                break;
            case vehicleType.Base:
                maxHealth = 5000;
                damageMax = 300;
                damageAOE = true;
                damageMinPercentage = 0.5f;
                damageAOErange = 10;
                sightRange = 200;
                attackRange = 100;
                summonRange = 75;
                energyCost = 0;
                break;
            case vehicleType.BaseTurret:
                maxHealth = 400;
                damageMax = 100;
                damageAOE = false;
                sightRange = 75;
                attackRange = 75;
                energyCost = 400;
                break;
            default:
                break;
        }
    }

    public virtual void DoDamage(float damage, vehicleType enemyType) { //NYI: rock-paper-scissors style damage using enemyType variable
        health -= damage;

        Debug.Log(vehicleObject.name + "dmg: " + damage + ", health remaining: " + health);

        if (health <= 0) {
            Die();
        }
    }

    private void Die() {
        GameLogic.Instance.RemoveVehicle(this);
        vehicleObject.SetActive(false);
        GameObject.Destroy(vehicleObject);
    }
}
