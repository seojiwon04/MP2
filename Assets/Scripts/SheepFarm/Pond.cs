using UnityEngine;

namespace SheepFarm
{
    public class Pond : MonoBehaviour
    {
        public Stat waterLevel = new Stat(100, 100, 2);
        public ErrorPopup errorPopup;

        Transform waterPivot;

        void Start()
        {
            waterPivot = transform.Find("WaterPivot");
        }

        void Update()
        {
            waterLevel.Tick(Time.deltaTime);

            // The water gets lower as it is drained.
            waterPivot.localScale = new Vector3(1, Mathf.Max(0.02f, waterLevel.Fraction()), 1);
        }

        // Removes up to "amount" water and returns how much was actually taken.
        public float Take(float amount)
        {
            float taken = Mathf.Min(amount, waterLevel.value);
            waterLevel.Add(-taken);
            return taken;
        }
    }
}
