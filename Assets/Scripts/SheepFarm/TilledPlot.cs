using UnityEngine;

namespace SheepFarm
{
    public class TilledPlot : MonoBehaviour
    {
        public Stat moisture = new Stat(30, 100, -1.5f);
        public Grass grass; // the grass planted here, if any

        Renderer rend;
        MaterialPropertyBlock block;

        void Start()
        {
            rend = GetComponentInChildren<Renderer>();
            block = new MaterialPropertyBlock();
        }

        void Update()
        {
            moisture.Tick(Time.deltaTime);

            // Darker soil when wet.
            Color wet = new Color(0.4f, 0.3f, 0.25f);
            block.SetColor("_BaseColor", Color.Lerp(Color.white, wet, moisture.Fraction()));
            rend.SetPropertyBlock(block);
        }

        public void Water(float amount)
        {
            moisture.Add(amount);
        }
    }
}
