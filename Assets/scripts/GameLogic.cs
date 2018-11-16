using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameLogic : MonoBehaviour {

    public static GameLogic Instance = null;
    public GameObject player;
    public GameObject enemy;
    public GameObject fogOfWar;

    public GameObject tankPrefab;
    public GameObject truckPrefab;
    //etc

    public List<Vehicle> vehicleTypes;
    public List<Vehicle> vehicles;
    public List<GameObject> bases;
    public List<GameObject> selectedObjects;
    public List<GameObject> powerStations;
    public GameObject hoverObject;

    public Text energyText;
    public Text distanceText;

    public Text wheelInfoText;
    public Text frictionInfoText;
    public Text controllerInfoText;
    public Text AIInfoText;
    public Text helpText;

    public int playerLayer = 10;
    public int enemyLayer = 11;


    public enum cameraMode {
        Unit,
        Free,
        Base
    }

    public Camera cam;
    public cameraMode camMode;
    FreeCameraMovement camControllerFree;
    BaseCamController camControllerBase;

    Vector3 mousePos;

    float horizontal;
    float vertical;
    float altitude;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
        vehicleTypes = new List<Vehicle>();
        vehicles = new List<Vehicle>();
        bases = new List<GameObject>();
        fogOfWar.SetActive(true);
    }

    // Use this for initialization
    void Start() {
        camControllerFree = cam.GetComponent<FreeCameraMovement>();
        camControllerBase = cam.GetComponent<BaseCamController>();
        changeCam(cameraMode.Free, null, true);

        bases.Add(player);
        bases.Add(enemy);

        vehicles.Add(player.GetComponent<BaseController>().baseVehicle);
        vehicles.Add(enemy.GetComponent<BaseController>().baseVehicle);

        Vehicle tank = new Vehicle(tankPrefab, Vehicle.vehicleType.Tank);
        vehicleTypes.Add(tank);

        AIInfoText.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        handleInput();
        updateUI();
    }

    private void updateUI() {
        energyText.text = "ENERGY: ";
        foreach(GameObject b in bases) {
            if(b.GetComponent<BaseController>().side == Vehicle.vehicleSide.Player) {
                energyText.text += b.GetComponent<BaseController>().energy.ToString();
            }
        }
    }

    private void handleInput() {

        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        altitude = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetMouseButtonDown(0)) {
            if (camMode == cameraMode.Free) {
                if (hoverObject != null) {
                    if (hoverObject.GetComponent<UnitHighlight>().selected) {
                        selectedObjects.Remove(hoverObject);
                        hoverObject.GetComponent<UnitHighlight>().Unselect();
                    } else {
                        selectedObjects.Add(hoverObject);
                        if (selectedObjects.Count > 1) {
                            hoverObject.GetComponent<UnitHighlight>().glowTimer = selectedObjects[0].GetComponent<UnitHighlight>().glowTimer;
                        }
                        hoverObject.GetComponent<UnitHighlight>().Select();
                    }
                }
            } else if (camMode == cameraMode.Base) {
                player.GetComponent<BaseController>().SummonCurrentObject();
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            if (camMode == cameraMode.Free) {
                if (selectedObjects.Count > 0) {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 300)) {
                        foreach (GameObject o in selectedObjects) {
                            Command newCommand;
                            if (hit.collider.tag == "terrain") {
                                newCommand = new Command(Command.commandTypes.Move, null, hit.point);
                                if (Input.GetKey(KeyCode.LeftControl)) {
                                    newCommand.commandType = Command.commandTypes.AttackMove;
                                }
                            } else {
                                newCommand = new Command(Command.commandTypes.AttackTarget, GetVehicleFromTransform(hit.transform.root));
                            }
                            AIMovement.commandPriority p = AIMovement.commandPriority.Normal;
                            if (!Input.GetKey(KeyCode.LeftShift)) {
                                o.GetComponent<AIMovement>().commandQueue.Clear();
                                p = AIMovement.commandPriority.SetAsCurrent;
                            }
                           
                            o.GetComponent<AIMovement>().AddCommand(newCommand, p);
                        }
                    }


                }
            }
        }

        if (Input.GetMouseButtonDown(2)) {
            if (camMode == cameraMode.Free) {
                camControllerFree.StartRotation();
            } else if (camMode == cameraMode.Base) {
                camControllerBase.StartRotation();
            }
            Invoke("lockCursor", Time.deltaTime);
            Cursor.visible = false;
        }
        if (Input.GetMouseButton(2)) {
            if (camMode == cameraMode.Free) {
                camControllerFree.Rotate();
            } else if (camMode == cameraMode.Base) {
                camControllerBase.Rotate();
            }
        }
        if (Input.GetMouseButtonUp(2)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (camMode == cameraMode.Free) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                camControllerFree.moveSpeed = camControllerFree.origMoveSpeed * 3;
            } else if (Input.GetKey(KeyCode.LeftControl)) {
                camControllerFree.moveSpeed = camControllerFree.origMoveSpeed / 3;
            } else {
                camControllerFree.moveSpeed = camControllerFree.origMoveSpeed;
            }

            camControllerFree.Move(vertical, horizontal, altitude);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (camMode == cameraMode.Free) {
                if (selectedObjects.Count > 0) {
                    Vector3 average = Vector3.zero;
                    foreach (GameObject s in selectedObjects) {
                        average += s.transform.position;
                    }
                    average /= selectedObjects.Count;
                    camControllerFree.AdjustPosition(average);
                } else {
                    camControllerFree.AdjustPosition(player.transform.position);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (camMode == cameraMode.Free) {
                changeCam(cameraMode.Base, null);
            } else if (camMode == cameraMode.Base) {
                changeCam(cameraMode.Free, null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (selectedObjects.Count > 0) {
                foreach (GameObject o in selectedObjects) {
                    o.GetComponent<UnitHighlight>().Unselect();
                }
                selectedObjects.Clear();
            }
        }
        if (Input.GetKeyDown(KeyCode.F1)) {
            helpText.enabled = !helpText.enabled;
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            AIInfoText.enabled = !AIInfoText.enabled;
            GetComponent<AIDebugInfos>().enabled = !GetComponent<AIDebugInfos>().enabled;
        }
        if (Input.GetKeyDown(KeyCode.F3)) {
            foreach(GameObject o in powerStations) {
                o.GetComponent<PowerStation>().moreRegen();
            }
        }
        if (Input.GetKeyDown(KeyCode.F4)) {
            foreach (GameObject o in powerStations) {
                o.GetComponent<PowerStation>().lessRegen();
            }
        }
        if (Input.GetKeyDown(KeyCode.F5)) {
            Application.Quit();
        }
    }

    private void lockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void changeCam(cameraMode newMode, Transform newUnit, bool isStart = false) {
        camMode = newMode;
        switch (newMode) {
            case cameraMode.Unit:
                throw new NotImplementedException();
            case cameraMode.Free:
                camControllerBase.enabled = false;
                camControllerFree.enabled = true;
                if (!isStart) {
                    camControllerFree.AdjustPosition(cam.transform.position);
                }
                break;
            case cameraMode.Base:
                camControllerFree.enabled = false;
                camControllerBase.enabled = true;
                camControllerBase.AdjustPosition(player.transform.position);
                break;
            default:
                break;
        }
    }

    public Vehicle SummonVehicle(GameObject prefab, Vector3 position, Quaternion rotation, Vehicle.vehicleType type, Vehicle.vehicleSide side = Vehicle.vehicleSide.Player) {
        GameObject newVehicleObj = (GameObject)Instantiate(prefab, position, rotation);
        Vehicle vehicle = new Vehicle(newVehicleObj, type, side);
        newVehicleObj.GetComponent<VehicleController>().vehicle = vehicle;
        vehicles.Add(vehicle);
        Debug.Log("added " + vehicle.vehicleObject.name);
        return vehicle;
    }
    public void RemoveVehicle(Vehicle vehicle) {
        vehicles.Remove(vehicle);
        if (selectedObjects.Contains(vehicle.vehicleObject)) {
            selectedObjects.Remove(vehicle.vehicleObject);
        }
        if (bases.Contains(vehicle.vehicleObject)) {
            bases.Remove(vehicle.vehicleObject);
        }
        
        Debug.Log("removed " + vehicle.vehicleObject.name);
    }
    public List<Vehicle> GetVehicles() {
        return vehicles;
    }
    

    public IUnitController GetController(Vehicle v) {
        switch (v.type) {
            case Vehicle.vehicleType.Tank:
                return v.vehicleObject.GetComponent<AIMovement>();
            case Vehicle.vehicleType.Truck:
                return null;
            case Vehicle.vehicleType.Heli:
                return null;
            case Vehicle.vehicleType.Spy:
                return null;
            case Vehicle.vehicleType.Base:
                return v.vehicleObject.GetComponent<BaseController>();
            case Vehicle.vehicleType.BaseTurret:
                return null;
            default:
                return null;
        }
    }

    public Vehicle GetVehicleFromTransform(Transform t) {
        if (t.GetComponent<VehicleController>()) {
            return t.GetComponent<VehicleController>().vehicle;
        } else if (t.GetComponent<BaseController>()) {
            return t.GetComponent<BaseController>().baseVehicle;
        } else {
            return null;
        }
    }
    
    public void setAllLayers(GameObject obj, int layer) {
        obj = obj.transform.root.gameObject;
        foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true)) {
            trans.gameObject.layer = layer;
        }
    }
}
