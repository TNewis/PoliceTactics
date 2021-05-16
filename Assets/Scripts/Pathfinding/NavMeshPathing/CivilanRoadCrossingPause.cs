using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CivilanRoadCrossingPause : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;

    public LayerMask _layers;


    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            throw new UnassignedReferenceException("Navmesh not assigned on road crossing AI.");
        }
    }

    void Update()
    {
        if (MovementBlocked())
        {
            _navMeshAgent.isStopped = true;
            return;
        }
        _navMeshAgent.isStopped = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward * 7));
    }

    private bool MovementBlocked()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, 7, _layers))
        {
            return true;
        }
        return false;
    }
}
