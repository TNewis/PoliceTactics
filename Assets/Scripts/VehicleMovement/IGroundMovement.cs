using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGroundMovement
{
    void MoveTowards(Vector3 targetPoint);
    void TurnTowards();
    void PauseMovement();
    void ResumeMovement();
}
