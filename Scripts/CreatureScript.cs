using System.Collections;
using static System.Math;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Runtime.CompilerServices;
using System.Numerics;

public class CreatureScript : MonoBehaviour
{
    private bool enoughFoodConsumed;
    private bool sheltered;
    private string[] creatureNames = { "Oliver", "Rin", "Rudy", "Frank", "Alex", "Jacob", "Tina", "Hunter", "Meza", "Travis", "Hadizah" };
    private float speed;
    private int currentDirection;
    private int currentFood;
    private float directionChangeTimer;

    private float angularVelocity;
    private int foodRequired;
    private string creatureName;
    private UnityEngine.Vector2 home;
    private DirectionalTendency tendency;

    public float getAngularVelocity()
    {
        return this.angularVelocity;
    }

    public string GetCreatureName()
    {
        return creatureName;
    }


    public UnityEngine.Vector2 GetHome()
    {
        return home;
    }


    public String GetTendency()
    {
        return tendency.ToString();
    }

    private enum DirectionalTendency
    {
        Left,
        Right,
        Ambi
    }

    private readonly float CHARGE_ANGULAR_VELOCITY = 720f;
    private readonly float SEARCH_ANGULAR_VELOCITY = 60f; 
    private readonly float POSITION_TARGET_DEADBAND = 1f;
    private readonly float DIRECTION_CHANGE_TIME = 0.1f;
    private readonly float BASE_SPEED = 20f;
    private readonly float TURN_PROBABILITY = 0.800f;
    private readonly float CHASE_MULTIPLIER = 1.5f;
    private readonly float PRIMARY_DIRECTION_PROBABILITY = 0.800f;

    [SerializeField]
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        currentDirection = 0;
        speed = BASE_SPEED;
        rb.angularDrag = 0.0f;
        currentFood = 0;
        home = transform.position;
        directionChangeTimer = DIRECTION_CHANGE_TIME;
        enoughFoodConsumed = false;
        sheltered = false;
        foodRequired = 1;

        tendency = UnityEngine.Random.Range(0, 3) switch
            {
                0 => DirectionalTendency.Left,
                1 => DirectionalTendency.Right,
                _ => DirectionalTendency.Ambi,
            };

        creatureName = creatureNames[UnityEngine.Random.Range(0, creatureNames.Length)];
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.GetChild(0).gameObject.transform.position, transform.up);
        if (enoughFoodConsumed)
        {
            speed = BASE_SPEED * CHASE_MULTIPLIER;
            angularVelocity = CHARGE_ANGULAR_VELOCITY;

            GoToTarget(home);

        } else if (hit.collider != null && hit.collider.gameObject.CompareTag("Food"))
        {
            speed = BASE_SPEED * CHASE_MULTIPLIER;
            angularVelocity = SEARCH_ANGULAR_VELOCITY;

            GoToTarget(hit.collider.gameObject.transform.position);
            
        } else
        {
            speed = BASE_SPEED;
            angularVelocity = SEARCH_ANGULAR_VELOCITY;
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0)
            {
                directionChangeTimer = DIRECTION_CHANGE_TIME;
                float turnRoll = UnityEngine.Random.Range(0f, 1f);
                if (turnRoll <= TURN_PROBABILITY)
                {   
                    float rand = UnityEngine.Random.Range(0f, 1f);
                    switch (tendency)
                    {
                        case DirectionalTendency.Left:
                            if (rand <= PRIMARY_DIRECTION_PROBABILITY) 
                            {
                                currentDirection = -1;
                            } else
                            {
                                currentDirection = UnityEngine.Random.Range(0, 2);
                            }
                            break;
                        case DirectionalTendency.Right:
                            if (rand <= PRIMARY_DIRECTION_PROBABILITY) 
                            {
                                currentDirection = 1;
                            } else
                            {
                                currentDirection = UnityEngine.Random.Range(-1, 1);
                            }
                            break;
                        default:
                            currentDirection = UnityEngine.Random.Range(-1, 2);
                            break;
                    };
                }
            }

        }

        if (NearPose(home) && enoughFoodConsumed)
            sheltered = true;

        if (!sheltered)
        {
            rb.angularVelocity = angularVelocity * currentDirection;
            rb.velocity = transform.up * speed;
        } else
        {
            rb.angularVelocity = 0;
            rb.velocity *= 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Food"))
        {
            Destroy(col.gameObject);
            currentFood++;
            if (currentFood >= foodRequired)
            {
                enoughFoodConsumed = true;
            }
        }
    }

    private void GoToTarget(UnityEngine.Vector2 targetPos)
    {
        UnityEngine.Vector2 dir = targetPos - (UnityEngine.Vector2) transform.position;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        float currentAngle = transform.rotation.eulerAngles.z + 90f;

        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        targetAngle -= currentAngle;

        targetAngle = NormalizedAngle(targetAngle);

        if (targetAngle < 180)
        {
            // Debug.Log("Should go counterclockwise");
            currentDirection = 1;
        } else if (targetAngle > 180)
        {
            // Debug.Log("Should go clockwise");
            currentDirection = -1;
        } else 
        {
            // Debug.Log("Should go straight");
            currentDirection = 0;
        }

        Debug.DrawRay(
            transform.position, 
            dir.normalized * (float) Sqrt(dir.x * dir.x + dir.y * dir.y), 
            Color.green);
    }

    private bool NearPose(UnityEngine.Vector2 pose) {
        return Abs(pose.x - transform.position.x) < POSITION_TARGET_DEADBAND && Abs(pose.y - transform.position.y) < POSITION_TARGET_DEADBAND;
    }

    public float NormalizedAngle(float angle)
    {
        while (angle > 360f || angle < 0f) {
            angle += -360 * (angle / Abs(angle));
        }
        return angle;
    }

    public void Reset()
    {
        currentFood = 0;
        enoughFoodConsumed = false;
        sheltered = false;
    }

    public bool Safe() {
        return enoughFoodConsumed && sheltered;
    }
}