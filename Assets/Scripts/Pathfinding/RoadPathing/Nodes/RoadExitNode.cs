using UnityEngine;

public class RoadExitNode : RoadNode
{
    [SerializeField]
    private RoadSegment _nextSegment;
    [SerializeField]
    private RoadEntranceNode _nextSegmentConnectedEntrance;

    public RoadSegment NextSegment { get => _nextSegment; set => _nextSegment = value; }
    public RoadEntranceNode NextSegmentConnectedEntrance { get => _nextSegmentConnectedEntrance; set => _nextSegmentConnectedEntrance = value; }

    public override void Initialise(Vector3 position)
    {
        _position = position;
    }


}