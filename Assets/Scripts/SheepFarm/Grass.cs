using UnityEngine;

namespace SheepFarm
{
    public class Grass : MonoBehaviour
    {
        public Stat growth = new Stat(30, 100, 2);
        public Stat bitesLeft = new Stat(3, 3, 0);
        public float edibleAt = 30;
        public TilledPlot plot; // set when planted on a plot

        Transform model;
        Renderer rend;
        MaterialPropertyBlock block;

        void Start()
        {
            model = transform.Find("Model");
            rend = model.GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            bitesLeft.OnZero = Die;
            PlayerResources.Instance.Add("Grass", 1);
        }

        bool IsDry()
        {
            return plot != null && plot.moisture.value <= 0;
        }

        void Update()
        {
            if (!IsDry()) growth.Tick(Time.deltaTime);

            // Taller as it grows, shorter with each bite, brown when its soil is dry.
            float height = Mathf.Lerp(0.2f, 1f, growth.Fraction()) * Mathf.Lerp(0.4f, 1f, bitesLeft.Fraction());
            model.localScale = new Vector3(1, height, 1);
            block.SetColor("_BaseColor", IsDry() ? new Color(0.6f, 0.45f, 0.2f) : Color.white);
            rend.SetPropertyBlock(block);
        }

        public void Water(float amount)
        {
            growth.Add(amount);
            if (plot != null) plot.Water(amount);
        }

        public bool CanBeEaten()
        {
            return growth.value >= edibleAt;
        }

        public void Bite()
        {
            bitesLeft.Add(-1);
        }

        void Die()
        {
            PlayerResources.Instance.Add("Grass", -1);
            Destroy(gameObject);
        }
    }
}
