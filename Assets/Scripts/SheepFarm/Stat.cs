using UnityEngine;

namespace SheepFarm
{
    // A number that changes over time (Euler integration: value += rate * dt).
    [System.Serializable]
    public class Stat
    {
        public float value;
        public float min = 0;
        public float max = 100;
        public float rate;
        public System.Action OnZero;

        public Stat() { }

        public Stat(float value, float max, float rate)
        {
            this.value = value;
            this.max = max;
            this.rate = rate;
        }

        public void Tick(float dt)
        {
            Add(rate * dt);
        }

        public void Add(float amount)
        {
            float old = value;
            value = Mathf.Clamp(value + amount, min, max);
            if (old > min && value <= min && OnZero != null) OnZero();
        }

        public float Fraction()
        {
            return (value - min) / (max - min);
        }
    }
}
