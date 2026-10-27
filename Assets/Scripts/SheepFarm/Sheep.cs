using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace SheepFarm
{
    public class Sheep : MonoBehaviour
    {
        public Stat hunger = new Stat(0, 100, 2);
        public Stat wool = new Stat(100, 100, 4);
        public Stat health = new Stat(100, 100, 1);
        public float hungryAt = 50;

        NavMeshAgent agent;
        Transform model;
        TextMeshPro statsText;
        Vector3 baseScale;
        Grass target;
        bool eating;
        float wanderTimer, searchTimer, eatTimer;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            model = transform.Find("Model");
            statsText = transform.Find("Stats").GetComponent<TextMeshPro>();
            baseScale = model.localScale;

            hunger.value = Random.Range(0f, 40f);
            health.OnZero = Die;
            PlayerResources.Instance.Add("Sheep", 1);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            hunger.Tick(dt);
            wool.Tick(dt);
            health.rate = hunger.value >= hunger.max ? -5 : 1; // starving hurts
            health.Tick(dt);

            // Start eating when hungry, keep eating until full.
            if (hunger.value > hungryAt) eating = true;
            if (hunger.value <= 0) eating = false;
            if (eating) GoEat(dt);
            else Wander(dt);

            // Fluffier with more wool, thinner with less health.
            float fluff = Mathf.Lerp(0.85f, 1.25f, wool.Fraction());
            float thin = Mathf.Lerp(0.6f, 1f, health.Fraction());
            model.localScale = new Vector3(baseScale.x * fluff * thin, baseScale.y * fluff, baseScale.z);

            statsText.text = "Wool: " + Mathf.Floor(wool.value) + "\nHunger: " + Mathf.Floor(hunger.value) + "\nHealth: " + Mathf.Floor(health.value);
        }

        void Wander(float dt)
        {
            target = null;
            wanderTimer -= dt;
            if (wanderTimer > 0) return;

            wanderTimer = Random.Range(3f, 7f);
            Vector3 point = transform.position + Random.insideUnitSphere * 6f;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        void GoEat(float dt)
        {
            searchTimer -= dt;
            if (target == null && searchTimer <= 0)
            {
                searchTimer = 1;
                target = FindNearestGrass();
            }
            if (target == null)
            {
                Wander(dt);
                return;
            }

            Vector3 toGrass = target.transform.position - transform.position;
            toGrass.y = 0;
            if (toGrass.magnitude > 1f)
            {
                agent.SetDestination(target.transform.position);
                eatTimer = 0;
            }
            else
            {
                agent.ResetPath();
                eatTimer += dt;
                if (eatTimer >= 1.5f)
                {
                    eatTimer = 0;
                    target.Bite();
                    hunger.Add(-25);
                }
            }
        }

        Grass FindNearestGrass()
        {
            Grass best = null;
            float bestDist = Mathf.Infinity;
            foreach (Grass g in FindObjectsByType<Grass>())
            {
                if (!g.CanBeEaten()) continue;
                float d = Vector3.Distance(transform.position, g.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = g;
                }
            }
            return best;
        }

        public void Shear()
        {
            if (wool.value < 25) return;
            float sheared = Mathf.Floor(wool.value / 25);
            PlayerResources.Instance.Add("Wool", sheared);
            PlayerResources.Instance.Add("Wool Collected", sheared); // total ever collected, for unlocks
            wool.value = 0;
        }

        void Die()
        {
            PlayerResources.Instance.Add("Sheep", -1);
            Destroy(gameObject);
        }
    }
}
