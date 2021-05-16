using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSelectUnit : MonoBehaviour
{
    [SerializeField]
    private LayerMask _selectableUnitLayers;

    [SerializeField]
    private LayerMask _movementTargetableLayers;

    private SelectedUnits _selectedUnits;

    private float _commandRaycastDistance = 400f;

    void Start()
    {
        _selectedUnits = gameObject.GetComponent<SelectedUnits>();
        if (_selectedUnits == null)
        {
            Debug.LogError("Unit Selection Control could not find a SelectedUnits instance- can't keep any selections!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //TODO: selectionbox controls will be needed at some point
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectUnit();
            return;
        }

        if (Input.GetMouseButtonUp(1))
        {
            //move selected units to location.
            SetSelectedUnitsDestination();
        }
    }

    private void SetSelectedUnitsDestination()
    {
        Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(clickRay, out RaycastHit hit, _commandRaycastDistance, _movementTargetableLayers))
        {
            foreach(SelectableUnit selectedUnit in _selectedUnits.GetCurrentlySelectedUnits())
            {
                if (selectedUnit is SelectableUnitControllable)
                {
                    SelectableUnitControllable selectedUnitControllable = selectedUnit as SelectableUnitControllable;
                    selectedUnitControllable.SetTargetMovementPosition(hit.point);
                }
            }
        }
    }

    private void SelectUnit()
    {
        Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(clickRay, out RaycastHit hit, _commandRaycastDistance, _selectableUnitLayers))
        {
            var unitToSelect = hit.transform.GetComponent<SelectableUnit>();

            if (unitToSelect == null)
            {
                Debug.Log("Tried to select a unit with no SelectableUnit component");
                return;
            }

            _selectedUnits.SelectSingleUnit(unitToSelect);
            return;
        }
        _selectedUnits.DeselectAll();
    }
}
