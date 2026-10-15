using UnityEngine;
using UnityEngine.UI;

public class FishingLiftNet : MonoBehaviour
{
    public float maxDurability = 100f;
    public float durability = 100f;
    public float durabilityLossRate = 1f;

    public float capacity = 50f;
    public float fishWeight;
    public float sharkWeight;
    public float catchRate = 2f;
    [Range(0f, 1f)] public float sharkChance = 0.2f;

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

        float remaining = capacity - fishWeight - sharkWeight;
        
        if (remaining > 0f)
        {
            float caught = Mathf.Min(catchRate * Time.deltaTime, remaining);

            if (Random.value < sharkChance)
                sharkWeight += caught;
            else
                fishWeight += caught;
        }

        catchWeightSlider.value = (fishWeight + sharkWeight) / capacity;
        durabilitySlider.value = durability / maxDurability;
    }

    public void CollectFish()
    {
        FishingResources resources = FishingResources.Instance;
        if (resources == null)
            return;

        resources.fishWeight += fishWeight;
        resources.sharkWeight += sharkWeight;

        fishWeight = 0f;
        sharkWeight = 0f;
    }

    public void Repair(float amount)
    {
        durability = Mathf.Min(maxDurability, durability + amount);
    }
}
