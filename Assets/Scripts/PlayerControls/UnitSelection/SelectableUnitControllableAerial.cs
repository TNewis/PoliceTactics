using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUnitControllableAerial : SelectableUnitControllable
{
    [SerializeField]
    private float _hoverAltitude= 20f;
    [SerializeField]
    private float _maxHover = 400f;
    [SerializeField]
    private float _minHover = 0f;
    [SerializeField]
    private float _hoverIncrement = 5f;

    public override void SetTargetMovementPosition(Vector3 targetPosition)
    {
        targetPosition.y = targetPosition.y + _hoverAltitude;
        _playerControllableUnitMovement.PlayerControlMoveToTargetPosition(targetPosition);
    }

    public void IncreaseAltitude()
    {
        if (_hoverAltitude< _maxHover)
        {
            _hoverAltitude = _hoverAltitude + _hoverIncrement;
        }
    }

    public void DecreaseAltitude()
    {
        if (_hoverAltitude > _minHover + _hoverIncrement)
        {
            _hoverAltitude = _hoverAltitude - _hoverIncrement;
        }
    }
}
