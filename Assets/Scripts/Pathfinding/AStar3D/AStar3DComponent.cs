using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar3DComponent : MonoBehaviour
{
    public AStar3dPathing pathfinder;

    public bool allowEarlyExit;

    protected I3DMovementController _movementController;

    public void Start()
    {
        if (pathfinder == null)
        {
            pathfinder=FindObjectOfType<AStar3dPathing>();
        }

        if (_movementController == null)
        {
            var controller = gameObject.GetComponent<I3DMovementController>();
            _movementController = controller ?? throw new Exception("AStar 3D component attached to "+ gameObject.name +" could not find a component that implements I3DMovementController");
        }
    }

    public void GetPathtoTarget(Vector3 end)
    {
        var currentPosition=gameObject.transform.position;

        pathfinder.FindPathTrackable(currentPosition, end, allowEarlyExit);

    }
}
