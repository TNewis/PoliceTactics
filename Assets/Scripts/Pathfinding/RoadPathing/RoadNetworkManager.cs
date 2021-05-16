using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadNetworkManager : MonoBehaviour
{
    [SerializeField]
    private List<RoadAttachmentNode> _attachmentNodes;
    [SerializeField]
    private List<RoadEntranceNode> _entranceNodes;
    [SerializeField]
    private List<RoadExitNode> _exitNodes;
    [SerializeField]
    private List<RoadSpawnNode> _spawnNodes;
    [SerializeField]
    private List<RoadDespawnNode> _despawnNodes;


    [SerializeField]
    private float _testSpawnInterval;
    [SerializeField]
    private GameObject _testVehicle;

    private void Start()
    {
        InvokeRepeating(nameof(TestSpawn), 1.0f, _testSpawnInterval);
    }

    private void TestSpawn()
    {
        var spawn = _spawnNodes.First();
        var spawnPos = spawn.GetPosition() + spawn.GetParent().transform.position;
        spawnPos.y += 1.0f;
        var quat = Quaternion.Euler(0f, spawn.GetAngle()+spawn.GetParent().transform.eulerAngles.y+180, 0f);
        SpawnVehicle(_testVehicle, spawnPos, quat, spawn.GetReachableExitNodes().First().Key);
    }

    public void SpawnVehicle(GameObject vehicle, Vector3 spawnPoint, Quaternion spawnAngle, RoadExitNode initialTarget)
    {
        var vehicleClone=GameObject.Instantiate(vehicle, spawnPoint, spawnAngle);
        var vehicleAI= vehicleClone.GetComponent<ICivilianRoadAI>();
        vehicleAI.Initialise(this, initialTarget);
    }

    public void AddRoadToNetwork(GameObject road)
    {
        road.transform.parent = gameObject.transform;
        var roadSegment = road.GetComponent<RoadSegment>();

        ConnectAttachmentNodes(roadSegment);
        ConnectExitNodes(roadSegment);
        ConnectEntranceNodes(roadSegment);

        _attachmentNodes.AddRange(roadSegment.GetRoadAttachmentNodes());
        _entranceNodes.AddRange(roadSegment.GetRoadEntranceNodes());
        _exitNodes.AddRange(roadSegment.GetRoadExitNodes());
        _spawnNodes.AddRange(roadSegment.GetRoadSpawnNodes());
        _despawnNodes.AddRange(roadSegment.GetRoadDespawnNodes());
    }

    private void ConnectEntranceNodes(RoadSegment roadSegment)
    {
        foreach (RoadEntranceNode entranceNode in roadSegment.GetRoadEntranceNodes())
        {
            foreach (RoadExitNode exitNode in _exitNodes.Where(n => n.NextSegmentConnectedEntrance == null))
            {
                if (Vector3.Distance(exitNode.GetPosition() + exitNode.GetParent().transform.position, entranceNode.GetPosition() + entranceNode.GetParent().transform.position) < 0.05f)
                {
                    exitNode.NextSegmentConnectedEntrance = entranceNode;
                    exitNode.NextSegment = roadSegment;
                    entranceNode.SetHasAssociatedExitNode(true);
                }
            }
        }
    }

    private void ConnectExitNodes(RoadSegment roadSegment)
    {
        foreach (RoadExitNode exitNode in roadSegment.GetRoadExitNodes())
        {
            foreach (RoadEntranceNode entranceNode in _entranceNodes.Where(n => n.GetHasAssociatedExitNode() == false))
            {
                if (Vector3.Distance(exitNode.GetPosition() + exitNode.GetParent().transform.position, entranceNode.GetPosition() + entranceNode.GetParent().transform.position) < 0.05f)
                {
                    exitNode.NextSegmentConnectedEntrance = entranceNode;
                    exitNode.NextSegment = roadSegment;
                    entranceNode.SetHasAssociatedExitNode(true);
                }
            }
        }
    }

    private void ConnectAttachmentNodes(RoadSegment roadSegment)
    {
        foreach (RoadAttachmentNode newNode in roadSegment.GetRoadAttachmentNodes())
        {
            foreach (RoadAttachmentNode existingOpenNode in _attachmentNodes.Where(n => n.GetOccupied() == false))
            {
                if (Vector3.Distance(existingOpenNode.GetPosition() + existingOpenNode.GetParent().transform.position, newNode.GetPosition() + newNode.GetParent().transform.position) < 0.05f)
                {
                    existingOpenNode.SetOccupied(true);
                    newNode.SetOccupied(true);
                }
            }
        }
    }

    public void ResetAttachmentNodes()
    {
        _attachmentNodes.Clear();
    }

    public List<RoadAttachmentNode> GetAttachmentNodes()
    {
        return _attachmentNodes;
    }

    public List<RoadEntranceNode> GetEntranceNodes()
    {
        return _entranceNodes;
    }

    public List<RoadExitNode> GetExitNodes()
    {
        return _exitNodes;
    }
}
