using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Priority_Queue;
using UnityEditor;
using System;

public class AStar3dPathing : MonoBehaviour
{
    public float NavigableAreaXSize;
    public float NavigableAreaYSize;
    public float NavigableAreaZSize;

    public float MaximumNodeSize;
    public float MinimumNodeSize;

    public float VerticalMovementWeightMulti=1.0f;

    public LayerMask NonNavigableLayers;

    [SerializeField]
    private List<AStarNode> _nodes;

    private bool _initialised;
    private string _assetPath;
    private bool _assetExists;

    private Vector3 _failedTarget;


    public void Initialise()
    {
        _initialised = true;
        _assetPath = "Assets/Prefabs/AStarNavigation/" + this.name + ".asset";
        _assetExists = AssetDatabase.LoadAssetAtPath<AStarNodeList>(_assetPath) !=null;
    }

    void Start()
    {
        if (!_initialised)
        {
            Initialise();
        }

        if (_nodes == null)
        {
            _nodes = new List<AStarNode>();
        }
        if (_nodes.Count == 0)
        {
            if (_assetExists)
            {
                var nodes=AssetDatabase.LoadAssetAtPath<AStarNodeList>(_assetPath).GetList();
                _nodes = nodes;
            }
        }

        FindPathNodes(_nodes.First(), _nodes.Last());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(NavigableAreaXSize, NavigableAreaYSize, NavigableAreaZSize));

        if (_nodes != null)
        {
            foreach (AStarNode node in _nodes)
            {
                Gizmos.DrawSphere(node.GetPosition(), (MinimumNodeSize / MaximumNodeSize) * node.GetSize() / 2);
            }

        }

        if (_failedTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_failedTarget, 8);
        }
    }

    public TrackablePath FindPathTrackable(Vector3 start, Vector3 end, bool allowEarlyExit)
    {
        AStarNode startNode = null;
        AStarNode endNode = null;

        foreach (AStarNode node in _nodes)
        {
            var checkStartNode = GetNearerNode(start, startNode, node);
            if (checkStartNode != null)
            {
                startNode = checkStartNode;
            }

            var checkEndNode=GetNearerNode(end, endNode, node);
            if (checkEndNode != null)
            {
                endNode = checkEndNode;
            }

            if (allowEarlyExit)
            {
                if (startNode != null && endNode != null)
                {
                    break;
                }
            }
        }

        if (startNode == null || endNode == null)
        {
            Debug.Log("FindPathTrackable could not find near enough nodes. Is the object that's trying to path inside a AStar3D pathable volume?");
            return null;
        }

        var path= FindPathNodes(startNode, endNode);

        if (path == null)
        {
            Debug.Log("Could not find a path between " + startNode.GetPosition() + " and " + endNode.GetPosition());
            _failedTarget = endNode.GetPosition();
            return null;
        }

        return new TrackablePath(startNode, endNode, path);
    }

    private static AStarNode GetNearerNode(Vector3 target, AStarNode currentNearestNode, AStarNode node)
    {
        if (Vector3.Distance(target, node.GetPosition()) < node.GetSize()*2)
        {
            if (currentNearestNode == null)
            {
                return node;
            }

            if (Vector3.Distance(target, node.GetPosition()) < Vector3.Distance(target, currentNearestNode.GetPosition()))
            {
                return node;
            }

            return currentNearestNode;
        }

        return null;
    }

    private Dictionary<AStarNode, AStarNode> FindPathNodes(AStarNode startNode, AStarNode endNode)
    {
        var path = new Dictionary<AStarNode, AStarNode>();

        var frontier = new SimplePriorityQueue<AStarNode>();

        var cameFromNode = new Dictionary<AStarNode, AStarNode>();
        var pathCostToNode = new Dictionary<AStarNode, float>();

        cameFromNode.Add(startNode, null);
        pathCostToNode.Add(startNode, 0);

        frontier.Enqueue(startNode, 0);

        while (!(frontier.Count() == 0))
        {
            var currentNode = frontier.Dequeue();

            if (currentNode == endNode)
            {
                return TrimPath(cameFromNode, startNode, endNode);
            }

            foreach (KeyValuePair<AStarNode, float> nextNode in currentNode.GetNeighbours())
            {
                var nextWeight = nextNode.Value;
                if (nextNode.Key.GetPosition().y> (currentNode.GetPosition().y+currentNode.GetSize()/2) || nextNode.Key.GetPosition().y < (currentNode.GetPosition().y - currentNode.GetSize() / 2))
                {
                    nextWeight = nextNode.Value * VerticalMovementWeightMulti;
                }

                var newCost = pathCostToNode[currentNode] + nextWeight;//todo: try replacing next.getsize with (current.getsize+next.getsize), see if it works better

                if (pathCostToNode.ContainsKey(nextNode.Key))
                {
                    if (newCost >= pathCostToNode[nextNode.Key])
                    {
                        continue;
                    }
                }

                pathCostToNode[nextNode.Key] = newCost;
                var priority = newCost + ManhattanDistance(endNode, nextNode.Key);
                frontier.Enqueue(nextNode.Key, priority);
                cameFromNode[nextNode.Key] = currentNode;
            }
        }

        return null;
    }

    private Dictionary<AStarNode, AStarNode> TrimPath(Dictionary<AStarNode, AStarNode> path, AStarNode startNode, AStarNode endNode)
    {
        Dictionary<AStarNode, AStarNode> trimmedPath= new Dictionary<AStarNode, AStarNode>();
        var targetNode = endNode;
        var trimmedToStart=false;
        var trimmedPathLengthSoFar= 0;

        while (trimmedToStart == false)
        {
            foreach (KeyValuePair<AStarNode, AStarNode> pair in path.Reverse())
            {
                if (pair.Key == targetNode)
                {
                    trimmedPath.Add(pair.Key, pair.Value);
                    targetNode = pair.Value;
                    trimmedToStart = (pair.Key == startNode);
                }
            }

            if(trimmedPath.Count()== trimmedPathLengthSoFar)
            {
                throw new System.Exception("TrimPath did not reach a complete path, but found no nodes to trim. Is this a valid path?");
            }

            trimmedPathLengthSoFar = trimmedPath.Count();
        }

        return trimmedPath.Reverse().ToDictionary(o => o.Key, o=> o.Value);
    }

    private float ManhattanDistance(AStarNode pointA, AStarNode pointB)
    {
        var pointAPosition = pointA.GetPosition();
        var pointBPosition = pointB.GetPosition();

        var distance = Mathf.Abs(pointAPosition.x - pointBPosition.x) + Mathf.Abs(pointAPosition.y - pointBPosition.y) + Mathf.Abs(pointAPosition.y - pointBPosition.y);
        return distance;
    }

    public void CreateNodes()
    {
        Debug.Log("Creating Nodes");

        var startingCorner = gameObject.transform.position;
        startingCorner.x = startingCorner.x - NavigableAreaXSize / 2;
        startingCorner.y = startingCorner.y - NavigableAreaYSize / 2;
        startingCorner.z = startingCorner.z - NavigableAreaZSize / 2;

        var startingPosition = startingCorner + new Vector3(MaximumNodeSize / 2, MaximumNodeSize / 2, MaximumNodeSize / 2);
        var nodePosition = startingPosition;

        var endPoint = new Vector3(gameObject.transform.position.x + NavigableAreaXSize / 2 - MaximumNodeSize / 2, gameObject.transform.position.y + NavigableAreaYSize / 2 - MaximumNodeSize / 2, gameObject.transform.position.z + NavigableAreaZSize / 2 - MaximumNodeSize / 2);

        CreateNodesXYZAxes(startingPosition, ref nodePosition, endPoint);

        foreach (AStarNode node in _nodes)
        {
            FindAllNeighbours(node);
        }
    }

    private void CreateNodesXYZAxes(Vector3 startingPosition, ref Vector3 nodePosition, Vector3 endPoint)
    {
        bool atEndOfX = PrettyMuchEqual(nodePosition.x, endPoint.x);

        while ((nodePosition.x < endPoint.x) || atEndOfX)
        {
            CreateNodesYZAxes(startingPosition, ref nodePosition, endPoint);

            if (atEndOfX)
            {
                break;
            }

            nodePosition.x = nodePosition.x + MaximumNodeSize;
            atEndOfX = PrettyMuchEqual(nodePosition.x, endPoint.x);
        }
    }

    private void CreateNodesYZAxes(Vector3 startingPosition, ref Vector3 nodePosition, Vector3 endPoint)
    {
        nodePosition.y = startingPosition.y;
        bool atEndOfY = PrettyMuchEqual(nodePosition.y, endPoint.y);

        while ((nodePosition.y < endPoint.y) || atEndOfY)
        {
            CreateNodesZAxis(startingPosition, ref nodePosition, endPoint);

            if (atEndOfY)
            {
                break;
            }
            nodePosition.y = nodePosition.y + MaximumNodeSize;
            atEndOfY = PrettyMuchEqual(nodePosition.y, endPoint.y);
        }
    }

    private void CreateNodesZAxis(Vector3 startingPosition, ref Vector3 nodePosition, Vector3 endPoint)
    {
        nodePosition.z = startingPosition.z;
        bool atEndOfZ = PrettyMuchEqual(nodePosition.z, endPoint.z);

        while ((nodePosition.z < endPoint.z) || atEndOfZ)
        {
            TryPlaceNodeAtPostionRecursive(nodePosition, MaximumNodeSize);

            if (atEndOfZ)
            {
                break;
            }
            nodePosition.z = nodePosition.z + MaximumNodeSize;
            atEndOfZ = PrettyMuchEqual(nodePosition.z, endPoint.z);
        }
    }

    private void TryPlaceNodeAtPostionRecursive(Vector3 position, float size)
    {
        if (size > MinimumNodeSize || PrettyMuchEqual(size, MinimumNodeSize))
        {
            var nodePlaced = TryPlaceNodeAtPostion(position, size);
            if (!nodePlaced)
            {
                TryPlaceSubNodes(position, size);
            }
        }
    }

    private void TryPlaceSubNodes(Vector3 position, float size)
    {
        var halfSize = size / 2;
        var quarterSize = size / 4;
        var positionModX = new Vector3(quarterSize, 0, 0);
        var positionModY = new Vector3(0, quarterSize, 0);
        var positionModZ = new Vector3(0, 0, quarterSize);

        TryPlaceNodeAtPostionRecursive(position + positionModX + positionModY + positionModZ, halfSize);
        TryPlaceNodeAtPostionRecursive(position + positionModX + positionModY - positionModZ, halfSize);
        TryPlaceNodeAtPostionRecursive(position + positionModX - positionModY + positionModZ, halfSize);
        TryPlaceNodeAtPostionRecursive(position + positionModX - positionModY - positionModZ, halfSize);

        TryPlaceNodeAtPostionRecursive(position - positionModX + positionModY + positionModZ, halfSize);
        TryPlaceNodeAtPostionRecursive(position - positionModX + positionModY - positionModZ, halfSize);
        TryPlaceNodeAtPostionRecursive(position - positionModX - positionModY + positionModZ, halfSize);
        TryPlaceNodeAtPostionRecursive(position - positionModX - positionModY - positionModZ, halfSize);
    }

    private bool TryPlaceNodeAtPostion(Vector3 position, float size)
    {
        var colliders = Physics.OverlapBox(position, new Vector3(size / 2, size / 2, size / 2));

        if (colliders.Count() == 0)
        {
            InitialiseAndSaveNewNode(position, size);
            return true;
        }
        else if (!colliders.Any(c => IsInLayerMask(c.gameObject.layer, NonNavigableLayers)))
        {
            InitialiseAndSaveNewNode(position, size);
            return true;
        }
        return false;
    }

    public void InitialiseAndSaveNewNode(Vector3 position, float size)
    {
        var newNode = ScriptableObject.CreateInstance<AStarNode>();
        newNode.InitialiseNode(position, size);

        if (!_assetExists)
        {
            var nodeListScriptable = ScriptableObject.CreateInstance<AStarNodeList>();
            nodeListScriptable.SetList(_nodes);
            AssetDatabase.CreateAsset(nodeListScriptable, _assetPath);

            _assetExists = AssetDatabase.LoadAssetAtPath<AStarNodeList>(_assetPath) != null;

            if (!_assetExists)
            {
                throw new Exception("asset still doesn't exist");
            }
        }

        AssetDatabase.AddObjectToAsset(newNode, _assetPath);
        _nodes.Add(newNode);
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }

    private void FindAllNeighbours(AStarNode node)
    {
        FindCardinalNeighbours(node);
        FindDiagonalNeighbours(node);
    }

    private void FindCardinalNeighbours(AStarNode node)
    {
        FindFirstNeighbourOrRecurse(node, node.GetSize(), Axes.PositiveX);
        FindFirstNeighbourOrRecurse(node, -node.GetSize(), Axes.NegativeX);

        FindFirstNeighbourOrRecurse(node, node.GetSize(), Axes.PositiveY);
        FindFirstNeighbourOrRecurse(node, -node.GetSize(), Axes.NegativeY);

        FindFirstNeighbourOrRecurse(node, node.GetSize(), Axes.PositiveZ);
        FindFirstNeighbourOrRecurse(node, -node.GetSize(), Axes.NegativeZ);
    }

    private void FindDiagonalNeighbours(AStarNode node)
    {
        List<Vector3> Offsets2D = new List<Vector3>
        {
            new Vector3(node.GetSize(), 0, node.GetSize()),
            new Vector3(node.GetSize(), 0, -node.GetSize()),
            new Vector3(-node.GetSize(), 0, node.GetSize()),
            new Vector3(-node.GetSize(), 0, -node.GetSize()),

            new Vector3(node.GetSize(), node.GetSize(), 0),
            new Vector3(node.GetSize(), -node.GetSize(), 0),
            new Vector3(-node.GetSize(), node.GetSize(), 0),
            new Vector3(-node.GetSize(), -node.GetSize(), 0),

            new Vector3(0, node.GetSize(), node.GetSize()),
            new Vector3(0, node.GetSize(), -node.GetSize()),
            new Vector3(0, -node.GetSize(), node.GetSize()),
            new Vector3(0, -node.GetSize(), -node.GetSize())
        };


        foreach (Vector3 offset in Offsets2D)
        {
            var neighbour = TryGetNeighbourFromNode(node, offset);
            if (neighbour != null)
            {
                node.AddNeighbour(neighbour, DiagonalType.Diagonal2D);
            }
        }

        List<Vector3> Offsets3D = new List<Vector3>
        {
            new Vector3(node.GetSize(), node.GetSize(), node.GetSize()),
            new Vector3(-node.GetSize(), node.GetSize(), node.GetSize()),
            new Vector3(node.GetSize(), node.GetSize(), -node.GetSize()),
            new Vector3(-node.GetSize(), node.GetSize(), -node.GetSize()),

            new Vector3(node.GetSize(), -node.GetSize(), node.GetSize()),
            new Vector3(-node.GetSize(), -node.GetSize(), -node.GetSize()),
            new Vector3(-node.GetSize(), -node.GetSize(), node.GetSize()),
            new Vector3(node.GetSize(), -node.GetSize(), -node.GetSize())
        };

        foreach (Vector3 offset in Offsets3D)
        {
            var neighbour = TryGetNeighbourFromNode(node, offset);
            if (neighbour != null)
            {
                node.AddNeighbour(neighbour, DiagonalType.Diagonal3D);
            }
        }
    }

    private void FindFirstNeighbourOrRecurse(AStarNode startingNode, float size, Axes axis)
    {
        Vector3 offset;
        switch (axis)
        {
            case Axes.PositiveX:
            case Axes.NegativeX:
                offset = new Vector3(size, 0, 0);
                break;
            case Axes.PositiveY:
            case Axes.NegativeY:
                offset = new Vector3(0, size, 0);
                break;
            case Axes.PositiveZ:
            case Axes.NegativeZ:
                offset = new Vector3(0, 0, size);
                break;
            default:
                return;
        }

        var neighbour = TryGetNeighbourFromNode(startingNode, offset);

        if (neighbour != null)
        {
            startingNode.AddNeighbour(neighbour);
        }
        else
        {
            FindAllSubNeighboursRecursive(startingNode, startingNode.GetPosition() + offset, startingNode.GetSize(), axis);
        }
    }

    private AStarNode TryGetNeighbourFromNode(AStarNode node, Vector3 offset)
    {
        Vector3 expectedNeighbourPosition = node.GetPosition() + offset;

        return TryGetNeighbour(expectedNeighbourPosition);
    }

    private AStarNode TryGetNeighbourFromPosition(Vector3 neighbourPosition, Vector3 offset)
    {
        Vector3 expectedNeighbourPosition = neighbourPosition + offset;

        return TryGetNeighbour(expectedNeighbourPosition);
    }

    private AStarNode TryGetNeighbour(Vector3 expectedNeighbourPosition)
    {
        var TargetNode = _nodes.FirstOrDefault(n => Vector3.Distance(n.GetPosition(), expectedNeighbourPosition) < MinimumNodeSize / 2);
        if (TargetNode != null)
        {
            return TargetNode;
        }
        return null;
    }

    private void FindAllSubNeighboursRecursive(AStarNode homeNode, Vector3 neighbourPosition, float size, Axes axis)
    {
        var offset = size / 4;
        switch (axis)
        {
            case Axes.PositiveX:
                neighbourPosition.x = neighbourPosition.x - offset;
                break;
            case Axes.NegativeX:
                neighbourPosition.x = neighbourPosition.x + offset;
                break;
            case Axes.PositiveY:
                neighbourPosition.y = neighbourPosition.y - offset;
                break;
            case Axes.NegativeY:
                neighbourPosition.y = neighbourPosition.y + offset;
                break;
            case Axes.PositiveZ:
                neighbourPosition.z = neighbourPosition.z - offset;
                break;
            case Axes.NegativeZ:
                neighbourPosition.z = neighbourPosition.z + offset;
                break;
            default:
                return;
        }

        SubNeighbourRecursive(homeNode, neighbourPosition, size, offset, offset, axis);
        SubNeighbourRecursive(homeNode, neighbourPosition, size, -offset, offset, axis);
        SubNeighbourRecursive(homeNode, neighbourPosition, size, offset, -offset, axis);
        SubNeighbourRecursive(homeNode, neighbourPosition, size, -offset, -offset, axis);
    }

    private void SubNeighbourRecursive(AStarNode homeNode, Vector3 neighbourPosition, float size, float offset1, float offset2, Axes axis)
    {
        AStarNode subNeighbour;
        //TODO: this never finds a y-neighbour. Look into it. Flaw in TryGetNeighbour? Update: This never finds different sized neighbours
        var positionOffset = new Vector3();
        switch (axis)
        {
            case Axes.PositiveX:
            case Axes.NegativeX:
                positionOffset = new Vector3(0, offset1, offset2);
                subNeighbour = TryGetNeighbourFromPosition(neighbourPosition, positionOffset);
                break;
            case Axes.PositiveY:
            case Axes.NegativeY:
                positionOffset = new Vector3(offset1, 0, offset2);
                subNeighbour = TryGetNeighbourFromPosition(neighbourPosition, positionOffset);
                break;
            case Axes.PositiveZ:
            case Axes.NegativeZ:
                positionOffset = new Vector3(offset1, offset2, 0);
                subNeighbour = TryGetNeighbourFromPosition(neighbourPosition, positionOffset);
                break;
            default:
                return;
        }

        if (subNeighbour != null)
        {
            homeNode.AddNeighbour(subNeighbour);
            subNeighbour.AddNeighbour(homeNode);
        }
        else
        {
            if (size / 2 < MinimumNodeSize && !PrettyMuchEqual(MinimumNodeSize, size / 2))
            {
                return;
            }
            FindAllSubNeighboursRecursive(homeNode, neighbourPosition + positionOffset, size / 2, axis);
        }
    }

    private bool PrettyMuchEqual(float numA, float numB, float allowedDeviation = 0.0001f)
    {
        float difference = Mathf.Abs(numA * allowedDeviation);

        if (Mathf.Abs(numA - numB) <= difference)
        {
            return true;
        }

        return false;
    }

    public List<AStarNode> GetNodes()
    {
        return _nodes;
    }

}

