using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SelectableUnit : MonoBehaviour
{
    public Image UnitPortrait { get; set; }
    //TODO: this shit
    //Selectable unit script
    //gameobjects with this script should be automatically selectable

    //selection should use a raycast that only checks for particular layers (Vehicle, pedestrian, playercontrolledvehicle, playercontrolledpedestrian etc.)
    //if it finds something on that layer, it checks that object for an instance of this script (SelectableUnit) or an extension thereof.

    //types of selectable unit:
    //civillians
    //civilian vehicles
    //player controlled police vehicles
    //player controlled police pedestrians
    //friendly non-civillian vehicles and pedestrians
    //neutral non-civillian vehicles and pedestrians
    //hostile non-civillian vehicles and pedestrians

    //civillians and civillian vehicles should show an info box with some options (e.g. arrest, kill, question, pull over, etc.)
    //player controlled vehicles should have a UI unit cameo and associated commands specific to the unit type (e.g. flyers should have altitude up/down buttons)
    //the player controlled versions of SelectableUnit should therefore call out to the UI to create a cameo UI element, keep a reference to it and be able to perform actions such as
    //highlight, destroy/change element to reflect unit destruction

    //non-civillain, non-player-controlled units should be layered the same as civillian equivalents, and have a specific extension to this script as required- 
    //they still select like civillians, but may have restricted
    //options (e.g. can't arrest or kill friendly non-player-controlled police units, can't pull over actively hostile units but can kill)

}
