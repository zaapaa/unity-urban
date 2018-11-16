using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    //Vector3 oldMousePos;
    //Vector3 mousePos;

    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);

    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);
    public Vector2 targetDirection;

    bool directionSet;

    protected float xMaxRot = 89;

    protected virtual void Awake() {

    }
    protected virtual void Start() {
    }
    protected virtual void Update() {
        if (Mathf.Abs(transform.eulerAngles.x) > xMaxRot) {
            Vector3 newRot = transform.eulerAngles;
            newRot.x = Mathf.Sign(newRot.x) * xMaxRot;
        }
    }


    public void StartRotation() {
        if (!directionSet) {
            targetDirection = transform.localRotation.eulerAngles;
            directionSet = true;
        }
        //oldMousePos = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    public void Rotate() {
        if (Cursor.lockState == CursorLockMode.Locked) {
            var targetOrientation = Quaternion.Euler(targetDirection);
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
            _mouseAbsolute += _smoothMouse;
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            transform.localRotation *= targetOrientation;

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }


        //mousePos = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        //Vector3 diff = oldMousePos - mousePos;

        //diff = diff / 5;
        //Vector3 newRot = transform.eulerAngles;
        //newRot.x += diff.y;
        //newRot.y -= diff.x;
        //newRot.z = 0;
        //transform.eulerAngles = newRot;
        //oldMousePos = mousePos;
    }

    public virtual void Move(float vertical, float horizontal, float altitude) {

    }

    public virtual void AdjustPosition(Vector3 newCamPos) {

    }
}
