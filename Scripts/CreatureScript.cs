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
    private string creatureName;
    private string[] creatureNames = { "Oliver", "Rin", "Rudy", "Frank", "Alex", "Jacob", "Tina", "Hunter", "Meza", "Travis", "Hadizah" };
    private int currentDirection;
    private int currentFood;
    private float speed;
    private UnityEngine.Vector2 home;
    private float directionChangeTimer;
    public bool enoughFoodConsumed;
    public bool sheltered = false;
    private int foodRequired;
    private DirectionalTendency tendency;

    private enum DirectionalTendency
    {
        Left,
        Right,
        Ambi
    }

    private readonly float ANGULAR_VELOCITY = 720f;
    private readonly float ANGLE_TARGET_DEADBAND = 2f;
    private readonly float POSITION_TARGET_DEADBAND = 0.2f;
    private readonly float DIRECTION_CHANGE_TIME = 0.1f;
    private readonly float BASE_SPEED = 20f;
    private readonly float TURN_PROBABILITY = 0.800f;
    private readonly float CHASE_MULTIPLIER = 1.5f;
    private readonly float PRIMARY_DIRECTION_PROBABILITY = 0.5f;

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

            GoToTarget(home);

        } else if (hit.collider != null && hit.collider.gameObject.CompareTag("Food"))
        {
            speed = BASE_SPEED * CHASE_MULTIPLIER;

            GoToTarget(hit.collider.gameObject.transform.position);
            
        } else
        {
            speed = BASE_SPEED;
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
            rb.angularVelocity = ANGULAR_VELOCITY * currentDirection;
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
        UnityEngine.Vector3 targetDir = new UnityEngine.Vector3(targetPos.x, targetPos.y, 0f) - transform.position;

        // TODO: SOMETHING AB THIS MATH IS NOT MATHING AND I FEEL STUPID

        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        
        // ITS PROBABLY THIS
        float currentAngle = transform.rotation.eulerAngles.z;

        if (currentAngle > 180)
        {
            currentAngle -= 360f;
        }

        float minusAtan = currentAngle - targetAngle;

        if (minusAtan < -ANGLE_TARGET_DEADBAND)
        {
            currentDirection = 1;
        } else if (minusAtan > ANGLE_TARGET_DEADBAND) 
        {
            currentDirection = -1;
        } else
        {
            currentDirection = 0;
        }

        UnityEngine.Vector2 dir = targetPos - (UnityEngine.Vector2) transform.position;

        Debug.DrawRay(
            transform.position, 
            dir.normalized * (float) Sqrt(targetDir.x * targetDir.x + targetDir.y * targetDir.y), 
            Color.green);
    }

    private bool NearPose(UnityEngine.Vector2 pose) {
        return Abs(pose.x - transform.position.x) < POSITION_TARGET_DEADBAND && Abs(pose.y - transform.position.y) < POSITION_TARGET_DEADBAND;
    }

    public string GetName() {
        return creatureName;
    }
}