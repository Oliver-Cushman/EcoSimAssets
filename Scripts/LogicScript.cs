using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicScript : MonoBehaviour
{
    [SerializeField]
    GameObject foodPrefab;
    [SerializeField]
    GameObject creaturePrefab;
    
    private float dayTimer;
    private float nightTimer;
    private List<GameObject> creatures = new List<GameObject>();
    private int day;

    private int FOOD_COUNT = 10;
    private int CREATURE_COUNT = 5;
    private float LEFT_BOUND = -20f;
    private float RIGHT_BOUND =  20f;
    private float UPPER_BOUND = 20f;
    private float LOWER_BOUND = -20f;
    private float DAY_TIME = 10f;
    private float NIGHT_TIME = 5f;

    // Start is called before the first frame update
    void Start()
    {

        day = 1;
        dayTimer = DAY_TIME;
        nightTimer = NIGHT_TIME;

        spawnFood();
        spawnCreatures();
    }

    // Update is called once per frame
    void Update()
    {

        if (dayTimer <= 0f)
        {
            if (nightTimer == NIGHT_TIME)
                endDay();

            nightTimer -= Time.deltaTime;

            if (nightTimer <= 0f)
            {
                dayTimer = DAY_TIME;
                nightTimer = NIGHT_TIME;

                startDay();
            }

        } else
        {
            dayTimer -= Time.deltaTime;
        }
    }
    
    private void spawnFood()
    {
        for (int i = 0; i < FOOD_COUNT; i++)
        {
            Instantiate(
                foodPrefab, 
                new Vector2(Random.Range(LEFT_BOUND, RIGHT_BOUND), Random.Range(LOWER_BOUND, UPPER_BOUND)), 
                transform.rotation);
        }
    }

    private void spawnCreatures()
    {
        for (int i = 0; i < CREATURE_COUNT; i++)
        {
            creatures.Add(
                Instantiate(
                    creaturePrefab, 
                    new Vector2(Random.Range(LEFT_BOUND, RIGHT_BOUND), Random.Range(LOWER_BOUND, UPPER_BOUND)), 
                    Quaternion.Euler(0, 0, Random.Range(0f, 360f))));
        }
    }

    private void endDay()
    {
        killCreatures();
    }

    private void startDay()
    {
        day++;

        spawnFood();

        creatures.ForEach(resetCreature);

        Debug.Log(day);
    }

    private void resetCreature(GameObject creature)
    {
        if (creature != null)
        {
            creature.GetComponent<CreatureScript>().enoughFoodConsumed = false;
            creature.GetComponent<CreatureScript>().sheltered = false;
        }
    }

    private void killCreatures()
    {
        for (int i = 0; i < creatures.Count; i++)
        {
            GameObject creature = creatures[i];
            if (creature != null && (!creature.GetComponent<CreatureScript>().enoughFoodConsumed || !creature.GetComponent<CreatureScript>().sheltered))
            {
                creatures.Remove(creature);
                Destroy(creature);
                i--;
                Debug.Log("Death");
            }
        }
    }
}
