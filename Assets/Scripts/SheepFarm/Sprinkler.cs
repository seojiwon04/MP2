using UnityEngine;

namespace SheepFarm
{
    public class Sprinkler : MonoBehaviour
    {
        public Stat tank = new Stat(50, 100, -3);
        public float radius = 2.5f;

        Transform head;
        Transform gaugePivot;
        ParticleSystem spray;
        float waterTimer;

        void Start()
        {
            head = transform.Find("Head");
            gaugePivot = transform.Find("GaugePivot");
            spray = GetComponentInChildren<ParticleSystem>();
            transform.localScale = Vector3.one * 0.2f; // starts small and grows
        }

        void Update()
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, Time.deltaTime);

            bool firing = tank.value > 0;
            tank.Tick(Time.deltaTime);

            if (firing)
            {
                waterTimer += Time.deltaTime;
                if (waterTimer >= 1)
                {
                    waterTimer = 0;
                    WaterNearby();
                }
            }

            if (firing && !spray.isPlaying) spray.Play();
            if (!firing && spray.isPlaying) spray.Stop();

            // Head droops when empty, gauge shows how full the tank is.
            head.localRotation = firing ? Quaternion.identity : Quaternion.Euler(70, 0, 0);
            gaugePivot.localScale = new Vector3(1, Mathf.Max(0.02f, tank.Fraction()), 1);
        }

        void WaterNearby()
        {
            foreach (Collider c in Physics.OverlapSphere(transform.position, radius))
            {
                TilledPlot plot = c.GetComponentInParent<TilledPlot>();
                if (plot != null) plot.Water(8);
                Grass grass = c.GetComponentInParent<Grass>();
                if (grass != null) grass.Water(4);
            }
        }

        public void Refill(float amount)
        {
            tank.Add(amount);
        }
    }
}
