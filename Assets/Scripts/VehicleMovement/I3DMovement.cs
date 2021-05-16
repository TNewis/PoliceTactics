using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I3DMovementController
{
    void MoveTowards(Vector3 targetPoint);
    void TurnTowards();
}
