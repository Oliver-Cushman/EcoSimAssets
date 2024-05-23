using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicScript : MonoBehaviour
{
    [SerializeField]
    GameObject foodPrefab;
    [SerializeField]
    GameObject creaturePrefab;
    [SerializeField]
    private TMPro.TextMeshProUGUI dayText;
    
    private float dayTimer;
    private float nightTimer;
    private List<GameObject> creatures = new List<GameObject>();
    private int day;

    private readonly int INITIAL_FOOD = 100;
    private readonly int INCREMENT_FOOD = 25;
    private readonly int CREATURE_COUNT = 30;
    private readonly float LEFT_BOUND = -100f;
    private readonly float RIGHT_BOUND =  100f;
    private readonly float UPPER_BOUND = 100f;
    private readonly float LOWER_BOUND = -100f;
    private readonly float DAY_TIME = 10f;
    private readonly float NIGHT_TIME = 2f;

    // Start is called before the first frame update
    void Start()
    {

        day = 1;
        dayTimer = DAY_TIME;
        nightTimer = NIGHT_TIME;

        dayText.text = "Day " + day;

        SpawnFood(true);
        SpawnCreatures();
    }

    // Update is called once per frame
    void Update()
    {

        if (dayTimer <= 0f)
        {
            if (nightTimer == NIGHT_TIME)
                EndDay();

            nightTimer -= Time.deltaTime;

            if (nightTimer <= 0f)
            {
                dayTimer = DAY_TIME;
                nightTimer = NIGHT_TIME;

                StartDay();
            }

        } else
        {
            dayTimer -= Time.deltaTime;
        }
    }
    
    private void SpawnFood(bool initial)
    {
        for (int i = 0; i < (initial ? INITIAL_FOOD : INCREMENT_FOOD); i++)
        {
            Instantiate(
                foodPrefab, 
                new Vector2(Random.Range(LEFT_BOUND, RIGHT_BOUND), Random.Range(LOWER_BOUND, UPPER_BOUND)), 
                transform.rotation);
        }
    }

    private void SpawnCreatures()
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

    private void EndDay()
    {
        KillCreatures();
    }

    private void StartDay()
    {
        day++;

        dayText.text = "Day " + day;

        SpawnFood(false);

        creatures.ForEach(ResetCreature);

        Debug.Log(day);
    }

    private void ResetCreature(GameObject creature)
    {
        if (creature != null)
        {
            creature.GetComponent<CreatureScript>().Reset();
        }
    }

    private void KillCreatures()
    {
        for (int i = 0; i < creatures.Count; i++)
        {
            GameObject creature = creatures[i];
            if (creature != null && (!creature.GetComponent<CreatureScript>().Safe()))
            {
                Debug.Log("Death: " + creature.GetComponent<CreatureScript>().GetTendency());
                creatures.Remove(creature);
                Destroy(creature);
                i--;
            }
        }
    }

    public List<GameObject> GetCreatures() {
        return creatures;
    }
}
