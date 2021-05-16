using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoMarker : MonoBehaviour
{
    private Vector3 _position = new Vector3();
    private float _angle;
    private Vector3 _angleMarkerEndPosition = new Vector3();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_position, 3);
        Gizmos.DrawSphere(_position, .2f);
        Gizmos.DrawLine(_position, _angleMarkerEndPosition);
    }

    public void SetPosition(Vector3 pos)
    {
        _position = pos;
        CalculateAngleMarkerPosition();
    }

    public void SetAngle(float angle)
    {
        _angle = angle;
        CalculateAngleMarkerPosition();
    }

    private void CalculateAngleMarkerPosition()
    {
        var line = (transform.forward * 4);
        var angleEndPositionModifier=Quaternion.AngleAxis(_angle, transform.up) * line;
        _angleMarkerEndPosition = _position + angleEndPositionModifier;
    }

}
