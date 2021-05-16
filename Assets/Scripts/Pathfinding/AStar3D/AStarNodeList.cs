using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AStarNodeList : ScriptableObject
{
    [SerializeField]
    private List<AStarNode> _nodes;

    public void SetList(List<AStarNode> nodes)
    {
        _nodes = nodes;
        EditorUtility.SetDirty(this);
    }

    public List<AStarNode> GetList()
    {
        return _nodes;
    }
}
