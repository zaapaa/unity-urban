using UnityEngine;
using System.Collections;

public class UnitHighlight : MonoBehaviour {

    public Vehicle vehicle = null;

    public bool selected = false;

    public AnimationCurve glowCurve;
    private bool glowing;
    private float glowTime = 1.5f;
    public float glowTimer;
    private float glowValue = 0.3f;

    // Use this for initialization
    void Start() {
        if (GetComponent<VehicleController>() != null) {
            vehicle = GetComponent<VehicleController>().vehicle;
        }
    }

    // Update is called once per frame
    void Update() {
        if (selected) {
            glowing = true;
        }
        if (glowing) {
            glowTimer += Time.deltaTime;
            if (glowTimer > glowTime) {
                glowTimer = 0;
            }
            Color newColor;

            float colorValue = glowCurve.Evaluate(glowTimer / glowTime) * glowValue;
            if (selected) {
                //Debug.Log("selected - blue: " + colorValue);
                newColor = new Color(0, 0, colorValue);
            } else {
                //Debug.Log("hovering - red: " + colorValue);
                newColor = new Color(colorValue, 0, 0);
            }
            
            GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", newColor);
        }
    }

    void OnMouseEnter() {
        if ((vehicle == null || vehicle.side == Vehicle.vehicleSide.Player) &&  GameLogic.Instance.camMode==GameLogic.cameraMode.Free) {
            glowing = true;
            GameLogic.Instance.hoverObject = gameObject;
        }
    }

    void OnMouseExit() {
        if (vehicle == null || vehicle.side == Vehicle.vehicleSide.Player) {
            StopGlowing(!selected);
        }
    }

    public void StopGlowing(bool stop) {
        if (stop) {
            Invoke("StopGlow", (glowTime - glowTimer));
        }
        
    }

    void StopGlow() {
        glowing = false;
    }

    public void Select() {
        selected = true;
        if (vehicle != null) {
            vehicle.selected = true;
        }
    }

    public void Unselect() {
        selected = false;
        StopGlowing(true);
        if (vehicle != null) {
            vehicle.selected = false;
        }
    }
}
