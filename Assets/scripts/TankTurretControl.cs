using UnityEngine;
using System.Collections;

public class TankTurretControl : WeaponControl {

    float shellVelocity = 8000;
    public GameObject shellPrefab;
    GameObject barrel;


    void Awake() {
        barrel = transform.GetChild(0).gameObject;
        float n_shootTimer = 1f;
        float n_turnSpeed = 2f;
        float n_minRotation = 0.001f;
        SetValues(n_shootTimer, n_turnSpeed, n_minRotation);
    }

    protected override void Attack() {
        //Debug.Log("Shell spawned");
        GameObject shell = (GameObject)Instantiate(shellPrefab, barrel.transform.position, Quaternion.identity);
        Physics.IgnoreCollision(shell.GetComponent<Collider>(), barrel.GetComponent<Collider>());
        Physics.IgnoreCollision(shell.GetComponent<Collider>(), GetComponent<Collider>());
        shell.GetComponent<Rigidbody>().AddForce(transform.forward * shellVelocity, ForceMode.Impulse);
        shell.GetComponent<TankShell>().source = transform.root.GetComponent<VehicleController>().vehicle;
        GetComponentInParent<Rigidbody>().AddForce(-transform.forward * shellVelocity, ForceMode.Impulse);

    }

    public override void Turn() {
        Vector3 relativePos = target.position - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(relativePos, Vector3.up);
        float rotationDiffY = transform.rotation.eulerAngles.y - rotationToTarget.eulerAngles.y;
        //float leftright = AngleDir(transform.forward, relativePos, transform.up);

        Vector3 rot = rotationToTarget.eulerAngles;
        rot.x = 0;
        rot.z = 0;
        rotationToTarget = Quaternion.Euler(rot);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToTarget, Time.deltaTime);

        if (Mathf.Abs(rotationDiffY) < minRotation) {
            facingTarget = true;
        } else {
            facingTarget = false;
        }
    }

}
