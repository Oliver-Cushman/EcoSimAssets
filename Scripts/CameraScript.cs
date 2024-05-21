using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [SerializeField]
    private Rigidbody2D rb;

    private float xVelocity;
    private float yVelocity;

    private readonly float SPEED = 10f;


    // Start is called before the first frame update
    void Start()
    {
        xVelocity = 0;
        yVelocity = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A)) 
        {
            xVelocity = -1;
        } else if (Input.GetKey(KeyCode.D)) 
        {
            xVelocity = 1;
        } else 
        {
            xVelocity = 0;
        }

        if (Input.GetKey(KeyCode.W)) 
        {
            yVelocity = 1;
        } else if (Input.GetKey(KeyCode.S)) 
        {
            yVelocity = -1;
        } else
        {
            yVelocity = 0;
        }

        rb.velocity = new Vector2(xVelocity * SPEED, yVelocity * SPEED);
    }
}
