using UnityEngine;

namespace SheepFarm
{
    public static class GridSnap
    {
        public static float cellSize = 1f;

        // Rounds a position to the nearest grid cell on the ground.
        public static Vector3 Snap(Vector3 p)
        {
            return new Vector3(Mathf.Round(p.x / cellSize) * cellSize, 0, Mathf.Round(p.z / cellSize) * cellSize);
        }

        // True if nothing farm-related is already in this spot.
        public static bool IsFree(Vector3 p)
        {
            foreach (Collider c in Physics.OverlapSphere(p + Vector3.up * 0.2f, 0.4f))
            {
                if (c.GetComponentInParent<TilledPlot>() || c.GetComponentInParent<Pond>()
                    || c.GetComponentInParent<Sprinkler>() || c.GetComponentInParent<Grass>())
                    return false;
            }
            return true;
        }
    }
}
