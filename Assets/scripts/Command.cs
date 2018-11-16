using UnityEngine;
using System.Collections;

public class Command {

    public enum commandTypes {
        Stay,
        AttackTarget,
        AttackMove,
        Move,
        Follow
    }
    public enum targetTypes {
        Unit,
        Point
    }

    public commandTypes commandType;
    public Vehicle target;
    public Vector3 destination;

    public targetTypes targetType;

    public Command(Command.commandTypes t, Vehicle target = null, Vector3 destination = default(Vector3)) {
        commandType = t;
        this.target = target;
        this.destination = destination;

        if (target == null && destination != default(Vector3)) {
            targetType = targetTypes.Point;
        } else if(target!=null) {
            targetType = targetTypes.Unit;
            this.destination = target.vehicleObject.transform.position;
        } else {
            Debug.LogError("Command created with invalid parameters");
            throw new UnityException();
        }
    }

    public Command(Command old) {
        commandType = old.commandType;
        target = old.target;
        destination = old.destination;
        targetType = old.targetType;
    }

    public override string ToString() {
        string targetName = target == null ? "no target" : target.vehicleObject.name;
        return "type:" + commandType + " targetType: " + targetType + " target: " + targetName + " destination: " + destination;
    }
}
