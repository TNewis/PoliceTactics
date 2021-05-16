using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedUnits : MonoBehaviour
{
    private List<SelectableUnit> _currentlySelectedUnits;

    private void Start()
    {
        if (_currentlySelectedUnits == null)
        {
            _currentlySelectedUnits = new List<SelectableUnit>();
        }
    }

    public List<SelectableUnit> GetCurrentlySelectedUnits()
    {
        return _currentlySelectedUnits;
    }

    public void SelectSingleUnit(SelectableUnit selectedUnit)
    {
        _currentlySelectedUnits.Clear();
        _currentlySelectedUnits.Add(selectedUnit);
        Debug.Log(selectedUnit.gameObject.name + " selected");
    }

    public void AddUnitToSelection(SelectableUnit selectedUnit)
    {
        //TODO: Shift+LMB- add unit to existing selection.
        _currentlySelectedUnits.Add(selectedUnit);
    }

    public void SelectMultipleUnits(List<SelectableUnit> selectedUnits)
    {
        //TODO: LMB drag- box select multiple units;
        _currentlySelectedUnits.Clear();
        _currentlySelectedUnits.AddRange(selectedUnits);
    }

    public void AddMultipleUnitsToSelection(List<SelectableUnit> selectedUnits)
    {
        //TODO: Shift + LMB drag- box select multiple units and add them to selection.
        _currentlySelectedUnits.AddRange(selectedUnits);
    }

    public void DeselectAll()
    {
        _currentlySelectedUnits.Clear();
        Debug.Log("Deselected units");
    }
}
