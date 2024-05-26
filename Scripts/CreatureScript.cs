using static System.Math;
using UnityEngine;
public class CreatureScript : MonoBehaviour
{
    private bool enoughFoodConsumed;
    private bool sheltered;
    private bool breeded;
    private float speed;
    private int currentDirection;
    private int currentFood;
    private float directionChangeTimer;

    private GameObject currentTarget;

    private float angularVelocity;
    private int foodRequired;
    private string creatureName;
    private GameObject home;
    private DirectionalTendency tendency;
    private int daySpawned;
    private bool male;
    private bool predator;
    private LogicScript logic;

    public bool GetPredator()
    {
        return predator;
    }

    public bool GetMale()
    {
        return male;
    }

    public string GetCreatureName()
    {
        return creatureName;
    }


    public GameObject GetHome()
    {
        return home;
    }


    public string GetTendency()
    {
        return tendency.ToString();
    }

    private enum DirectionalTendency
    {
        Left,
        Right,
        Ambi
    }

    private float BASE_SPEED = 30f;
    private readonly float PREDATOR_BASE_SPEED = 35f;
    private readonly float CHARGE_ANGULAR_VELOCITY = 720f;
    private readonly float SEARCH_ANGULAR_VELOCITY = 60f;
    private readonly float POSITION_TARGET_DEADBAND = 0.75f;
    private readonly float DIRECTION_CHANGE_TIME = 0.1f;
    private readonly float TURN_PROBABILITY = 0.800f;
    private readonly float CHASE_MULTIPLIER = 1.5f;
    private readonly float PRIMARY_DIRECTION_PROBABILITY = 0.800f;
    private readonly float SIGHT_RANGE = 200f;
    

    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private GameObject homePrefab;

    // Start is called before the first frame update
    void Start()
    {
        currentDirection = 0;
        speed = BASE_SPEED;
        rb.angularDrag = 0.0f;
        currentFood = 0;
        home = Instantiate(homePrefab, transform.position, transform.rotation);
        directionChangeTimer = DIRECTION_CHANGE_TIME;
        enoughFoodConsumed = true;
        sheltered = true;
        breeded = true;
        foodRequired = 1;
        currentTarget = null;
        logic = GameObject.Find("LogicManager").GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (currentTarget != null)
        {
            GoToTarget(currentTarget.transform.position);

            speed = BASE_SPEED * CHASE_MULTIPLIER;
            angularVelocity = CHARGE_ANGULAR_VELOCITY;
        }
        else
        {
            speed = BASE_SPEED;
            angularVelocity = SEARCH_ANGULAR_VELOCITY;

            RaycastHit2D hit = Physics2D.Raycast(transform.GetChild(0).gameObject.transform.position, transform.up);
            Collider2D target = hit.collider;
            
            if (ValidTarget(target) && IsFood(hit.collider.gameObject) && !enoughFoodConsumed)
            {
                SetTarget(hit.collider.gameObject);
            }
            else if (ValidTarget(target) && hit.collider.gameObject.CompareTag("Creature") && ShouldBreed() && IsCompatiblePartner(hit.collider.gameObject))
            {
                SetTarget(hit.collider.gameObject);
                AttractCreature(hit.collider.gameObject);

            }
            else if (enoughFoodConsumed && !ShouldBreed())
            {
                SetTarget(home);

            } else
            {
                directionChangeTimer -= Time.deltaTime;
                if (directionChangeTimer <= 0)
                {
                    directionChangeTimer = DIRECTION_CHANGE_TIME;
                    float turnRoll = Random.Range(0f, 1f);
                    if (turnRoll <= TURN_PROBABILITY)
                    {
                        float rand = Random.Range(0f, 1f);
                        switch (tendency)
                        {
                            case DirectionalTendency.Left:
                                if (rand <= PRIMARY_DIRECTION_PROBABILITY)
                                {
                                    currentDirection = -1;
                                }
                                else
                                {
                                    currentDirection = Random.Range(0, 2);
                                }
                                break;
                            case DirectionalTendency.Right:
                                if (rand <= PRIMARY_DIRECTION_PROBABILITY)
                                {
                                    currentDirection = 1;
                                }
                                else
                                {
                                    currentDirection = Random.Range(-1, 1);
                                }
                                break;
                            default:
                                currentDirection = Random.Range(-1, 2);
                                break;
                        };
                    }
                }
            }
        }

        if (NearPose(home.transform.position) && enoughFoodConsumed)
            sheltered = true;

        if (currentTarget != null && NearPose(currentTarget.transform.position))
            currentTarget = null;

        if (!sheltered)
        {
            rb.angularVelocity = angularVelocity * currentDirection;
            rb.velocity = transform.up * speed;
        }
        else
        {
            rb.angularVelocity = 0;
            rb.velocity *= 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (IsFood(col.gameObject))
        {
            if (predator)
            {
                logic.KillCreature(col.gameObject);
            } else 
            {
                Destroy(col.gameObject);
            }
            currentFood++;
            if (currentFood >= foodRequired)
            {
                enoughFoodConsumed = true;
            }
        }
        else if (col.gameObject.CompareTag("Creature") && ShouldBreed() && IsCompatiblePartner(col.gameObject))
        {
            breeded = true;

            if (!male) {
                logic.SpawnCreature(tendency.ToString(), Random.Range(0, 2) == 0, transform.position, transform.rotation.eulerAngles.z, predator);
                Debug.Log("Fucked");
            }
        }
    }

    private bool ValidTarget(Collider2D target)
    {
        if (target == null)
            return false;

        Vector2 dir = (Vector2) target.gameObject.transform.position - (Vector2) transform.position;
        
        return dir.magnitude <= SIGHT_RANGE;
    }

    private void GoToTarget(Vector2 targetPos)
    {
        Vector2 dir = targetPos - (Vector2)transform.position;

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
        }
        else if (targetAngle > 180)
        {
            // Debug.Log("Should go clockwise");
            currentDirection = -1;
        }
        else
        {
            // Debug.Log("Should go straight");
            currentDirection = 0;
        }

        Debug.DrawRay(
            transform.position,
            dir.normalized * (float)Sqrt(dir.x * dir.x + dir.y * dir.y),
            Color.green);
    }

    private bool NearPose(Vector2 pose)
    {
        return Abs(pose.x - transform.position.x) < POSITION_TARGET_DEADBAND && Abs(pose.y - transform.position.y) < POSITION_TARGET_DEADBAND;
    }

    private bool ShouldBreed()
    {
        return enoughFoodConsumed && !breeded;
    }

    private float NormalizedAngle(float angle)
    {
        while (angle > 360f || angle < 0f)
        {
            angle += -360 * (angle / Abs(angle));
        }
        return angle;
    }

    public void Reset()
    {
        currentFood = 0;
        enoughFoodConsumed = false;
        sheltered = false;
        breeded = false;
    }

    public bool Safe()
    {
        return enoughFoodConsumed && sheltered;
    }

    private bool IsFood(GameObject gameObject)
    {
        return 
            (gameObject.CompareTag("Food") && !predator) 
            || (gameObject.CompareTag("Creature") && !gameObject.GetComponent<CreatureScript>().GetPredator() && predator);
    }

    private bool IsCompatiblePartner(GameObject creature)
    {
        CreatureScript creatureLogic = creature.GetComponent<CreatureScript>();

        return creatureLogic.GetMale() != male && creatureLogic.GetPredator() == predator;
    }

    public void SetTraits(string tendency, int daySpawned, string name, bool male, bool predator)
    {
        this.tendency = tendency switch
        {
            "Left" => DirectionalTendency.Left,
            "Right" => DirectionalTendency.Right,
            _ => DirectionalTendency.Ambi,
        };

        this.daySpawned = daySpawned;
        this.creatureName = name;
        this.male = male;
        this.predator = predator;

        if (predator) {
            BASE_SPEED = PREDATOR_BASE_SPEED;
        }
    }

    public void SetTarget(GameObject target)
    {
        currentTarget = target;
    }

    private void AttractCreature(GameObject creature)
    {
        creature.GetComponent<CreatureScript>().SetTarget(transform.gameObject);
    }
}