using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private LogicScript logic;
    [SerializeField]
    private TMPro.TextMeshProUGUI text;

    private float xVelocity;
    private float yVelocity;
    private float scale;
    private GameObject currentCreature;
    private int currentCreatureIndex;

    private readonly float SPEED = 40f;
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
        currentCreature = null;
        currentCreatureIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCreature == null)
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

        } else
        {
            Vector2 creaturePose = currentCreature.transform.position;
            transform.position = new Vector3(creaturePose.x, creaturePose.y, -10f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentCreature = null;
            text.text = "";
        } else if (Input.GetKeyDown(KeyCode.G))
        {
            List<GameObject> creatures = logic.GetCreatures();
            if (creatures.Count > 0)
            {
                currentCreatureIndex++;
                currentCreatureIndex %= logic.GetCreatures().Count;
                currentCreature = logic.GetCreatures()[currentCreatureIndex];
                text.text = "Watching: " + currentCreature.GetComponent<CreatureScript>().GetName();
            }
        }

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
