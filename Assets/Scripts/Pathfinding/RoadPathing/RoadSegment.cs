using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class RoadSegment : MonoBehaviour
{
    [SerializeField]
    private List<RoadEntranceNode> _roadEntranceNodes = new List<RoadEntranceNode>();
    [SerializeField]
    private List<RoadExitNode> _roadExitNodes = new List<RoadExitNode>();
    [SerializeField]
    private List<RoadAttachmentNode> _roadAttachmentNodes = new List<RoadAttachmentNode>();
    [SerializeField]
    private List<RoadSpawnNode> _roadSpawnNodes = new List<RoadSpawnNode>();
    [SerializeField]
    private List<RoadDespawnNode> _roadDespawnNodes = new List<RoadDespawnNode>();
    [SerializeField]
    private float _currentRotation;

    private bool _isTrafficLightEnabled;

    private void Start()
    {
        var trafficLightController = GetComponent<AbstractTrafficLightController>();
        if (trafficLightController == null)
        {
            _isTrafficLightEnabled = false;
            return;
        }
        _isTrafficLightEnabled = true;
    }

    private void OnDrawGizmos()
    {
        foreach (RoadAttachmentNode attachNode in _roadAttachmentNodes)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(gameObject.transform.position + attachNode.GetPosition(), 2);
        }

        foreach (RoadEntranceNode entranceNode in _roadEntranceNodes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(gameObject.transform.position + entranceNode.GetPosition(), 2);

            if (entranceNode.TrafficLightState == TrafficLightEnum.GREEN && _isTrafficLightEnabled)
            {
                var aboveEntrance = entranceNode.GetPosition();
                aboveEntrance.y += 8;
                Gizmos.DrawSphere(gameObject.transform.position + aboveEntrance, 3);
            }

            foreach (KeyValuePair<RoadExitNode, RoadPath> pair in entranceNode.GetReachableExitNodes())
            {
                if (pair.Value == null)
                {
                    Gizmos.DrawLine(gameObject.transform.position + entranceNode.GetPosition(), gameObject.transform.position + pair.Key.GetPosition());
                    continue;
                }

                if (pair.Value.NodeCount() == 0)
                {
                    Gizmos.DrawLine(gameObject.transform.position + entranceNode.GetPosition(), gameObject.transform.position + pair.Key.GetPosition());
                    continue;
                }
                else
                {
                    var previousPos = entranceNode.GetPosition();
                    foreach (RoadWaypointNode waypoint in pair.Value.GetPath())
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(gameObject.transform.position + waypoint.GetPosition(), 2);
                        Gizmos.DrawLine(gameObject.transform.position + previousPos, gameObject.transform.position + waypoint.GetPosition());
                        previousPos = waypoint.GetPosition();
                    }
                    Gizmos.DrawLine(gameObject.transform.position + previousPos, gameObject.transform.position + pair.Key.GetPosition());
                }
            }
        }

        foreach (RoadExitNode exitNode in _roadExitNodes)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gameObject.transform.position + exitNode.GetPosition(), 2);
        }

        foreach (RoadSpawnNode spawnNode in _roadSpawnNodes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(gameObject.transform.position + spawnNode.GetPosition(), new Vector3(2,2,2));

            foreach (KeyValuePair<RoadExitNode, RoadPath> pair in spawnNode.GetReachableExitNodes())
            {
                if (pair.Value.NodeCount() == 0)
                {
                    Gizmos.DrawLine(gameObject.transform.position + spawnNode.GetPosition(), gameObject.transform.position + pair.Key.GetPosition());
                    continue;
                }
                else
                {
                    foreach (RoadWaypointNode waypoint in pair.Value.GetPath())
                    {

                    }
                }
            }

        }

        foreach (RoadDespawnNode despawnNode in _roadDespawnNodes)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(gameObject.transform.position + despawnNode.GetPosition(), new Vector3(2, 2, 2));
        }
    }

    public void RotateRoadSegment(float angle)
    {
        var eulerVector = new Vector3(0, angle, 0);
        gameObject.transform.Rotate(eulerVector, Space.Self);

        RotateNodes(_roadAttachmentNodes, angle);
        RotateNodes(_roadEntranceNodes, angle);
        RotateNodes(_roadExitNodes, angle);

        _currentRotation = _currentRotation + angle;
    }

    private void RotateNodes<T>(List<T> nodes, float angle) where T : RoadNode
    {
        foreach (RoadNode node in nodes)
        {
            node.SetPosition(Quaternion.Euler(0, angle, 0) * node.GetPosition());
        }
    }

    public GameObject CreateInstance(Vector3 position)
    {
        var instance = Instantiate(gameObject, position, new Quaternion());
        var segment = instance.GetComponent<RoadSegment>();

        var instancedAttachmentNodes = CreateInstancedList(_roadAttachmentNodes);
        var instancedExitNodes = CreateInstancedList(_roadExitNodes);
        var instancedEntranceNodes = CreateInstancedList(_roadEntranceNodes);
        var instancedSpawnNodes = CreateInstancedList(_roadSpawnNodes);
        var instancedDespawnNodes = CreateInstancedList(_roadDespawnNodes);

        foreach (RoadAttachmentNode node in instancedAttachmentNodes)
        {
            node.SetParent(instance);
        }

        foreach (RoadExitNode node in instancedExitNodes)
        {
            node.SetParent(instance);
        }

        foreach (RoadEntranceNode node in instancedEntranceNodes)
        {
            node.SetParent(instance);
        }

        foreach (RoadSpawnNode node in instancedSpawnNodes)
        {
            node.SetParent(instance);
        }

        foreach (RoadDespawnNode node in instancedDespawnNodes)
        {
            node.SetParent(instance);
        }

        segment.SetRoadAttachmentNodes(instancedAttachmentNodes);
        segment.SetRoadExitNodes(instancedExitNodes);
        segment.SetRoadEntranceNodes(instancedEntranceNodes);
        segment.SetRoadSpawnNodes(instancedSpawnNodes);
        segment.SetRoadDespawnNodes(instancedDespawnNodes);

        ReplaceEntranceNodeConnections(segment, segment.GetRoadEntranceNodes());
        ReplaceEntranceNodeConnections(segment, segment.GetRoadSpawnNodes());

        return instance;
    }

    private static void ReplaceEntranceNodeConnections<T>(RoadSegment segment, List<T> entranceNodes) where T : RoadEntranceNode
    {
        foreach (RoadEntranceNode entranceNode in entranceNodes)
        {
            foreach (RoadExitNode exitNode in segment.GetRoadExitNodes())
            {
                entranceNode.ReplaceReachableExitNode(exitNode);
            }

            foreach (RoadDespawnNode despawnNode in segment.GetRoadDespawnNodes())
            {
                //todo: despawn nodes are a child type of exit nodes. will this work straight up, or turn our despawn nodes into regular exit nodes?
                entranceNode.ReplaceReachableExitNode(despawnNode);
            }

        }
    }

    private List<T> CreateInstancedList<T>(List<T> nodes) where T : RoadNode
    {
        var nodeList = new List<T>();
        foreach (T node in nodes)
        {
            var newNode = Instantiate(node);
            nodeList.Add(newNode);
        }
        return nodeList;
    }

    public void AddAttachmentNode(RoadAttachmentNode node)
    {
        _roadAttachmentNodes.Add(node);
    }

    public void AddEntranceNode(RoadEntranceNode node)
    {
        _roadEntranceNodes.Add(node);
    }

    public void AddExitNode(RoadExitNode node)
    {
        _roadExitNodes.Add(node);
    }

    public void AddSpawnNode(RoadSpawnNode node)
    {
        _roadSpawnNodes.Add(node);
    }

    public void AddDespawnNode(RoadDespawnNode node)
    {
        _roadDespawnNodes.Add(node);
    }

    public List<RoadAttachmentNode> GetRoadAttachmentNodes()
    {
        return _roadAttachmentNodes;
    }

    public void SetRoadAttachmentNodes(List<RoadAttachmentNode> roadAttachmentNodes)
    {
        _roadAttachmentNodes = roadAttachmentNodes;
    }

    public List<RoadEntranceNode> GetRoadEntranceNodes()
    {
        return _roadEntranceNodes;
    }

    public void SetRoadEntranceNodes(List<RoadEntranceNode> roadEntranceNodes)
    {
        _roadEntranceNodes = roadEntranceNodes;
    }

    public List<RoadExitNode> GetRoadExitNodes()
    {
        return _roadExitNodes;
    }

    public void SetRoadExitNodes(List<RoadExitNode> roadExitNodes)
    {
        _roadExitNodes = roadExitNodes;
    }

    public List<RoadSpawnNode> GetRoadSpawnNodes()
    {
        return _roadSpawnNodes;
    }

    public void SetRoadSpawnNodes(List<RoadSpawnNode> roadSpawnNodes)
    {
        _roadSpawnNodes = roadSpawnNodes;
    }

    public List<RoadDespawnNode> GetRoadDespawnNodes()
    {
        return _roadDespawnNodes;
    }

    public void SetRoadDespawnNodes(List<RoadDespawnNode> roadDespawnNodes)
    {
        _roadDespawnNodes = roadDespawnNodes;
    }

    public float GetRotation()
    {
        return _currentRotation;
    }

    public bool GetIsTrafficLightEnabled()
    {
        return _isTrafficLightEnabled;
    }
}

public abstract class RoadNode : ScriptableObject
{
    [SerializeField]
    protected Vector3 _position;
    [SerializeField]
    private GameObject _parent;

    public virtual void Initialise(Vector3 position)
    {
        _position = position;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public void SetPosition(Vector3 position)
    {
        _position = position;
    }

    public void SetParent(GameObject parent)
    {
        _parent = parent;
    }

    public GameObject GetParent()
    {
        return _parent;
    }
}
