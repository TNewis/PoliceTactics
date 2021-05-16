using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;

public class RoadBuilderEditor : EditorWindow
{
    private readonly string _roadSegmentAssetPath = "Assets/Prefabs/Roads/RoadSegments";

    private List<RoadAttachmentNode> _openAttachmentNodes;
    private List<RoadAttachmentNode> _allAttachmentNodes;
    private RoadNetworkManager _roadNetworkManager;
    private RoadAttachmentNode _selectedAttachmentNode;

    private GameObject _roadPreviewSegment;
    private List<RoadAttachmentNode> _roadPreviewAttachmentNodes;
    private RoadAttachmentNode _currentRoadPreviewAttachmentNode;

    private List<GameObject> _roadSegmentAssets = new List<GameObject>();

    private bool _managerFoundInScene;

    private bool _setEditMode;
    private bool _editMode;

    private float _rotationAngle;

    private Vector2 _roadSelectorScrollPosition = new Vector2();

    [MenuItem("Tools/Road Builder")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(RoadBuilderEditor));
        window.titleContent = new GUIContent("Road Builder");
        window.Focus();
        window.Repaint();
    }

    private void OnDestroy()
    {
        GameObject.DestroyImmediate(_roadPreviewSegment);
    }

    public void Awake()
    {
        _roadNetworkManager = FindObjectOfType<RoadNetworkManager>();
        if (_roadNetworkManager != null)
        {
            _managerFoundInScene = true;
            UpdateAttachmentNodesLists();
        }
    }

    private void Update()
    {
        _editMode = _setEditMode;
    }

    private void OnGUI()
    {
        if (!_managerFoundInScene)
        {
            GUI.Label(new Rect(5, 5, 300, 20), "Road Network Manager not found in scene.");
            return;
        }

        if (_allAttachmentNodes.Count() == 0)
        {
            InitialRoadSelection();
            return;
        }

        if (GUI.Button(new Rect(5, 5, 60, 20), "Build"))
        {
            _setEditMode = false;
        }

        if (GUI.Button(new Rect(70, 5, 60, 20), "Edit"))
        {
            _setEditMode = true;
        }

        if (_openAttachmentNodes.Count() == 0)
        {
            GUI.Label(new Rect(5, 5, 300, 20), "No open nodes found. Is the road network complete?");
            return;
            //either
            //we're in a new road network and need to put down an initial road piece.
            //or a completed road network and need to disable road placement, but allow some actions such as deleting a section.
        }

        if (_editMode)
        {

        }
        else
        {
            if (GUI.Button(new Rect(5, 35, 30, 20), "<"))
            {
                SelectPreviousOpenAttachmentNode();
                MoveAndRotatePreviewToCurrentlySelectedAttachmentNode();
            }

            GUI.Label(new Rect(35, 35, 130, 20), _selectedAttachmentNode.GetPosition().ToString());

            if (GUI.Button(new Rect(150, 35, 30, 20), ">"))
            {
                SelectNextOpenAttachmentNode();
                MoveAndRotatePreviewToCurrentlySelectedAttachmentNode();
            }

            RoadSelection(5, 60, 5, 5, 300);
        }
    }

    private void MoveAndRotatePreviewToCurrentlySelectedAttachmentNode()
    {
        if (_roadPreviewSegment != null)
        {
            var totalRotate = -_roadPreviewSegment.GetComponent<RoadSegment>().GetRotation() + (_currentRoadPreviewAttachmentNode.GetAngle() + 180.0f) - (_selectedAttachmentNode.GetAngle() - _selectedAttachmentNode.GetParent().GetComponent<RoadSegment>().GetRotation());
            _roadPreviewSegment.GetComponent<RoadSegment>().RotateRoadSegment(totalRotate);
            _roadPreviewSegment.transform.position = (_selectedAttachmentNode.GetParent().transform.position + _selectedAttachmentNode.GetPosition()) - _currentRoadPreviewAttachmentNode.GetPosition();
        }
    }

    private void RoadSelection(float x, float y, float spacingX, float spacingY, float height)
    {
        GetRoadSelectionPreview(x, y, spacingX, spacingY, height, _selectedAttachmentNode);

        if (GUI.Button(new Rect(x + 30, y + height, 20, 20), ">"))
        {
            _currentRoadPreviewAttachmentNode = RoadSelectorRotateAndMoveToAttachedNode(true, _roadPreviewSegment.GetComponent<RoadSegment>(), _roadPreviewAttachmentNodes, _currentRoadPreviewAttachmentNode);
        }

        if (GUI.Button(new Rect(x, y + height, 20, 20), "<"))
        {
            _currentRoadPreviewAttachmentNode = RoadSelectorRotateAndMoveToAttachedNode(false, _roadPreviewSegment.GetComponent<RoadSegment>(), _roadPreviewAttachmentNodes, _currentRoadPreviewAttachmentNode);
        }

        if (_roadPreviewSegment != null)
        {
            if (GUI.Button(new Rect(x, y + height + 30, 60, 20), "Confirm"))
            {
                _roadNetworkManager.AddRoadToNetwork(_roadPreviewSegment);
                _roadPreviewSegment = null;

                _selectedAttachmentNode.SetOccupied(true);
                UpdateAttachmentNodesLists();
            }
        }
    }

    private void InitialRoadSelection()
    {
        GetRoadSelectionPreview(5, 40, 5, 5, 300);

        if (_roadPreviewSegment != null)
        {
            var previousAngle = _rotationAngle;
            EditorGUI.BeginChangeCheck();
            _rotationAngle = EditorGUI.FloatField(new Rect(130, 15, 60, 20), _rotationAngle);

            if (EditorGUI.EndChangeCheck())
            {
                _roadPreviewSegment.GetComponent<RoadSegment>().RotateRoadSegment(_rotationAngle - previousAngle);
            }

            if (GUI.Button(new Rect(70, 15, 60, 20), "Confirm"))
            {
                _roadNetworkManager.AddRoadToNetwork(_roadPreviewSegment);
                _roadPreviewSegment = null;

                UpdateAttachmentNodesLists();
            }
        }
    }

    private void GetRoadSelectionPreview(float x, float y, float spacingX, float spacingY, float height, RoadAttachmentNode node = null)
    {
        var selectedRoad = RoadSelector(x, y, spacingX, spacingY, height);
        if (selectedRoad != null)
        {
            if (_roadPreviewSegment != null)
            {
                GameObject.DestroyImmediate(_roadPreviewSegment);
            }
            if (node == null)
            {
                _roadPreviewSegment = selectedRoad.GetComponent<RoadSegment>().CreateInstance(_roadNetworkManager.transform.position);
            }
            else
            {
                AttachNextSegment(node, selectedRoad);
            }

        }
    }

    private void AttachNextSegment(RoadAttachmentNode node, GameObject selectedRoad)
    {
        _roadPreviewSegment = selectedRoad.GetComponent<RoadSegment>().CreateInstance(node.GetParent().transform.position + node.GetPosition()); 

        var segment = _roadPreviewSegment.GetComponent<RoadSegment>();
        _roadPreviewAttachmentNodes = segment.GetRoadAttachmentNodes();
        _currentRoadPreviewAttachmentNode = _roadPreviewAttachmentNodes.First();

        _currentRoadPreviewAttachmentNode.SetOccupied(true);
        var totalRotate = (_currentRoadPreviewAttachmentNode.GetAngle() + 180.0f) - (node.GetAngle() - node.GetParent().GetComponent<RoadSegment>().GetRotation());
        segment.RotateRoadSegment(totalRotate);

        _roadPreviewSegment.transform.position = (_roadPreviewSegment.transform.position - _currentRoadPreviewAttachmentNode.GetPosition());

    }

    private RoadAttachmentNode RoadSelectorRotateAndMoveToAttachedNode(bool movePositive, RoadSegment segment, List<RoadAttachmentNode> attachmentNodes, RoadAttachmentNode currentNode)
    {
        currentNode.SetOccupied(false);
        _roadPreviewSegment.transform.position = (_roadPreviewSegment.transform.position + currentNode.GetPosition());
        segment.RotateRoadSegment(-currentNode.GetAngle() + 180f);

        if (movePositive)
        {
            if (attachmentNodes.IndexOf(currentNode) + 1 >= attachmentNodes.Count())
            {
                currentNode = attachmentNodes[0];
            }
            else
            {
                currentNode = attachmentNodes[attachmentNodes.IndexOf(currentNode) + 1];
            }
        }
        else
        {
            if (attachmentNodes.IndexOf(currentNode) - 1 < 0)
            {
                currentNode = attachmentNodes[attachmentNodes.Count() - 1];
            }
            else
            {
                currentNode = attachmentNodes[attachmentNodes.IndexOf(currentNode) - 1];
            }
        }


        currentNode.SetOccupied(true);

        segment.RotateRoadSegment(currentNode.GetAngle() + 180f);
        _roadPreviewSegment.transform.position = (_roadPreviewSegment.transform.position - currentNode.GetPosition());
        return currentNode;
    }

    private GameObject RoadSelector(float x, float y, float spacingX, float spacingY, float height)
    {
        int imageSize = 128;

        if (_roadSegmentAssets.Count() == 0)
        {
            LoadRoadSegments();
        }

        if (GUI.Button(new Rect(x, y, 60, 20), "Refresh"))
        {
            LoadRoadSegments();
        }

        var positionX = 0f;
        var positionY = 0f;

        var roadListColumns = Math.Max(Mathf.CeilToInt((this.position.width - x) / (imageSize + spacingX)), 1);

        var currentColumn = 1;

        Vector2 scrollPostion = Vector2.zero;

        Rect scrollRect = new Rect(x, y, this.position.width - x, height);

        float scrollAreaHeight;
        if (roadListColumns < 2)
        {
            scrollAreaHeight = (imageSize + spacingY) * _roadSegmentAssets.Count();
        }
        else
        {
            scrollAreaHeight = (imageSize + spacingY) * Mathf.CeilToInt((float)_roadSegmentAssets.Count() / (roadListColumns - 1));
        }


        Rect viewRect = new Rect(0, 0, 0, scrollAreaHeight);

        _roadSelectorScrollPosition = GUI.BeginScrollView(scrollRect, _roadSelectorScrollPosition, viewRect);

        foreach (GameObject road in _roadSegmentAssets)
        {
            var preview = AssetPreview.GetAssetPreview(road);
            if (GUI.Button(new Rect(positionX, positionY, imageSize, imageSize), preview))
            {
                return road;
            }

            currentColumn++;

            if (currentColumn % roadListColumns == 0)
            {
                currentColumn = 1;
                positionY = positionY + imageSize + spacingY;
                positionX = 0f;
            }
            else
            {
                positionX = positionX + imageSize + spacingX;
            }
        }
        GUI.EndScrollView();
        return null;
    }

    private void LoadRoadSegments()
    {
        _roadSegmentAssets.Clear();
        var assetPaths = Directory.GetFiles(_roadSegmentAssetPath);
        foreach (string path in assetPaths)
        {
            if (Path.GetExtension(path) == ".meta")
            {
                continue;
            }
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            _roadSegmentAssets.Add(asset);
        }
    }

    private void UpdateAttachmentNodesLists()
    {
        _allAttachmentNodes = _roadNetworkManager.GetAttachmentNodes();
        _openAttachmentNodes = _allAttachmentNodes.Where(n => n.GetOccupied() == false).ToList();

        if (_selectedAttachmentNode != null)
        {
            if (_selectedAttachmentNode.GetOccupied())
            {
                _selectedAttachmentNode = null;
            }
        }

        if (_selectedAttachmentNode == null && _openAttachmentNodes.Count() > 0)
        {
            _selectedAttachmentNode = _openAttachmentNodes.Last();
        }
    }

    private void SelectNextOpenAttachmentNode()
    {
        var nextIndex = _openAttachmentNodes.IndexOf(_selectedAttachmentNode) + 1;

        if (nextIndex >= _openAttachmentNodes.Count())
        {
            nextIndex = 0;
        }

        _selectedAttachmentNode = _openAttachmentNodes[nextIndex];
    }

    private void SelectPreviousOpenAttachmentNode()
    {
        var previousIndex = _openAttachmentNodes.IndexOf(_selectedAttachmentNode) - 1;

        if (previousIndex < 0)
        {
            previousIndex = _openAttachmentNodes.Count() - 1;
        }

        _selectedAttachmentNode = _openAttachmentNodes[previousIndex];
    }
}
