using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AStarPathNodeGenerator : EditorWindow
{
    private AStar3dPathing _pathfinder;
    private float _volume;
    private float _maximumSizeNodes;
    private float _minimumSizeNodes;

    [MenuItem("Tools/A* 3D Node Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(AStarPathNodeGenerator));
        window.titleContent = new GUIContent("A* 3D Node Generator");
        window.Focus();
        window.Repaint();
    }

    private void Awake()
    {
        _pathfinder = FindObjectOfType<AStar3dPathing>();
        if (_pathfinder != null)
        {
            _volume = _pathfinder.NavigableAreaXSize * _pathfinder.NavigableAreaYSize * _pathfinder.NavigableAreaZSize;
            _maximumSizeNodes = _volume / (Mathf.Pow(_pathfinder.MaximumNodeSize, 3));
            _minimumSizeNodes = _volume / (Mathf.Pow(_pathfinder.MinimumNodeSize, 3));

            _pathfinder.Initialise();
        }
    }

    void OnGUI()
    {
        if (_pathfinder == null)
        {
            GUI.Label(new Rect(5, 5, 300, 20), "No pathfinder found. Add AStar3DPathing to scene to allow generation.");
            return;
        }

        GUI.Label(new Rect(5, 5, 300, 20), "A* Pathfinder Object: "+ _pathfinder.name);
        GUI.Label(new Rect(5, 20, 300, 20), "Pathfinding space volume: " + _volume);
        GUI.Label(new Rect(5, 35, 300, 20), "Potential maximum size nodes: " + _maximumSizeNodes);
        GUI.Label(new Rect(5, 50, 300, 20), "Potential minimum size nodes: " + _minimumSizeNodes);

        if (GUI.Button(new Rect(5, 65, 80, 20), "Generate"))
        {
            _pathfinder.CreateNodes();
            AssetDatabase.SaveAssets();
        }

        if (_pathfinder.GetNodes() != null)
        {
            GUI.Label(new Rect(5, 85, 300, 20), "Nodes in generated pathing volume: " + _pathfinder.GetNodes().Count);
        }
        else
        {
            GUI.Label(new Rect(5, 85, 300, 20), "No nodes generated in this pathfinder's volume.");
        }

    }
}
