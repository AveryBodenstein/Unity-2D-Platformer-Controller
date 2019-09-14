using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autoMoveCrate : PhysicsObject
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // attempt to move to the left
        targetVelocity = Vector2.left;
    }
}
