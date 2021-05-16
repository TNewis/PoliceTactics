using System.Collections.Generic;
using UnityEngine;

public class RoadPath :ScriptableObject
{
    [SerializeField]
    private List<RoadWaypointNode> _path;

    public RoadPath()
    {
        _path = new List<RoadWaypointNode>();
    }

    public void AddWaypoint(RoadWaypointNode node)
    {
        _path.Add(node);
    }

    public int NodeCount()
    {
        return _path.Count;
    }

    public List<RoadWaypointNode> GetPath()
    {
        return _path;
    }

}
