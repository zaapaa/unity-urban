using UnityEngine;
using System.Collections;

public class WeaponControl : MonoBehaviour {
    private float shootTimer=0;
    protected float shootTime=1;
    public bool attacking=false;

    public bool turning = false;

    protected float turnSpeed = 2f;
    protected Transform target;

    protected float minRotation = 0.001f;

    public bool facingTarget = false;

    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    protected void Update() {
        if (attacking) {
            //Debug.Log("shoot timer: " + shootTimer + ", shoot time" + shootTime);
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootTime+Random.Range(-0.2f,0.2f)) {
                //Debug.Log("shoot!");
                shootTimer = 0;
                Attack();
            }
        }
        if (turning) {
            Turn();
        }
        if(target == null) {
            StopAttacking();
        }
    }

    public void StartAttacking(Transform target) {
        this.target = target;
        attacking = true;
        Attack();
    }

    public void StopAttacking() {
        attacking = false;
    }

    protected virtual void Attack() {

    }

    public void StartTurning(Transform target) {
        turning = true;
        this.target = target;
    }

    public void stopTurning() {
        turning = false;
    }

    public virtual void Turn() {
        Vector3 relativePos = target.position - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(relativePos, Vector3.up);
        float rotationDiffY = transform.rotation.eulerAngles.y - rotationToTarget.eulerAngles.y;
        //float leftright = AngleDir(transform.forward, relativePos, transform.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToTarget, Time.deltaTime);

        if (Mathf.Abs(rotationDiffY) < minRotation) {
            facingTarget = true;
        } else {
            facingTarget = false;
        }
    }


    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f) {
            return dir;
        } else if (dir < 0f) {
            return -dir;
        } else {
            return 0f;
        }
    }

    protected void SetValues(float shootTime, float turnSpeed, float minRot) {
        this.shootTime = shootTime;
        this.turnSpeed = turnSpeed;
        this.minRotation = minRot;
    }
}
