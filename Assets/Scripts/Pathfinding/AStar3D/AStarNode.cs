using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class AStarNode : ScriptableObject
{
    [SerializeField]
    private Vector3 _position;
    [SerializeField]
    private float _size;
    [SerializeField]
    private DictionaryNodeFloat _neighbours;

    public void InitialiseNode(Vector3 position, float size)
    {
        _neighbours = new DictionaryNodeFloat();
        _position = position;
        _size = size;

        EditorUtility.SetDirty(this);
    }

    public void AddNeighbour(AStarNode node, DiagonalType diagonal= DiagonalType.None)
    {
        var weight = CalculateNeighbourWeight(node, diagonal);
        _neighbours.Add(node, weight);
        EditorUtility.SetDirty(this);
    }

    public void AddNeighbourReciprocal(AStarNode node, DiagonalType diagonal = DiagonalType.None)
    {
        var weight = CalculateNeighbourWeight(node, diagonal);
        _neighbours.Add(node, weight);
        node.AddNeighbour(this, diagonal);
        EditorUtility.SetDirty(this);
    }

    private float CalculateNeighbourWeight(AStarNode node, DiagonalType diagonal)
    {
        float weight;
        switch (diagonal)
        {
            case DiagonalType.Diagonal2D:
                weight=node.GetSize() * Mathf.Sqrt(2);
                break;
            case DiagonalType.Diagonal3D:
                weight = node.GetSize() * Mathf.Sqrt(3);
                break;
            case DiagonalType.None:
            default:
                weight = node.GetSize();
                break;
        }

        return weight;
    }

    public DictionaryNodeFloat GetNeighbours()
    {
        return _neighbours;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public float GetSize()
    {
        return _size;
    }
}