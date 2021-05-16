using UnityEngine;

public class Basic3DMovement : MonoBehaviour, I3DMovementController
{
    public float Thrust = 20.0f;
    public float RotationalDamp = 5.0f;

    public void MoveTowards(Vector3 targetPoint)
    {
        LookAtTargetNonPhysics(targetPoint);
        MoveForwardsNonPhysics();
    }

    public void TurnTowards()
    {
        throw new System.NotImplementedException();
    }

    void LookAtTargetNonPhysics(Vector3 targetPoint)
    {
        Vector3 pos = targetPoint - transform.position;
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationalDamp * Time.deltaTime);
    }

    void MoveForwardsNonPhysics()
    {
        transform.position += transform.forward * Thrust * Time.deltaTime;
    }

}
