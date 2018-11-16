using UnityEngine;
using System.Collections;

public class TankShell : MonoBehaviour {

    public Vehicle source;

	// Use this for initialization
	void Start () {
	    if(source == null) {
            Destroy(gameObject);
        } else {
            if(source.side == Vehicle.vehicleSide.Player) {
                gameObject.layer = GameLogic.Instance.playerLayer;
            } else {
                gameObject.layer = GameLogic.Instance.enemyLayer;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision col) {
        
        
        

        if (col.transform.tag == "terrain") {
            Debug.Log("hit ground");
            Destroy(gameObject);
        } else {
            IUnitController controller = col.gameObject.GetComponent<IUnitController>();
            Debug.Log("hit other, controller: " + controller);

            if (controller != null) {
                controller.getVehicle().DoDamage(source.damageMax, source.type);
            }
            Destroy(gameObject, 0.5f);
        }
    }
}
