using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleGreenSequenceTrafficLightController : AbstractTrafficLightController
{
    public override void UpdateTrafficLights()
    {
        foreach (RoadEntranceNode node in roadEntranceNodes)
        {
            node.TrafficLightState = TrafficLightEnum.RED;
        }
    }

    public override void SecondaryUpdateTrafficLights()
    {
        roadEntranceNodes[currentGreenLight].TrafficLightState = TrafficLightEnum.GREEN;

        if (currentGreenLight + 1 >= roadEntranceNodes.Count)
        {
            currentGreenLight = 0;
        }
        else
        {
            currentGreenLight++;
        }
    }
}
