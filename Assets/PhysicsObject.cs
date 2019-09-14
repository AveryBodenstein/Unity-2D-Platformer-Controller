using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float gravityModifier = 1f;  // allows user to change strength of gravity
    public float minGroundNormalY = 0.65f; // how flat a surface needs to be to be considered 'ground'

    // the protected keyword here protects these variables from being globally changed by a derivative class. 
    // Each derivative class can change it's own local version of these variables, but these changes do not propagate
    // back to the parent class (PhysicsObject)
    protected bool grounded;
    protected Vector2 groundNormal;

    protected Vector2 targetVelocity;
    protected Vector2 velocity;
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
    }
    // Start is called before the first frame update
    void Start()
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
        // initialize player as ungrounded
        grounded = false;
        // constantly add velocity due to gravity
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
        // add input velocity in x direction
        velocity.x = targetVelocity.x;
        // create vector along the ground movement direction (perpendicular to ground normal)
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        // calculate change in position for this timestep
        Vector2 deltaPosition = velocity * Time.deltaTime;
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
                    grounded = true;
                    // if currently calculating y movement zero out x portion of collision normal
                    // this enables 'scraping' behavior on slope collision
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
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
