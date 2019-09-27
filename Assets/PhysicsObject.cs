using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: fix slopes!!! right now they're very inconsistent... for some reason. Sometimes 

// PositionState tracks how the object is interacting with the world
public struct PositionState
{
    public bool pushRight;
    public bool pushLeft;
    public bool pushBottom;
    public bool pushTop;

    public bool grounded;

    public void Reset()
    {
        pushRight = false;
        pushLeft = false;
        pushBottom = false;
        pushTop = false;
        grounded = false;
    }
}

public class PhysicsObject : MonoBehaviour
{
    public float gravityModifier = 1f;  // allows user to change strength of gravity
    public float minGroundNormalY = 0.65f; // how flat a surface needs to be to be considered 'ground'
    public float minWallNormalX = 0.95f; // how vertical a surface needs to be to be considered a 'wall'
    [Range(-1f,1f)] public float collisionSlipFactor = 0.5f; // how slippery collisions are (1 is all horizontal velocity is conserved, -1 is no horizontal velocity is maintained);

    // the protected keyword here protects these variables from being globally changed by a derivative class. 
    // Each derivative class can change it's own local version of these variables, but these changes do not propagate
    // back to the parent class (PhysicsObject)
    protected PositionState positionState;
    protected PositionState prevState;
    protected Vector2 groundNormal = new Vector2(0.0f,1.0f);

    protected Vector2 targetVelocity;
    protected Vector2 velocity;
    protected Vector2 acceleration;
    protected Vector2 gravity;
    protected Rigidbody2D rb2D;
    
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    // OnEnable occurs once when the object is instantiated
    private void OnEnable()
    {
        // get the rigidbody2D component of current object
        rb2D = GetComponent<Rigidbody2D>();
        // assign gravity to user input unless it is overridden later
        gravity = gravityModifier * Physics2D.gravity;
        // Debug.Log(string.Format("OnEnable gravity {0}\n", gravity));
    }
    // Start is called before the first frame update
    private void Start()
    {
        // ignore any contacts with trigger colliders (Allows player to move through triggers)
        contactFilter.useTriggers = false;
        // set layer contact filter to current objects layer (ensures layer collision is properly handled per project settings)
        // edit -> project settings -> physics2D in Unity Editor allows user to edit layer collision matrix
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    // FixedUpdate is called at a fixed rate for physics simulation
    private void FixedUpdate()
    {
        // store previouse player state
        prevState = positionState;
        // reset current player state
        positionState.Reset();
        // calculate acceleration vector
        acceleration = gravity;
        // calculate new velocity due to acceleration
        velocity += acceleration * Time.deltaTime;
        // check if we are in the air
        if (!positionState.grounded)
        {
            groundNormal.x = 0.0f;
            groundNormal.y = 1.0f;
        }
        // add input velocity in x direction (note groundNormal.y here increases speed up hills)
        velocity.x = targetVelocity.x * (1.0f / groundNormal.y);
        // create vector along the ground movement direction (perpendicular to ground normal)
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x).normalized;
        // calculate change in position for this timestep (assumes constant acceleration)
        Vector2 deltaPosition = velocity * Time.deltaTime + 0.5f * acceleration * Time.deltaTime * Time.deltaTime;
        // calculate movement vector along ground direction
        Vector2 move = moveAlongGround * deltaPosition.x;
        // move along ground
        Movement(move, false);
        // create y axis movement vector
        move = Vector2.up * deltaPosition.y;
        // move the object along the y axis
        Movement(move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        // get magnitude of movement
        float distance = move.magnitude;
        // decide whether to check for collisions or not
        if (distance > minMoveDistance)
        {
            int collisionCount = rb2D.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            // copy only actual contacts from hitbuffer list
            hitBufferList.Clear();

            for(int i = 0; i<collisionCount; i++)
            {
                // copy over first 'count' collisions from hitBuffer to hitBufferList
                hitBufferList.Add(hitBuffer[i]);
            }

            for(int i = 0; i < hitBufferList.Count; i++)
            {
                // check current collision normal
                Vector2 currentNormal = hitBufferList[i].normal;
                // check if collision is with 'ground'
                if (currentNormal.y > minGroundNormalY)
                {
                    // player is grounded
                    positionState.grounded = true;
                    // if currently calculating y movement zero out x portion of collision normal
                    // this enables 'scraping' behavior on slope collision
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }
                // check if collision with left 'wall'
                else if (currentNormal.x > minWallNormalX)
                {
                    // update current state
                    positionState.pushLeft = true;
                    if (!prevState.pushLeft)
                    {
                        Debug.Log("pushes left!");
                    }
                }
                // check if collision with right 'wall'
                else if (currentNormal.x < -minWallNormalX)
                {
                    // update current state
                    positionState.pushRight = true;
                    if (!prevState.pushRight)
                    {
                        Debug.Log("Pushes Right!");
                    }
                }
                // get component of velocity in direction of ground
                float projection = Vector2.Dot(velocity, currentNormal);
                // if our velocity is in the opposite direction of the collision normal
                if (projection < 0)
                {
                    // subtract the velocity of the collision from our current velocity
                    velocity = velocity - projection * currentNormal;
                }
                // get the distance moved based on the collision
                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                // use the smaller of the two distances 
                // note: this is the collision property, if the collision distance is smaller than the original distance
                // it means we were going to hit something and need to move less to avoid going into it.)
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }
        // add delta position to current position
        rb2D.position = rb2D.position + move.normalized * distance;
    }
}
