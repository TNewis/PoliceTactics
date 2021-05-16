using UnityEngine;

public class RoadAttachmentNode : RoadNode
{
    [SerializeField]
    private bool _occupied;
    [SerializeField]
    private float _angle = 0.0f;

    public override void Initialise(Vector3 position)
    {
        _position = position;
        _occupied = false;
    }

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    public float GetAngle()
    {
        return _angle;
    }

    public void SetOccupied(bool occupied)
    {
        _occupied = occupied;
    }

    public bool GetOccupied()
    {
        return _occupied;
    }

}