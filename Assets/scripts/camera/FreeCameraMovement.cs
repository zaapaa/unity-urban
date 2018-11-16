using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class FreeCameraMovement : CameraController {

    public float origMoveSpeed = 100;
    public float moveSpeed;
    public float moveSpeedMultiplier = 5;
    public float scrollMult = -20;

    public float offSet = -50;

    private float xMax = 450;
    private float xMin = 50;
    private float zMax = 450;
    private float zMin = 50;

    private float yMax = 300;
    private float yMin = 5;

    private float origYPos;

    public AnimationCurve moveCurve;
    bool moving;
    float moveTime = 0.5f;
    float moveTimer;
    Vector3 movePosition;

    protected override void Awake() {
        xMaxRot = 89;
    }


    protected override void Start() {
        moveSpeed = origMoveSpeed;
        origYPos = transform.position.y;
    }

    public override void Move(float vertical, float horizontal, float altitude) {
        Vector3 newPos = transform.position;

        newPos += transform.forward.normalized * vertical * moveSpeed * Time.deltaTime;
        newPos += transform.right.normalized * horizontal * moveSpeed * Time.deltaTime;

        if (newPos.x > xMax) {
            newPos.x = xMax;
        } else if (newPos.x < xMin) {
            newPos.x = xMin;
        }
        if (newPos.z > zMax) {
            newPos.z = zMax;
        } else if (newPos.z < zMin) {
            newPos.z = zMin;
        }
        newPos.y = transform.position.y;
        if (altitude != 0) {
            newPos.y += altitude * moveSpeed * Time.deltaTime*scrollMult;

            if(newPos.y > yMax) {
                newPos.y = transform.position.y;
            }
        }

        Ray ray = new Ray(newPos, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.tag == "terrain" && Vector3.Distance(hit.point, transform.position) < yMin) {
                newPos.y = hit.point.y + yMin;
            }
        } else {
            ray.direction = Vector3.up;
            if(Physics.Raycast(ray, out hit)) {
                newPos.y = hit.point.y + yMin;
            } else {
                Debug.LogWarning("no objects detected above or below camera");
            }
        }
        transform.position = newPos;
    }

    public override void AdjustPosition(Vector3 position) {
        position.y = origYPos;
        Vector3 temp = transform.forward;
        temp.y = 0f;

        position += temp * offSet;
        StartCoroutine(SmoothMove(position));
    }

    IEnumerator SmoothMove(Vector3 newPos) {
        moveTimer = 0;
        while (moveTimer <= moveTime) {
            transform.position = Vector3.Lerp(transform.position, newPos, moveCurve.Evaluate(moveTimer / moveTime));
            moveTimer += Time.deltaTime;
            yield return null;
        }
    }
}

