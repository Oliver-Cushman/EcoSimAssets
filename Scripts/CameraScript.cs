using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private Camera cam;

    private float xVelocity;
    private float yVelocity;
    private float scale;

    private readonly float SPEED = 20f;
    private readonly float CAMERA_BASE_SIZE = 20f;
    private readonly float ZOOM_INCREMENT = 5f;
    private readonly float MIN_ZOOM = 10f;
    private readonly float MAX_ZOOM = 100f;


    // Start is called before the first frame update
    void Start()
    {
        xVelocity = 0;
        yVelocity = 0;
        scale = CAMERA_BASE_SIZE;
    }

    // Update is called once per frame
    void Update()
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

        rb.velocity = new Vector2(xVelocity * SPEED * (scale / CAMERA_BASE_SIZE), yVelocity * SPEED * (scale / CAMERA_BASE_SIZE));

        if (Input.GetKeyDown(KeyCode.Q) && scale < MAX_ZOOM) 
        {
            scale += ZOOM_INCREMENT;
        } else if (Input.GetKeyDown(KeyCode.E) && scale > MIN_ZOOM) 
        {
            scale -= ZOOM_INCREMENT;
        }

        cam.orthographicSize = scale;
    }
}
