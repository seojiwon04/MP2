using UnityEngine;

namespace SheepFarm
{
    // Touch the bag to a tilled plot to plant grass. Costs 1 Seed.
    public class SeedBag : MonoBehaviour
    {
        public GameObject grassPrefab;

        Collider bagCollider;

        void Start()
        {
            bagCollider = GetComponent<Collider>();
        }

        void Update()
        {
            // Checked by overlap because a held object is kinematic and won't get collision events.
            Bounds b = bagCollider.bounds;
            foreach (Collider c in Physics.OverlapBox(b.center, b.extents * 1.05f))
            {
                TilledPlot plot = c.GetComponentInParent<TilledPlot>();
                if (plot == null || plot.grass != null) continue;
                if (!PlayerResources.Instance.Spend("Seeds", 1)) return;

                GameObject g = Instantiate(grassPrefab, plot.transform.position + Vector3.up * 0.2f, Quaternion.identity);
                Grass grass = g.GetComponent<Grass>();
                grass.growth.value = 5;
                grass.plot = plot;
                plot.grass = grass;
                PlayerResources.Instance.Add("Planted", 1);
            }
        }
    }
}
