using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoadEntranceNode : RoadNode
{
    [SerializeField]
    private bool _hasAssociatedExitNode;

    [SerializeField]
    private DictionaryExitNodePath _reachableExitNodes;

    public TrafficLightEnum TrafficLightState= TrafficLightEnum.GREEN;

    public override void Initialise(Vector3 position)
    {
        _position = position;
        _reachableExitNodes = new DictionaryExitNodePath();
    }

    public Dictionary<RoadExitNode, RoadPath> GetReachableExitNodes()
    {
        return _reachableExitNodes;
    }

    public RoadPath AddReachableExitNode(RoadExitNode exitNode)
    {
        var blankPath = CreateInstance<RoadPath>();
        _reachableExitNodes.Add(exitNode, blankPath);
        return blankPath;
    }

    public RoadPath AddReachableExitNode(RoadExitNode exitNode, RoadPath path)
    {
        _reachableExitNodes.Add(exitNode, path);
        return path;
    }

    public void ReplaceReachableExitNode(RoadExitNode exitNode)
    {
        //todo: this doesn't do the actual path waypoints. At the time of typing, those don't exist yet anyway.
        var pair = _reachableExitNodes.FirstOrDefault(n => n.Key.GetPosition() == exitNode.GetPosition());
        if (pair.Key == null)
        {
            return;
        }
        RemoveReachableExitNode(pair.Key);

        AddReachableExitNode(exitNode);
    }

    public RoadPath GetPath(RoadExitNode node)
    {
        _reachableExitNodes.TryGetValue(node, out RoadPath path);
        return path;
    }

    public void RemoveReachableExitNode(RoadExitNode exitNode)
    {
        if (_reachableExitNodes.ContainsKey(exitNode))
        {
            _reachableExitNodes.Remove(exitNode);
        }
    }

    public void SetHasAssociatedExitNode(bool hasNode)
    {
        _hasAssociatedExitNode = hasNode;
    }

    public bool GetHasAssociatedExitNode()
    {
        return _hasAssociatedExitNode;
    }
}