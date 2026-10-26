using UnityEngine;
using UnityEngine.UI;

public class FishingLiftNet : MonoBehaviour
{
    public float maxDurability = 100f;
    public float durability = 100f;
    public float durabilityLossRate = 0.5f;

    public float capacity = 20f;
    public float fishWeight;
    public float catchRate = 0.5f;

    public Slider catchWeightSlider;
    public Slider durabilitySlider;

    void Update()
    {
        durability -= durabilityLossRate * Time.deltaTime;

        if (durability <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        fishWeight = Mathf.Min(
            capacity,
            fishWeight + catchRate * Time.deltaTime
        );

        catchWeightSlider.value = fishWeight / capacity;
        durabilitySlider.value = durability / maxDurability;
    }

    public void CollectFish()
    {
        FishingResources resources = FishingResources.Instance;
        if (resources == null)
            return;

        resources.fishWeight += Mathf.FloorToInt(fishWeight);
        fishWeight = 0f;
    }

    public void Repair(float amount)
    {
        durability = Mathf.Min(maxDurability, durability + amount);
    }
}
