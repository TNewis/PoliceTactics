using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICivilianRoadAI
{
    void Initialise(RoadNetworkManager manager, RoadExitNode initialTarget);
}
