using UnityEngine;
using UnityEngine.AI;

public class PedestrianTargetFinder : MonoBehaviour
{
    private Vector3 _target;
    private NavMeshAgent _navAgent;
    private PedestrianManager _pedestrianManager;

    private float _targetProximityDistance = 2.0f;

    private void Start()
    {
        if (_navAgent == null)
        {
            _navAgent = gameObject.GetComponent<NavMeshAgent>();
        }

        if (_pedestrianManager == null)
        {
            _pedestrianManager = FindObjectOfType<PedestrianManager>();
        }

        SetNewRandomTarget();
    }

    void Update()
    {
        if (Vector3.Distance(gameObject.transform.position, _target) < _targetProximityDistance)
        {
            SetNewRandomTarget();
        }
    }

    private void SetNewRandomTarget()
    {
        _target = _pedestrianManager.GetRandomTarget();
        _navAgent.SetDestination(_target);
    }
}
