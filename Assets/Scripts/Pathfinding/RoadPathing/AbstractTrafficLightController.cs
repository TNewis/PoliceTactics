using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTrafficLightController : MonoBehaviour
{
    [SerializeField]
    private int _updateInterval;

    [SerializeField]
    private int _secondaryIntervalOffset;

    private RoadSegment _attachedSegment;
    protected List<RoadEntranceNode> roadEntranceNodes;

    protected int currentGreenLight;

    protected virtual void Start()
    {
        _attachedSegment = gameObject.GetComponent<RoadSegment>();
        roadEntranceNodes = _attachedSegment.GetRoadEntranceNodes();

        UpdateTrafficLights();
        InvokeRepeating("UpdateTrafficLights", 0.0f, _updateInterval);
        InvokeRepeating("SecondaryUpdateTrafficLights", _secondaryIntervalOffset, _updateInterval);
    }

    public virtual void UpdateTrafficLights()
    {
    }

    public virtual void SecondaryUpdateTrafficLights()
    {
    }

}
