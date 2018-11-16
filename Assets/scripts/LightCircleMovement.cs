using UnityEngine;
using System.Collections;

public class LightCircleMovement : MonoBehaviour {

    public float angle = 0;
    float speed = (2 * Mathf.PI) / 60;
    float radius = 225;
    Vector3 newPos;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        angle += speed * Time.deltaTime;
        newPos = transform.localPosition;
        newPos.x = Mathf.Cos(angle) * radius;
        newPos.z = Mathf.Sin(angle) * radius;

        transform.localPosition = newPos;

        transform.LookAt(transform.parent);
	}
}
