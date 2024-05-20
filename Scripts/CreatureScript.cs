using System.Collections;
using static System.Math;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly float ANGULAR_VELOCITY = 180f;
    private readonly float TARGET_DEADBAND = 0.2f;
    private readonly float DIRECTION_CHANGE_TIME = 0.1f;
    private readonly float BASE_SPEED = 10f;
    private readonly float TURN_PROBABILITY = 0.800f;
    private readonly float CHASE_MULTIPLIER = 1.5f;

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
                float randDirection = Random.Range(0f, 1f);
                if (randDirection <= TURN_PROBABILITY)
                {   
                    currentDirection = (int) Random.Range(-2f, 2f);
                }
            }

        }

        if (Abs(home.x - transform.position.x) < TARGET_DEADBAND && Abs(home.y - transform.position.y) < TARGET_DEADBAND && enoughFoodConsumed)
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

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.gameObject.tag == "Food")
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

        float desiredAOA = -1 * Mathf.Atan2(distanceX, distanceY) * Mathf.Rad2Deg;
        
        float currentAngle = transform.rotation.eulerAngles.z;

        if (currentAngle >= 180)
        {
            currentAngle -= 360f;
        }

        float minusAtan = currentAngle - desiredAOA;

        if (minusAtan < -TARGET_DEADBAND)
        {
            currentDirection = 1;
        } else if (minusAtan > TARGET_DEADBAND) 
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
}
