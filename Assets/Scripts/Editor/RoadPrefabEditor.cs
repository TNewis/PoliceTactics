using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RoadPrefabEditor : EditorWindow
{
    private GameObject _segmentAsset;
    private RoadSegment _segmentComponent;
    private RoadSegment _sceneSampleSegment;
    private GameObject _instance;

    private GizmoMarker _marker;

    private Vector3 _point = new Vector3();
    private Vector3 _guiPos;
    private float _angle;

    private Texture2D _assetPreview;
    private string _previousScene;

    private bool _prefabSelected;
    private bool _prefabPreviewSceneShown;
    private bool _shouldReturnToPreviousScene;
    private bool _showAddNodesTab = true;
    private bool _addNodesTab = true;
    private bool _connectorListIsShowingSpawners = false;

    private RoadEntranceNode _selectedEntranceNode;
    private RoadSpawnNode _selectedSpawnNode;
    private RoadPath _selectedExitPath;

    private List<RoadPath> _pathsToSave = new List<RoadPath>();

    [MenuItem("Tools/Road Prefab Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(RoadPrefabEditor));
        window.titleContent = new GUIContent("Road Prefab Editor");
        window.Focus();
        window.Repaint();
    }

    private void Update()
    {
        if (_prefabSelected && !_prefabPreviewSceneShown)
        {
            ShowPrefabPreviewScene();
        }

        if (_shouldReturnToPreviousScene)
        {
            _shouldReturnToPreviousScene = false;
            EditorSceneManager.OpenScene(_previousScene);
        }

        _addNodesTab = _showAddNodesTab;

    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(5, 5, 60, 20), "Open"))
        {
            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", controlID);
        }

        string commandName = Event.current.commandName;
        if (commandName == "ObjectSelectorClosed")
        {
            var asset = (GameObject)EditorGUIUtility.GetObjectPickerObject();

            if (asset == null)
            {
                return;
            }

            _segmentAsset = asset;

            _assetPreview = AssetPreview.GetAssetPreview(_segmentAsset);
            _segmentComponent = _segmentAsset.GetComponent<RoadSegment>();
            _prefabSelected = true;
            if (_segmentComponent == null)
            {
                GUI.Label(new Rect(5, 20, 300, 30), "No RoadSegment component found.");
            }
        }

        if (_segmentAsset == null)
        {
            return;
        }

        if (GUI.Button(new Rect(5, 35, 80, 20), "Add Nodes"))
        {
            _showAddNodesTab = true;
        }

        if (GUI.Button(new Rect(90, 35, 80, 20), "Edit Paths"))
        {
            _showAddNodesTab = false;
        }

        if (_addNodesTab)
        {
            GUILayout.Space(60);
            _guiPos = EditorGUILayout.Vector3Field("Position", _guiPos);
            _angle = EditorGUILayout.FloatField("Angle", _angle);
            UpdateMarker(_guiPos, _angle);

            if (GUI.Button(new Rect(5, 120, 140, 20), "Add Attachment Node"))
            {
                AddAttachmentNode();
            }

            if (GUI.Button(new Rect(5, 145, 140, 20), "Add Entrance Node"))
            {
                AddEntranceNode();
                if (_selectedEntranceNode == null)
                {
                    _selectedEntranceNode = _segmentComponent.GetRoadEntranceNodes().First();
                }
            }

            if (GUI.Button(new Rect(5, 170, 140, 20), "Add Exit Node"))
            {
                AddExitNode();
            }

            if (GUI.Button(new Rect(5, 195, 140, 20), "Add Spawn Node"))
            {
                AddSpawnNode();
                if (_selectedSpawnNode == null)
                {
                    _selectedSpawnNode = _segmentComponent.GetRoadSpawnNodes().First();
                }
            }

            if (GUI.Button(new Rect(5, 220, 140, 20), "Add Despawn Node"))
            {
                AddDespawnNode();
            }

            if (GUI.Button(new Rect(5, 245, 100, 20), "Save & Exit"))
            {
                SaveAndExitPrefabEditor();
            }

            return;
        }


        if (_selectedSpawnNode == null)
        {
            var spawnNodes = _segmentComponent.GetRoadSpawnNodes();
            if (spawnNodes.Count != 0)
            {
                _selectedSpawnNode = spawnNodes.First();
            }


        }


        if (_selectedEntranceNode == null)
        {
            var entranceNodes = _segmentComponent.GetRoadEntranceNodes();
            if (entranceNodes.Count != 0)
            {
                _selectedEntranceNode = entranceNodes.First();
            }
        }

        if(_selectedEntranceNode==null && _selectedSpawnNode == null)
        {
            GUI.Label(new Rect(5, 20, 300, 30), "No entrance or spawn nodes found to connect.");
            return;
        }

        if (_connectorListIsShowingSpawners)
        {
            UpdateMarker(_selectedSpawnNode.GetPosition(), 0);
        }
        else
        {
            UpdateMarker(_selectedEntranceNode.GetPosition(), 0);
        }



        GUI.Label(new Rect(5, 60, 100, 30), "Entrance Node:");

        if (GUI.Button(new Rect(5, 80, 30, 20), "<"))
        {
            SelectPreviousEntranceNode();
        }

        if (_connectorListIsShowingSpawners)
        {
            GUI.Label(new Rect(35, 80, 100, 30), _selectedSpawnNode.GetPosition().ToString());
        }
        else
        {
            GUI.Label(new Rect(35, 80, 100, 30), _selectedEntranceNode.GetPosition().ToString());
        }



        if (GUI.Button(new Rect(145, 80, 30, 20), ">"))
        {
            SelectNextEntranceNode();
        }

        int pathCheckBoxX = 5;
        int pathCheckBoxY = 110;
        pathCheckBoxY = NodeConnectionCheckBoxes(pathCheckBoxX, pathCheckBoxY, _segmentComponent.GetRoadExitNodes());
        pathCheckBoxY += 30;
        pathCheckBoxY = NodeConnectionCheckBoxes(pathCheckBoxX, pathCheckBoxY, _segmentComponent.GetRoadDespawnNodes());

        EditorGUILayout.Space(pathCheckBoxY);

        _guiPos = EditorGUILayout.Vector3Field("Position", _guiPos);
        UpdateMarker(_guiPos, 0f);

        if (GUI.Button(new Rect(5, pathCheckBoxY+35, 100, 20), "Add Waypoint"))
        {
            CreatAndAddWaypointToPath(_guiPos);
        }
    }

    private void CreatAndAddWaypointToPath(Vector3 _guiWaypointPos)
    {
        var waypoint = new RoadWaypointNode();
        waypoint.Initialise(_guiWaypointPos);
        _selectedExitPath.AddWaypoint(waypoint);
    }

    private int NodeConnectionCheckBoxes<T>(int pathCheckBoxX, int pathCheckBoxY, List<T> roadExitNodes) where T : RoadExitNode
    {
        foreach (RoadExitNode node in roadExitNodes)
        {
            bool addToPaths;
            if (_connectorListIsShowingSpawners)
            {
                addToPaths = _selectedSpawnNode.GetReachableExitNodes().ContainsKey(node);
            }
            else
            {
                addToPaths = _selectedEntranceNode.GetReachableExitNodes().ContainsKey(node);
            }

            EditorGUI.BeginChangeCheck();
            addToPaths = GUI.Toggle(new Rect(pathCheckBoxX, pathCheckBoxY, 120, 20), addToPaths, node.GetPosition().ToString());

            if (EditorGUI.EndChangeCheck())
            {
                if (_connectorListIsShowingSpawners)
                {
                    if (addToPaths)
                    {
                        _pathsToSave.Add(_selectedSpawnNode.AddReachableExitNode(node));
                        continue;
                    }
                    var path = _selectedSpawnNode.GetPath(node);
                    _pathsToSave.Remove(path);
                    _selectedSpawnNode.RemoveReachableExitNode(node);
                }
                else
                {
                    if (addToPaths)
                    {
                        _pathsToSave.Add(_selectedEntranceNode.AddReachableExitNode(node));
                        continue;
                    }
                    var path = _selectedEntranceNode.GetPath(node);
                    _pathsToSave.Remove(path);
                    _selectedEntranceNode.RemoveReachableExitNode(node);
                }

            }

            if (addToPaths)
            {
                if (GUI.Button(new Rect(pathCheckBoxX+120, pathCheckBoxY, 80, 20), "Waypoints"))
                {
                    _selectedExitPath = _selectedEntranceNode.GetPath(node);
                }
            }

            pathCheckBoxY = pathCheckBoxY + 25;
        }

        return pathCheckBoxY;
    }

    private void SaveAndExitPrefabEditor()
    {
        foreach (RoadPath path in _pathsToSave)
        {
            AssetDatabase.AddObjectToAsset(path, _segmentAsset);
        }
        _pathsToSave.Clear();

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(_previousScene);
    }

    private void SelectNextEntranceNode()
    {
        List<RoadEntranceNode> entranceNodeList = _segmentComponent.GetRoadEntranceNodes();
        List<RoadSpawnNode> spawnNodeList = _segmentComponent.GetRoadSpawnNodes();

        int nextIndex;

        if (entranceNodeList.Count == 0 && spawnNodeList.Count == 0)
        {
            return;
        }
        else
        {
            if (entranceNodeList.Count == 0)
            {
                _connectorListIsShowingSpawners = true;
            }

            if (spawnNodeList.Count == 0)
            {
                _connectorListIsShowingSpawners = false;
            }
        }

        if (_connectorListIsShowingSpawners == false)
        {
            nextIndex = entranceNodeList.IndexOf(_selectedEntranceNode) + 1;

            if (nextIndex >= entranceNodeList.Count())
            {
                nextIndex = 0;

                if (spawnNodeList.Count > 0)
                {
                    _connectorListIsShowingSpawners = true;
                }
               
            }

            _selectedEntranceNode = entranceNodeList[nextIndex];
        }
        else
        {
            nextIndex = spawnNodeList.IndexOf(_selectedSpawnNode) + 1;

            if (nextIndex >= spawnNodeList.Count())
            {
                nextIndex = 0;
                if (entranceNodeList.Count > 0)
                {
                    _connectorListIsShowingSpawners = false;
                }
            }

            _selectedSpawnNode = spawnNodeList[nextIndex];
        }

    }

    private void SelectPreviousEntranceNode()
    {
        List<RoadEntranceNode> entranceNodeList = _segmentComponent.GetRoadEntranceNodes();
        List<RoadSpawnNode> spawnNodeList = _segmentComponent.GetRoadSpawnNodes();

        int previousIndex;

        if (entranceNodeList.Count == 0 && spawnNodeList.Count == 0)
        {
            return;
        }
        else
        {
            if (entranceNodeList.Count == 0)
            {
                _connectorListIsShowingSpawners = true;
            }

            if (spawnNodeList.Count == 0)
            {
                _connectorListIsShowingSpawners = false;
            }
        }

        if (_connectorListIsShowingSpawners)
        {
            previousIndex = spawnNodeList.IndexOf(_selectedSpawnNode) - 1;

            if (previousIndex < 0)
            {
                _connectorListIsShowingSpawners = false;
                previousIndex = entranceNodeList.Count() - 1;
            }
        }
        else
        {
            previousIndex = entranceNodeList.IndexOf(_selectedEntranceNode) - 1;

            if (previousIndex < 0)
            {
                _connectorListIsShowingSpawners = true;
                previousIndex = spawnNodeList.Count() - 1;
            }

        }

        if (_connectorListIsShowingSpawners)
        {
            _selectedSpawnNode = spawnNodeList[previousIndex];
        }
        else
        {
            _selectedEntranceNode = entranceNodeList[previousIndex];
        }

    }

    private void AddAttachmentNode()
    {
        _point = _guiPos;
        var node = AddNode<RoadAttachmentNode>(_point);
        node.SetAngle(_angle);
        _segmentComponent.AddAttachmentNode(node);
        _sceneSampleSegment.AddAttachmentNode(node);
    }

    private void AddEntranceNode()
    {
        _point = _guiPos;
        var node = AddNode<RoadEntranceNode>(_point);
        _segmentComponent.AddEntranceNode(node);
        _sceneSampleSegment.AddEntranceNode(node);
    }

    private void AddExitNode()
    {
        _point = _guiPos;
        var node = AddNode<RoadExitNode>(_point);
        _segmentComponent.AddExitNode(node);
        _sceneSampleSegment.AddExitNode(node);
    }

    private void AddSpawnNode()
    {
        _point = _guiPos;
        var node = AddNode<RoadSpawnNode>(_point);
        node.SetAngle(_angle);
        _segmentComponent.AddSpawnNode(node);
        _sceneSampleSegment.AddSpawnNode(node);
    }

    private void AddDespawnNode()
    {
        _point = _guiPos;
        var node = AddNode<RoadDespawnNode>(_point);
        _segmentComponent.AddDespawnNode(node);
        _sceneSampleSegment.AddDespawnNode(node);
    }

    private void UpdateMarker(Vector3 position, float angle)
    {
        if (_marker != null)
        {
            _marker.SetPosition(position);
            _marker.SetAngle(angle);
        }
    }

    private void OnDestroy()
    {
        if (_segmentAsset == null)
        {
            return;
        }
        EditorSceneManager.OpenScene(_previousScene);
    }

    private void ShowPrefabPreviewScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        _previousScene = EditorSceneManager.GetActiveScene().path;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        _instance = (GameObject)PrefabUtility.InstantiatePrefab(_segmentAsset, scene);
        _sceneSampleSegment = _instance.GetComponent<RoadSegment>();

        Selection.activeObject = _instance;
        SceneView.lastActiveSceneView.FrameSelected();

        _prefabPreviewSceneShown = true;

        var markerObj = new GameObject();
        _marker = markerObj.AddComponent<GizmoMarker>();
    }

    private T AddNode<T>(Vector3 position) where T : RoadNode
    {
        var node = CreateInstance<T>();
        node.Initialise(position);
        EditorUtility.SetDirty(node);

        AssetDatabase.AddObjectToAsset(node, _segmentAsset);
        return node;
    }
}
