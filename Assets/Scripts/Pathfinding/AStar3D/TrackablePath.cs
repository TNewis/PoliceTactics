using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackablePath
{
    public Dictionary<AStarNode, AStarNode> path;
    public AStarNode currentNode;
    public AStarNode nextNodeInPath;
    public AStarNode endNode;
    public AStarNode startNode;

    public TrackablePath(AStarNode currentNode, AStarNode endNode, Dictionary<AStarNode, AStarNode> path)
    {
        this.path = path;
        this.currentNode = currentNode;
        this.endNode = endNode;

        try
        {
            nextNodeInPath = path.First(n => n.Value == currentNode).Key;
        }catch (ArgumentNullException e)
        {
            Debug.Log("ArgumentNullException");
        }

        startNode = currentNode;
    }

    public bool IsReachedNodeDestination()
    {
        if (currentNode == endNode)
        {
            return true;
        }

        return false;
    }

    public void ReachedNextNode()
    {
        currentNode = nextNodeInPath;

        nextNodeInPath = path.FirstOrDefault(n => n.Value == nextNodeInPath).Key;
        if (nextNodeInPath == null)
        {
            Debug.Log("Reached Destination");
        }
    }

}

