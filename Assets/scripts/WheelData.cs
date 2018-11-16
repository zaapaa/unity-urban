using UnityEngine;
using System.Collections;

public class WheelData : MonoBehaviour {

    public enum WheelSide {
        Left,
        Right
    }

    public WheelSide side;
    public bool turning;
    public bool drive;
    public WheelCollider col;
    
    void Awake() {
        col = GetComponent<WheelCollider>();
    }
}
