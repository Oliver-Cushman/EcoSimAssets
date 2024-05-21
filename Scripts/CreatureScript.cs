using System.Collections;
using static System.Math;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class CreatureScript : MonoBehaviour
{
    private int currentDirection;
    private int currentFood;
    private float speed;
    private Vector2 home;
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
    private readonly float PRIMARY_DIRECTION_PROBABILITY = 0.7f;

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

        Debug.Log(tendency);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.GetChild(0).gameObject.transform.position, transform.up);
        if (enoughFoodConsumed)
        {
            speed = BASE_SPEED * CHASE_MULTIPLIER;

            GoToTarget(home);

        } else if (hit.collider != null && hit.collider.gameObject.tag.Equals("Food"))
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
        if (col.gameObject.tag == "Food")
        {
            Destroy(col.gameObject);
            currentFood++;
            if (currentFood >= foodRequired)
            {
                enoughFoodConsumed = true;
            }
        }
    }

    private void GoToTarget(Vector2 targetPos)
    {
        float distanceX = targetPos.x - transform.position.x;
        float distanceY = targetPos.y - transform.position.y;

        float desiredAOA = -Mathf.Atan2(distanceX, distanceY) * Mathf.Rad2Deg;
        
        float currentAngle = transform.rotation.eulerAngles.z;

        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        float minusAtan = currentAngle - desiredAOA;

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

        Debug.DrawRay(
            transform.position, 
            transform.up * (float) Sqrt((double) (distanceX * distanceX + distanceY * distanceY)), 
            Color.green);
    }

    private bool NearPose(Vector2 pose) {
        return Abs(pose.x - transform.position.x) < POSITION_TARGET_DEADBAND && Abs(pose.y - transform.position.y) < POSITION_TARGET_DEADBAND;
    }
}
