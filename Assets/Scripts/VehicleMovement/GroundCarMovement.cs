using UnityEngine;

public class GroundCarMovement : MonoBehaviour, IGroundMovement
{
    public float Thrust = 20.0f;
    public float RotationalDamp = 5.0f;

    [SerializeField]
    private LayerMask _blockingLayers;

    private bool _movementPaused;

    public void MoveTowards(Vector3 targetPoint)
    {
        if (_movementPaused)
        {
            return;
        }

        if (MovementBlocked())
        {
            return;
        }
        LookAtTargetHorizontalNonPhysics(targetPoint);
        MoveForwardsNonPhysics();
    }

    public void TurnTowards()
    {
        throw new System.NotImplementedException();
    }

    public void PauseMovement()
    {
        _movementPaused = true;
    }

    public void ResumeMovement()
    {
        _movementPaused = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward*7));
    }

    private bool MovementBlocked()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, 7, _blockingLayers))
        {
            return true;
        }
        return false;
    }

    private void LookAtTargetHorizontalNonPhysics(Vector3 targetPoint)
    {
        //this is an extremely basic car mover that doesn't use physics and will fuck up on hills
        Vector3 pos = targetPoint - transform.position;
        //pos.y = transform.position.y;
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationalDamp * Time.deltaTime);
    }

    private void MoveForwardsNonPhysics()
    {
        transform.position += transform.forward * Thrust * Time.deltaTime;
    }
}
