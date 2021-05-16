using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadFollowerRandomBasic : MonoBehaviour, ICivilianRoadAI
{
    public RoadExitNode Target;

    private RoadNetworkManager _roadNetworkManager;
    private IGroundMovement _groundMovement;

    private bool _initialised;

    private float _roadNodeCheckDist = 4.0f;

    //void Start()
    //{
    //    _roadNetworkManager = FindObjectOfType<RoadNetworkManager>();
    //    _groundMovement = GetComponent<IGroundMovement>();
    //    //find road network

    //    //find nearest exit node, set it as target
    //    //if we spawned the car with a spawner it should have a exit node already though.

    //    //maybe make it so all cars are spawned.
    //    //but what about traffic initially on the road? have a simulation period on load?
    //    //run the traffic in editor and save its state for later runs?
    //    //have all entrance nodes behave a spawners for a short period at the start of a level?

    //    //leaning towards all road entrances being spawners for a short time. not much fucking about with acceleration, no serialising shit, customisable with how many cars spawn/how long
    //}

    public void Initialise(RoadNetworkManager manager, RoadExitNode initialTarget)
    {
        _roadNetworkManager = manager;
        _groundMovement = GetComponent<IGroundMovement>();
        Target = initialTarget;
        _initialised = true;
    }

    void Update()
    {
        if (_initialised)
        {
            FollowRoad();

            if (Vector3.Distance(this.transform.position, Target.GetPosition() + Target.GetParent().transform.position) < _roadNodeCheckDist)
            {
                if (Target is RoadDespawnNode)
                {
                    Destroy(gameObject);
                    return;
                }

                if (Target.NextSegmentConnectedEntrance.TrafficLightState == TrafficLightEnum.RED)
                {
                    _groundMovement.PauseMovement();
                    return;
                }
                _groundMovement.ResumeMovement();

                var targets = Target.NextSegmentConnectedEntrance.GetReachableExitNodes();
                Target = targets.ElementAt(Random.Range(0, targets.Count)).Key;
            }
        }

        //move towards target exit node
        //check every so often (doesn't need to be every update) if we're close enough to the exit node
        //if we are, get the entrance node it's connected to, select one of the valid exits from that node
        //valid exits are in the entrance node's reachable exit nodes (duh), and themselves have an entrance node set
        //we may not need to check for valid exit nodes, but on an unfinished road network it's fine.

        //if the exit node we reach is actually a despawnnode, destroy the vehicle.
    }

    private void FollowRoad()
    {
        if (Target != null)
        {
            var targetpos = Target.GetPosition() + Target.GetParent().transform.position;
            targetpos.y = this.transform.position.y;
            _groundMovement.MoveTowards(targetpos);
        }

    }
}
