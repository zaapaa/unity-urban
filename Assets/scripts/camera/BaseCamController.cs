using UnityEngine;
using System.Collections;
using System;

public class BaseCamController : CameraController {

    float xMinRot = -89;

    public AnimationCurve moveCurve;
    bool moving;
    bool rotating;
    float moveTime = 0.5f;
    float moveTimer;
    Vector3 movePosition;

    protected override void Awake() {
        xMaxRot = 10;
    }


    protected override void Start() {
    }

    protected override void Update() {
        if (!moving) {
            if (transform.eulerAngles.x > xMaxRot) {
                Vector3 newRot = transform.eulerAngles;
                newRot.x = xMaxRot;
            } else if (transform.eulerAngles.x < xMinRot) {
                Vector3 newRot = transform.eulerAngles;
                newRot.x = xMinRot;
            }
        }

    }

    public override void AdjustPosition(Vector3 position) {
        position.y -= GameLogic.Instance.player.GetComponent<Renderer>().bounds.size.y * 0.55f;
        moving = true;

        Vector3 newRot = transform.eulerAngles;

        if (transform.eulerAngles.x > xMaxRot) {
            rotating = true;
            newRot.x = xMaxRot;
        } else if (transform.eulerAngles.x < xMinRot) {
            newRot.x = xMinRot;
            rotating = true;
        }

        StartCoroutine(SmoothMove(position, newRot));
    }

    IEnumerator SmoothMove(Vector3 newPos, Vector3 newRot) {
        moveTimer = 0;
        while (moveTimer <= moveTime) {
            transform.position = Vector3.Lerp(transform.position, newPos, moveCurve.Evaluate(moveTimer / moveTime));
            if (rotating) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRot), moveCurve.Evaluate(moveTimer / moveTime));
            }
            moveTimer += Time.deltaTime;
            yield return null;
        }
        moving = false;
        rotating = false;
    }
}
