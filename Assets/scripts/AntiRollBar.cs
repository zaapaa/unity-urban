using UnityEngine;
using System.Collections;

public class AntiRollBar : MonoBehaviour {

    public WheelCollider left, right;
    float antiRoll = 5000f;
    Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        WheelHit hit;
        float travelL = 1f;
        float travelR = 1f;

        bool groundedL = (left.GetGroundHit(out hit));
        if (groundedL) {
            travelL = (-left.transform.InverseTransformPoint(hit.point).y - left.radius) / left.suspensionDistance;
        }

        bool groundedR = (right.GetGroundHit(out hit));
        if (groundedR) {
            travelR = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL) {
            rb.AddForceAtPosition(left.transform.up * -antiRollForce, left.transform.position);
        }
        if (groundedR) {
            rb.AddForceAtPosition(right.transform.up * antiRollForce, right.transform.position);
        }



    }
}
