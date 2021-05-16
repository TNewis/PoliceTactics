//This script handles all of the physics behaviors for the player's ship. The primary functions
//are handling the hovering and thrust calculations. 

using UnityEngine;

public class HoverMovement : MonoBehaviour
{
	public float speed;						//The current forward speed of the ship

	[Header("Drive Settings")]
	public float driveForce = 17f;			//The force that the engine generates
	public float slowingVelFactor = .99f;   //The percentage of velocity the ship maintains when not thrusting (e.g., a value of .99 means the ship loses 1% velocity when not thrusting)
	public float brakingVelFactor = .95f;   //The percentage of velocty the ship maintains when braking
	public float angleOfRoll = 30f;			//The angle that the ship "banks" into a turn

	[Header("Hover Settings")]
	public float hoverHeight = 1.5f;        //The height the ship maintains when hovering
	public float maxGroundDist = 5f;        //The distance the ship can be above the ground before it is "falling"
	public float hoverForce = 300f;			//The force of the ship's hovering
	public LayerMask whatIsGround;			//A layer mask to determine what layer the ground is on
	public PIDController hoverPID;			//A PID controller to smooth the ship's hovering

	[Header("Physics Settings")]
	public Transform shipBody;				//A reference to the ship's body, this is for cosmetics
	public float terminalVelocity = 100f;   //The max speed the ship can go
	public float hoverGravity = 20f;        //The gravity applied to the ship while it is on the ground
	public float fallGravity = 80f;			//The gravity applied to the ship while it is falling

	Rigidbody rigidBody;					//A reference to the ship's rigidbody
	//PlayerInput input;						//A reference to the player's input					
	float drag;								//The air resistance the ship recieves in the forward direction
	bool isOnGround;						//A flag determining if the ship is currently on the ground

    public Transform target;

    private float _desiredThrust = 10.0f;
    private float _desiredRudder=0.1f;
    private float _rayCastOffset = 20f;
    private float _detectObstacleDistance = 200.0f;

	void Start()
	{
		rigidBody = GetComponent<Rigidbody>();

		drag = driveForce / terminalVelocity;
	}

	void FixedUpdate()
	{
		speed = Vector3.Dot(rigidBody.velocity, transform.forward);

        RaycastPathing();
        MoveTowardsTargetNonPhysics();
    }

    void CalculateHover()
	{
		Vector3 groundNormal;

		Ray ray = new Ray(transform.position, -transform.up);
		RaycastHit hitInfo;
		isOnGround = Physics.Raycast(ray, out hitInfo, maxGroundDist, whatIsGround);

		if (isOnGround)
		{
			float height = hitInfo.distance;
			groundNormal = hitInfo.normal.normalized;
			float forcePercent = hoverPID.Seek(hoverHeight, height);
			
			Vector3 force = groundNormal * hoverForce * forcePercent;
			Vector3 gravity = -groundNormal * hoverGravity * height;

			rigidBody.AddForce(force, ForceMode.Acceleration);
			rigidBody.AddForce(gravity, ForceMode.Acceleration);
		}
		else
		{
			groundNormal = Vector3.up;
			Vector3 gravity = -groundNormal * fallGravity;
			rigidBody.AddForce(gravity, ForceMode.Acceleration);
		}

		Vector3 projection = Vector3.ProjectOnPlane(transform.forward, groundNormal);
		Quaternion rotation = Quaternion.LookRotation(projection, groundNormal);
		rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation, Time.deltaTime * 10f));
	}


    void LookAtTargetNonPhysics()
    {
        float rotationalDamp = .25f;
        Vector3 pos = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationalDamp * Time.deltaTime);
    }

    void MoveTowardsTargetNonPhysics()
    {
        transform.position += transform.forward * _desiredThrust * Time.deltaTime;
    }

    void RaycastPathing()
    {
        RaycastHit hit;
        Vector3 raycastOffset = Vector3.zero;

        Vector3 left = transform.position - transform.right * _rayCastOffset;
        Vector3 right = transform.position + transform.right * _rayCastOffset;
        Vector3 up = transform.position + transform.up * _rayCastOffset;
        Vector3 down = transform.position - transform.up * _rayCastOffset;

        Debug.DrawRay(left, transform.forward * _detectObstacleDistance, Color.cyan);
        Debug.DrawRay(right, transform.forward * _detectObstacleDistance, Color.cyan);
        Debug.DrawRay(up, transform.forward * _detectObstacleDistance, Color.cyan);
        Debug.DrawRay(down, transform.forward * _detectObstacleDistance, Color.cyan);

        if (Physics.Raycast(left, transform.forward, out hit, _detectObstacleDistance))
        {
            raycastOffset += Vector3.right;
        }
        else if (Physics.Raycast(right, transform.forward, out hit, _detectObstacleDistance))
        {
            raycastOffset -= Vector3.right;
        }

        if (Physics.Raycast(up, transform.forward, out hit, _detectObstacleDistance))
        {
            raycastOffset -= Vector3.up;
        }
        else if (Physics.Raycast(down, transform.forward, out hit, _detectObstacleDistance))
        {
            raycastOffset += Vector3.up;
        }

        if (raycastOffset != Vector3.zero)
        {
            transform.Rotate(raycastOffset * 5f * Time.deltaTime);
        }
        else
        {
            LookAtTargetNonPhysics();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        //If the ship has collided with an object on the Wall layer...
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            //...calculate how much upward impulse is generated and then push the vehicle down by that amount 
            //to keep it stuck on the track (instead up popping up over the wall)
            Vector3 upwardForceFromCollision = Vector3.Dot(collision.impulse, transform.up) * transform.up;
            rigidBody.AddForce(-upwardForceFromCollision, ForceMode.Impulse);
        }
    }

    public float GetSpeedPercentage()
	{
		//Returns the total percentage of speed the ship is traveling
		return rigidBody.velocity.magnitude / terminalVelocity;
	}
}
