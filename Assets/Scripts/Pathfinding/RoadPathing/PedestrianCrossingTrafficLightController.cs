using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianCrossingTrafficLightController : AbstractTrafficLightController
{
    [SerializeField]
    private int _pedestrianCrossingGreenLayer = 12;
    [SerializeField]
    private int _pedestrianCrossingRedLayer = 11;

    public override void UpdateTrafficLights()
    {
        foreach (RoadEntranceNode node in roadEntranceNodes)
        {
            node.TrafficLightState = TrafficLightEnum.RED;
        }

        gameObject.layer = _pedestrianCrossingGreenLayer;
    }

    public override void SecondaryUpdateTrafficLights()
    {
        foreach (RoadEntranceNode node in roadEntranceNodes)
        {
            node.TrafficLightState = TrafficLightEnum.GREEN;
        }

        gameObject.layer = _pedestrianCrossingRedLayer;
    }
}
