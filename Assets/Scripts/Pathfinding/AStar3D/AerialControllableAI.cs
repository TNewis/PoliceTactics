using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialControllableAI : AStar3DComponent, IPlayerControlMovementCommand
{
    private TrackablePath _path;
    private List<AStarNode> _nodes;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.green;

        if (_path?.path != null)
        {
            foreach (KeyValuePair<AStarNode, AStarNode> node in _path.path)
            {
                if (node.Value == null)
                {
                    continue;
                }
                Gizmos.DrawLine(node.Key.GetPosition(), node.Value.GetPosition());
            }
        }

        if (_path != null)
        {
            Gizmos.DrawSphere(_path.startNode.GetPosition(), 2.0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_path.endNode.GetPosition(), 2.0f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_path.nextNodeInPath.GetPosition(), 3.0f);
        }

    }

    void Update()
    {
        if (_nodes == null)
        {
            _nodes = pathfinder.GetNodes();
        }

        if (_path != null)
        {
            MoveAlongPath();
            return;
        }
    }

    private void MoveAlongPath()
    {
        _movementController.MoveTowards(_path.nextNodeInPath.GetPosition());

        if (Vector3.Distance(transform.position, _path.nextNodeInPath.GetPosition()) < _path.nextNodeInPath.GetSize() / 2)
        {
            _path.ReachedNextNode();
        }

        if (_path.IsReachedNodeDestination())
        {
            Debug.Log("Reached a destination!");
            _path = null;
        }
    }

    public void PlayerControlMoveToTargetPosition(Vector3 target)
    {
        _path = pathfinder.FindPathTrackable(transform.position, target, false);
    }
}
