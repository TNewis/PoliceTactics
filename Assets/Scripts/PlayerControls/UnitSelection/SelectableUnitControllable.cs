using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnitControllable : SelectableUnit
{
    protected IPlayerControlMovementCommand _playerControllableUnitMovement;

    void Start()
    {
        //create cameo and set portrait to unitPortrait.
        _playerControllableUnitMovement = gameObject.GetComponent<IPlayerControlMovementCommand>();
    }

    public virtual void SetTargetMovementPosition(Vector3 targetPosition)
    {
        _playerControllableUnitMovement.PlayerControlMoveToTargetPosition(targetPosition);
    }

}
