using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Fix Slope jumping (nearly no horizontal velocity possible)
// TODO: Implement minimum jump height input?
// TODO: Implement maximum fall speed
// TODO: Implement incresed fall speed on down arrow key press
// TODO: Implement oneshot audio when hitting walls/floor/ceiling
// TODO: Implement "jump leniency" allowing jumping slightly after leaving a platform
// TODO: stop upward momentum when hitting a side wall? right now character slides unrealistically


public class PlayerPlatformerController : PhysicsObject
{

    public float jumpHeight = 2f;
    public float jumpDistance = 3f;
    public float fallDistance = 1.5f;
    // public float jumpTakeOffSpeed = 7;
    public float maxSpeed = 7f;

    private SpriteRenderer spriteR;
    private Animator animator;
    private float jumpTakeOffSpeed;
    private Vector2 defaultGravity;
    private Vector2 fallGravity;

    void Awake()
    {
        // get components of player object
        spriteR = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }

    // called right before the first frame
    private void Start()
    {
        // calculate jump take off speed and gravity
        jumpTakeOffSpeed = (2 * jumpHeight * maxSpeed) / jumpHeight;
        gravity = (2 * jumpHeight * maxSpeed * maxSpeed) / (jumpHeight * jumpHeight) * Physics2D.gravity.normalized;
        defaultGravity = gravity;
        fallGravity = gravity * (jumpDistance / fallDistance);
        // Debug.Log(string.Format("jumpTakeOffSpeed {0}\n", jumpTakeOffSpeed));
        // Debug.Log(string.Format("Awake gravity {0}\n", gravity));
    }

    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;
        // get input on horizontal axis
        move.x = Input.GetAxis("Horizontal");
        // if jump button is held down and we are on the ground
        if (Input.GetButtonDown("Jump") && grounded)
        {
            gravity = defaultGravity;
            velocity.y = jumpTakeOffSpeed;
        }
        // when the player releases the jump button
        else if (Input.GetButtonUp("Jump"))
        {
            // if they are still travelling upward
            if (velocity.y > 0)
            {
                // change gravity to reduce jump height
                gravity = fallGravity;
            }
        }

        // check if sprite needs to be flipped
        bool flipSprite = (spriteR.flipX ? (move.x > 0.01f) : (move.x < 0.01f));
        if (flipSprite)
        {
            spriteR.flipX = !spriteR.flipX;
        }

        // check if we are falling
        if (velocity.y < 0 && gravity != fallGravity)
        {
            gravity = fallGravity;
        }

        // set animator boolean "grounded" to the value of our grounded variable
        animator.SetBool("grounded", grounded);
        // set animator float "velocityX" to percentage of full speed
        animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

        // set target velocity based on input and max speed
        targetVelocity = move * maxSpeed;
    }
}
