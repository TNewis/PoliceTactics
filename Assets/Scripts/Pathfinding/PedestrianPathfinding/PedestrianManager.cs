using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianManager : MonoBehaviour
{
    public List<Vector3> targetPositions;
    private System.Random _random = new System.Random();

    public Vector3 GetRandomTarget()
    {
        return targetPositions[_random.Next(targetPositions.Count)];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        foreach(Vector3 position in targetPositions)
        {
            Gizmos.DrawSphere(position, 2.0f);
        }
    }
}
