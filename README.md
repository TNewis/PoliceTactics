# PoliceTactics Project
 
 A project that never made much progress, but has a few interesting features included
 
## A* Pathfinding

![PoliceCopterAStarPath](https://user-images.githubusercontent.com/47041450/118393200-be782980-b635-11eb-9faa-6364118e6f2c.png)

There is a small helicopter with a very basic movement script. It can be selected with LMB and will move to any point within the predefined pathfindable volume with RMB. It will move towards a target set by the included A* pathfinding algorithm. 
- The pathfinding nodes and their adjacency are pre-generated and serialised, as the generation of the full mesh takes a significant amount of time.
- The pathfinder can generate a path between any accessible pair of nodes
- Pathfinding can be sped up by using the "Allow Early Exit" flag to allow pathfinding to finish as soon as an path has been returned by the pathfinder.
- Scene view has some gizmos set up to show the path, start, end and current target nodes.
- Selecting the AStarPath object will show some gizmos displaying the pathfinding mesh and nodes, though this will cause some frame rate issues as there are a lot of nodes.
- To generate a pathfinding volume, make sure a gameobject with the AStar3DPathing script attached exists, them use the included generator in Tools > A* 3D Node Generator.
- The intention for this was that this system would be used for overall paths that navigate around generally static objects, and a more precise pathing system used for navigating around smaller, mobile objects.

## Roads

![PoliceCopterCarPathing](https://user-images.githubusercontent.com/47041450/118393405-ddc38680-b636-11eb-8e93-1cb9cea90724.png)

There is a basic road network included. Actual functionality is very basic.
- Vehicles will move from the entrance node of a road segment directly to the exit.
- They will then attach to the next road segment.
- Given a choice of exit nodes, they will choose randomly.
- They will stop if the segment has a traffic light system enabled and the light is RED.
- They will stop if a collider is directly in front of them.

The more interesting part of this road system is the set of unity editor plugins for creating road networks and segments. The UX is pretty poor, but since these tools were intended for my own use, it wasn't a huge concern for me.

### Road Network Builder

![PoliceCopterRoadBuilder](https://user-images.githubusercontent.com/47041450/118393577-d81a7080-b637-11eb-95e8-182035492222.png)

This editor extension allows the placing of road segments.
- Segments are attached using their attachment node.
- Each entrance/exit node will then find its closest partner in the other segment and connect.
- If no attachment nodes are available, the road network is considered complete.
- If there are spawn nodes, the network will spawn vehicles.
- Vehicles will be destroyed if they reach a despwn node.
- Object pooling is not used as this project never went far enough to justify it.

### Road Segment Editor

![PoliceCopterRoadEditor](https://user-images.githubusercontent.com/47041450/118393867-3562f180-b639-11eb-9db3-4c934b5f7c19.png)

This editor allows the placement of the various nodes used by the road network.
- Attachment nodes flag to the network builder where two segments may connect.
- Entrance nodes flag a valid location for a pathing object to enter the road segment.
- Exit nodes flag where the pathing object may leave the segment.
- The "Edit Paths" section allows the selection of which exit nodes can be reached from a given entrance node.
- Spawn nodes will create pathing objects at their location on an interval.
- Despawn nodes will destroy any pathing object that gets close.
