using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;

public class AIMovement : MonoBehaviour, IUnitController {

    private enum moveMode {
        Closest,
        ToPlayer,
        Command
    }

    public enum aggroLevels {
        Passive,
        Hold,
        Defensive,
        Aggressive
    }

    public enum commandPriority {
        Normal,
        FrontOfQueue,
        SetAsCurrent
    }

    private Vehicle.vehicleType vehicleType;

    public List<Command> commandQueue = new List<Command>();
    public Command currentCommand;

    public Transform target;
    public Vector3 targetPoint;
    private Vector3 noTarget = default(Vector3);

    private new Rigidbody rigidbody;
    private VehicleController controller;
    private WeaponControl attackController;

    moveMode aiMovementMode;
    public bool facingNextNode = false;
    public bool canSeeTarget = false;
    public bool targetReached = false;
    public bool moving = false;

    public float rotationDiffY;

    private float sightRange = 100f;
    private float attackRange = 40f;
    private float followRange = 10f;

    private float minYRotation = 2;
    private float minYRotationChange = 5;
    private float maxMoveRotation = 45;
    private float changeSpeed;
    private float speedLimitTurn = 2f;

    private float distanceNode = Mathf.Infinity;

    public aggroLevels aggroLevel = aggroLevels.Aggressive;
    public float followTime = 10f;
    private Transform aggroTarget;

    //pathfinding variables
    NavMeshAgent agent;
    Vector3 nextNode;

    //debugging variables
    public String distanceInfo;
    public String moreInfo;

    //Variables related to stuck detection
    float stuckTimer;
    float stuckTime = 5;
    Vector3 oldPos;
    float stuckThreshold = 0.1f;
    float stuckRotThreshold = 0.01f;

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        controller = GetComponent<VehicleController>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

    }

    // Use this for initialization
    void Start() {
        aiMovementMode = moveMode.Command;
        agent.updatePosition = false;
        agent.updateRotation = false;

        vehicleType = controller.vehicle.type;
        switch (vehicleType) {
            case Vehicle.vehicleType.Tank:
                attackController = GetComponentInChildren<TankTurretControl>();
                break;
            case Vehicle.vehicleType.Truck:
                break;
            case Vehicle.vehicleType.Heli:
                break;
            case Vehicle.vehicleType.Spy:
                break;
            case Vehicle.vehicleType.Base:
                break;
            default:
                attackController = GetComponentInChildren<WeaponControl>();
                break;
        }

        Vehicle v = GetComponent<VehicleController>().vehicle;

        attackRange = v.attackRange;
        sightRange = v.sightRange;

        oldPos = transform.position;
    }

    //void Update() {
    //    agent.nextPosition = transform.position;
    //}

    void FixedUpdate() {
        //Commands
        if (currentCommand == null) {
            NextCommand();
            return;
        } else {
            if (currentCommand.targetType == Command.targetTypes.Unit) {
                if (target == null) {
                    NextCommand();
                    return;
                }
                targetPoint = target.position;
                //check if target is moving and do path recalculation
                if (GameLogic.Instance.GetController(currentCommand.target).isMoving()) {
                    RecalcPath();
                }
            }

            switch (currentCommand.commandType) { //do stuff every frame while having command
                case Command.commandTypes.Stay:
                    if (commandQueue.Count > 0) {
                        NextCommand();
                    }
                    break;
                case Command.commandTypes.AttackTarget:
                    break;
                case Command.commandTypes.AttackMove:
                    Vehicle newTarget;
                    if (GetNearestObject(out newTarget, sightRange)) {
                        Debug.Log("attackmove new target");
                        Command attackCommand = new Command(Command.commandTypes.AttackTarget, newTarget);
                        Command oldCommand = new Command(currentCommand);
                        AddCommand(attackCommand, commandPriority.SetAsCurrent);
                        AddCommand(oldCommand, commandPriority.FrontOfQueue);
                    }
                    break;
                case Command.commandTypes.Move:
                    break;
                case Command.commandTypes.Follow:
                    if (commandQueue.Count > 0) {
                        NextCommand();
                    }
                    break;
                default:
                    break;
            }
        }

        if (currentCommand.targetType == Command.targetTypes.Unit && (target == null || target == transform)) {
            NextCommand();
        }

        moreInfo = "";
        changeSpeed = Time.deltaTime * 5;

        if (targetPoint == noTarget) {
            switch (aiMovementMode) {
                case moveMode.Closest:
                    Vehicle newTarget;
                    GetNearestObject(out newTarget);
                    target = newTarget.vehicleObject.transform;
                    break;
                case moveMode.ToPlayer:
                    target = GameLogic.Instance.player.transform;
                    break;
                case moveMode.Command:
                    break;
                default:
                    break;
            }
            agent.enabled = false;
            //controller.vertical = -1;
        }

        if (targetPoint != noTarget) {

            //pathfinding stuff
            agent.enabled = true;
            if (!agent.hasPath && !agent.pathPending) {
                RecalcPath();
            }
            if (agent.path.corners.Length > 1) {
                nextNode = agent.path.corners[1];
            } else if (!targetReached && currentCommand.commandType != Command.commandTypes.Stay) {
                Debug.LogWarning("NO PATH");
            }

            //Stuck detection
            if (Vector3.Distance(oldPos, transform.position) > stuckRotThreshold) {
                moving = true;
            } else {
                moving = false;
            }
            if (!targetReached && currentCommand.commandType != Command.commandTypes.Stay) {
                float AngularVelY = rigidbody.angularVelocity.y;
                if (!moving && AngularVelY < stuckRotThreshold) {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > stuckTime) {
                        Debug.Log(gameObject.name + " was stuck");
                        stuckTimer = 0;
                        agent.enabled = false;
                        return;
                    }
                } else {
                    stuckTimer = 0;
                }
            }



            oldPos = transform.position;

        }

        if (currentCommand.commandType != Command.commandTypes.Stay) {
            if (!facingNextNode && targetPoint != noTarget) {
                TurnToTarget();
            }

            if (facingNextNode && targetPoint != noTarget) {
                CheckRotation();
                MoveToTarget();
            }
        }

        if (targetReached) {
            if (currentCommand.commandType == Command.commandTypes.AttackTarget) {
                controller.vertical = -1;
            } else {
                NextCommand();
            }
        }
        if ((!targetReached || currentCommand.commandType == Command.commandTypes.Stay) && currentCommand.commandType != Command.commandTypes.AttackTarget) {
            Vehicle aggroVehicle = null;
            switch (aggroLevel) {
                case aggroLevels.Passive:
                    break;
                case aggroLevels.Hold:
                    if (aggroVehicle != null || GetNearestObject(out aggroVehicle, sightRange)) {
                        aggroTarget = aggroVehicle.vehicleObject.transform;
                    }
                    break;
                case aggroLevels.Defensive: //TODO: new command when receiving hit
                    break;
                case aggroLevels.Aggressive:
                    if (aggroVehicle != null || GetNearestObject(out aggroVehicle, sightRange)) {
                        if (currentCommand.commandType == Command.commandTypes.Stay) {
                            Command newcmd = new Command(Command.commandTypes.AttackTarget, aggroVehicle);
                            AddCommand(newcmd, commandPriority.SetAsCurrent);
                        } else {
                            aggroTarget = aggroVehicle.vehicleObject.transform;
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        if (aggroTarget != null) {
            float distance = Vector3.Distance(aggroTarget.position, transform.position);
            if (distance < sightRange && attackController.facingTarget && !attackController.attacking) {
                if (distance < attackRange) {
                    attackController.StartAttacking(aggroTarget);
                }
            } else if (distance < sightRange && !attackController.facingTarget) {
                attackController.StartTurning(aggroTarget);
            }
        }


        for (int i = 0; i < agent.path.corners.Length - 1; i++) {
            Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.red);
        }



        agent.nextPosition = transform.position;

        moreInfo += "target reached: " + targetReached + " LOS to target: " + checkTargetLOS() + ", stucktimer:" + stuckTimer + ", path node count: " + agent.path.corners.Length;

    }

    private void RecalcPath() {
        agent.enabled = true;
        if (currentCommand.commandType != Command.commandTypes.Stay) {
            agent.SetDestination(targetPoint);
            //Debug.Log("path calc");
        }
    }

    public bool isMoving() {
        return moving;
    }

    public Vehicle getVehicle() {
        return GetComponent<VehicleController>().vehicle;
    }

    private void TurnToTarget() {

        Vector3 relativePos = nextNode - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(relativePos, Vector3.up);
        rotationDiffY = transform.rotation.eulerAngles.y - rotationToTarget.eulerAngles.y;
        if (Mathf.Abs(rotationDiffY) > minYRotation) {

            float leftright = AngleDir(transform.forward, relativePos, transform.up);

            controller.horizontal = Mathf.Lerp(controller.horizontal, leftright, changeSpeed);

            if (Mathf.Abs(rotationDiffY) < maxMoveRotation) {
                MoveToTarget();
            } else {
                if (controller.localVel.z > speedLimitTurn) {
                    controller.vertical = -1f;
                } else {
                    controller.vertical = 0f;
                }
            }
        } else {
            facingNextNode = true;
            controller.horizontal = 0f;
        }
        //Debug.DrawRay(target.position, relativePos, Color.blue);
        //Debug.DrawRay(target.position, rotationToTarget.eulerAngles, Color.magenta);
        //Debug.DrawRay(target.position, transform.forward, Color.red);


    }
    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f) {
            return Mathf.Clamp(Mathf.Abs(dir), 0.8f, 1.2f);
        } else if (dir < 0f) {
            return Mathf.Clamp(Mathf.Abs(dir), 0.8f, 1.2f) * -1;
        } else {
            return 0f;
        }
    }

    private void CheckRotation() {
        Vector3 relativePos = nextNode - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(relativePos, Vector3.up);
        float rotationDiffY = transform.rotation.eulerAngles.y - rotationToTarget.eulerAngles.y;
        if (Mathf.Abs(rotationDiffY) > minYRotationChange) {
            facingNextNode = false;
            TurnToTarget();
        }
    }

    private void MoveToTarget() {
        distanceInfo = "";
        distanceNode = Vector3.Distance(nextNode, transform.position);
        Vector3 ownPosSameY = transform.position;
        ownPosSameY.y = targetPoint.y;
        float distanceTarget = Vector3.Distance(targetPoint, ownPosSameY);
        if (!targetReached) {
            controller.vertical = 1;

            switch (currentCommand.commandType) {
                case Command.commandTypes.Stay:
                    controller.vertical = -1;
                    break;
                case Command.commandTypes.AttackTarget:
                    if (distanceTarget < attackRange && checkTargetLOS()) {
                        controller.vertical = -1;
                        targetReached = true;
                    }
                    break;
                case Command.commandTypes.AttackMove:
                    if (distanceTarget < attackRange) {
                        controller.vertical = -1;
                        targetReached = true;
                    }
                    break;
                case Command.commandTypes.Move:
                    if (distanceTarget < attackRange) {
                        controller.vertical = -1;
                        targetReached = true;
                    }
                    break;
                case Command.commandTypes.Follow:
                    if (distanceTarget < attackRange && checkTargetLOS()) {
                        controller.vertical = -1;
                    }
                    break;
                default:
                    break;
            }
        }
        distanceInfo += "Distance to node: " + distanceNode + ", distance to target: " + distanceTarget;
    }

    public bool GetNearestObject(out Vehicle bestTarget, float maxDistance = Mathf.Infinity) {
        Vehicle.vehicleSide mySide = GetComponent<VehicleController>().vehicle.side;
        targetReached = false;
        bestTarget = null;
        List<Vehicle> vehicles = new List<Vehicle>();
        vehicles = GameLogic.Instance.GetVehicles();
        if (vehicles.Count <= 1) {
            return false;
        }
        float closest = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (var v in vehicles) {
            Vector3 vpos = v.vehicleObject.transform.position;
            float distance = Vector3.Distance(vpos, position);
            if (mySide != v.side && distance < maxDistance && distance < closest) {
                closest = distance;
                bestTarget = v;
            }
        }
        if (bestTarget == null) {
            return false;
        } else {
            return true;
        }
    }

    private bool checkTargetLOS() {
        if (target != null) {
            Vector3 direction = (target.position - transform.position);
            Ray ray = new Ray(transform.position, direction);
            //Debug.DrawRay(transform.position, direction, Color.green);
            RaycastHit hit;

            Vehicle.vehicleSide mySide = getVehicle().side;
            int layerMask;
            if (mySide == Vehicle.vehicleSide.Player) {
                layerMask = 1 << GameLogic.Instance.playerLayer;
            } else {
                layerMask = 1 << GameLogic.Instance.enemyLayer;
            }
            layerMask = ~layerMask;

            if (Physics.Raycast(ray, out hit, attackRange, layerMask)) {
                if (hit.collider.transform.root.gameObject == target.gameObject) {
                    //Debug.Log("LOS to target");
                    return true;
                } else {
                    //Debug.Log("hit something else: " + hit.collider.name + ", parent: " + hit.collider.transform.root.name + ", target: " + target.name);
                    return false;
                }
            } else {
                return false;
            }
        } else {
            return false;
        }

    }

    public void Reset() {
        stuckTimer = 0;
        facingNextNode = false;
        canSeeTarget = false;
        targetReached = false;
        target = null;
        controller.horizontal = 0f;
        controller.vertical = 0f;
        RecalcPath();
    }

    public void NextCommand() {
        targetReached = false;
        if (commandQueue.Count > 0) {
            currentCommand = commandQueue[0];
            commandQueue.RemoveAt(0);
        } else {
            currentCommand = new Command(Command.commandTypes.Stay, null, transform.position);
        }

        targetPoint = currentCommand.destination;

        if (currentCommand.targetType == Command.targetTypes.Point) {
            targetPoint = currentCommand.destination;
        } else if (currentCommand.targetType == Command.targetTypes.Unit) {
            target = currentCommand.target.vehicleObject.transform;
            targetPoint = target.position;
        }

        switch (currentCommand.commandType) { //do stuff when getting new command
            case Command.commandTypes.Stay:
                controller.vertical = -1;
                break;
            case Command.commandTypes.AttackTarget:
                controller.vertical = 0;
                aggroTarget = currentCommand.target.vehicleObject.transform;
                break;
            case Command.commandTypes.AttackMove:
                controller.vertical = 0;
                break;
            case Command.commandTypes.Move:
                controller.vertical = 0;
                attackRange = 5f;
                break;
            case Command.commandTypes.Follow:
                controller.vertical = 0;
                attackRange = followRange;
                break;
            default:
                break;
        }
        RecalcPath();
    }

    public void AddCommand(Command command, commandPriority priority = 0) {

        switch (priority) {
            case commandPriority.Normal:
                commandQueue.Add(command);
                break;
            case commandPriority.FrontOfQueue:
                commandQueue.Insert(0, command);
                break;
            case commandPriority.SetAsCurrent:
                commandQueue.Insert(0, command);
                NextCommand();
                break;
            default:
                Debug.Log("AddCommand invalid priority");
                break;
        }
    }

    public void ClearCommands() {
        commandQueue.Clear();
    }
}
