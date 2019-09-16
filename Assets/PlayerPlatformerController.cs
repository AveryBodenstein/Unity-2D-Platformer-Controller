using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : PhysicsObject
{

    public float jumpHeight = 2;
    public float jumpDistance = 3;
    // public float jumpTakeOffSpeed = 7;
    public float maxSpeed = 7;

    private SpriteRenderer spriteR;
    private Animator animator;
    private float jumpTakeOffSpeed;

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
            velocity.y = jumpTakeOffSpeed;
        }
        // when the player releases the jump button
        else if (Input.GetButtonUp("Jump"))
        {
            // if they are still travelling upward
            if (velocity.y > 0)
            {
                // reduce their velocity in the y direction by half
                // TODO: Change this to affect the gravity instead of the acceleration
                velocity.y = velocity.y * 0.5f;
            }
        }

        // check if sprite needs to be flipped
        bool flipSprite = (spriteR.flipX ? (move.x > 0.01f) : (move.x < 0.01f));
        if (flipSprite)
        {
            spriteR.flipX = !spriteR.flipX;
        }

        // set animator boolean "grounded" to the value of our grounded variable
        animator.SetBool("grounded", grounded);
        // set animator float "velocityX" to percentage of full speed
        animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

        // set target velocity based on input and max speed
        targetVelocity = move * maxSpeed;
    }
}
