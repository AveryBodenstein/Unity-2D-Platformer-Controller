using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : PhysicsObject
{
    public GameObject[] wayPointObjects;

    public float moveSpeed = 1.5f;
    public float pauseTime = 0.1f; // time to pause at each waypoint (seconds)

    private int currentWaypoint = 0;
    private int nextWaypoint = 1;
    private Vector2 moveAlong;
    private int nWaypoints;

    private float waitTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        // get the number of waypoints
        nWaypoints = wayPointObjects.Length;
        // calculate vector from waypoint 1 to waypoint 2
        moveAlong.Set(wayPointObjects[1].transform.position.x - wayPointObjects[0].transform.position.x, wayPointObjects[1].transform.position.y - wayPointObjects[0].transform.position.y);
        moveAlong.Normalize();
    }

    // FixedUpdate is called consistently for physics simulation
    private void FixedUpdate()
    {
        
        float distance;
        // check if we should be waiting
        if (waitTimer > 0.0f)
        {
            waitTimer -= Time.deltaTime;
        }
        else
        {
            // calculate distance to move
            distance = moveSpeed * Time.deltaTime;
            // check if this distance places us at or past the waypoint
            if (Vector2.Distance(rb2D.position,wayPointObjects[nextWaypoint].transform.position) <= distance)
            {
                // move platform to waypoint
                rb2D.position = wayPointObjects[nextWaypoint].transform.position;
                // trigger wait state
                waitTimer = pauseTime;
                // switch targets
                currentWaypoint = (currentWaypoint + 1) % nWaypoints;
                nextWaypoint = (currentWaypoint + 1) % nWaypoints;
                // calculate vector from waypoint 1 to waypoint 2
                moveAlong.Set(wayPointObjects[nextWaypoint].transform.position.x - wayPointObjects[currentWaypoint].transform.position.x, wayPointObjects[nextWaypoint].transform.position.y - wayPointObjects[currentWaypoint].transform.position.y);
                moveAlong.Normalize();
            }
            else
            {
                // move platform
                rb2D.position = rb2D.position + moveAlong * distance;
            }
        }
    }
}
