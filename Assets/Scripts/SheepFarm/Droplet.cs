using UnityEngine;

namespace SheepFarm
{
    // A water drop sprayed by the watering can. Waters whatever it hits.
    public class Droplet : MonoBehaviour
    {
        public float amount = 2;

        void Start()
        {
            Destroy(gameObject, 3f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger || other is CharacterController) return;

            Grass grass = other.GetComponentInParent<Grass>();
            TilledPlot plot = other.GetComponentInParent<TilledPlot>();
            Sprinkler sprinkler = other.GetComponentInParent<Sprinkler>();

            if (grass != null) grass.Water(amount);
            else if (plot != null) plot.Water(amount);
            else if (sprinkler != null) sprinkler.Refill(amount);

            Destroy(gameObject);
        }
    }
}
