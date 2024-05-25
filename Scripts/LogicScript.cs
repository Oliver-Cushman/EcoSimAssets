using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LogicScript : MonoBehaviour
{
    [SerializeField]
    GameObject foodPrefab;
    [SerializeField]
    GameObject creaturePrefab;
    [SerializeField]
    GameObject predatorPrefab;
    [SerializeField]
    private TMPro.TextMeshProUGUI dayText;
    [SerializeField]
    private TMPro.TextMeshProUGUI countText;
    
    private float dayTimer;
    private float nightTimer;
    private List<GameObject> creatures = new List<GameObject>();
    private int day;

    private readonly int INITIAL_FOOD = 500;
    private readonly int INCREMENT_FOOD = 150;
    private readonly int HERBIVORE_COUNT = 150;
    private readonly int PREDATOR_COUNT = 50;
    private readonly float LEFT_BOUND = -500f;
    private readonly float RIGHT_BOUND =  500f;
    private readonly float UPPER_BOUND = 500f;
    private readonly float LOWER_BOUND = -500f;
    private readonly float DAY_TIME = 30f;
    private readonly float NIGHT_TIME = 2f;
    private readonly string[] CREATURE_NAMES = { "Oliver", "Rudy", "Frank", "Alex", "Jacob", "Tina", "Hunter", "Meza", "Travis", "Hadizah" };

    // Start is called before the first frame update
    void Start()
    {

        day = 0;
        dayTimer = DAY_TIME / 2;
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

        countText.text = creatures.Count.ToString() + " Creatures Remaining";
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
        for (int i = 0; i < HERBIVORE_COUNT + PREDATOR_COUNT; i++)
        {
            string tendency = "";
            switch (Random.Range(0, 3)) {
                case 0:
                    tendency = "Left";
                    break;
                case 1:
                    tendency = "Right";
                    break;
                case 2:
                    tendency = "Ambi";
                    break;
            }

            if (i < PREDATOR_COUNT)
            {
                SpawnPredator(
                tendency,
                Random.Range(0, 2) == 0,
                new Vector2(Random.Range(LEFT_BOUND, RIGHT_BOUND), Random.Range(LOWER_BOUND, UPPER_BOUND)),
                Random.Range(0f, 360f));
            } else
            {
                SpawnHerbivore(
                tendency,
                Random.Range(0, 2) == 0,
                new Vector2(Random.Range(LEFT_BOUND, RIGHT_BOUND), Random.Range(LOWER_BOUND, UPPER_BOUND)),
                Random.Range(0f, 360f));
            }
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
                KillCreature(creatures[i]);
                i--;
            }
        }
    }

    public List<GameObject> GetCreatures() {
        return creatures;
    }

    public void SpawnCreature(string tendency, int daySpawned, string name, bool male, Vector2 position, float angle, bool predator) 
    {
        GameObject creature = Instantiate(predator ? predatorPrefab : creaturePrefab, position, Quaternion.Euler(0, 0, angle));
        creature.GetComponent<CreatureScript>().SetTraits(tendency, daySpawned, name, male, predator);
        creatures.Add(creature);
    }

    public void SpawnCreature(string tendency, string name, bool male, Vector2 position, float angle, bool predator) 
    {
        SpawnCreature(tendency, day, name, male, position, angle, predator);
    }

    public void SpawnCreature(string tendency, bool male, Vector2 position, float angle, bool predator) 
    {
        SpawnCreature(tendency, CREATURE_NAMES[Random.Range(0, CREATURE_NAMES.Length)], male, position, angle, predator);
    }
    
    public void SpawnHerbivore(string tendency, bool male, Vector2 position, float angle) {
        SpawnCreature(tendency, male, position, angle, false);
    }

    public void SpawnPredator(string tendency, bool male, Vector2 position, float angle)
    {
        SpawnCreature(tendency, male, position, angle, true);
    }

    public void KillCreature(GameObject creature)
    {
        Debug.Log("Death");
        creatures.Remove(creature);
        Destroy(creature.GetComponent<CreatureScript>().GetHome());
        Destroy(creature);
    }
}
