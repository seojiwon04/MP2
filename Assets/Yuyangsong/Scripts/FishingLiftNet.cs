using UnityEngine;

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

    void Update()
    {
        durability -= durabilityLossRate * Time.deltaTime;

        if (durability <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float remaining = capacity - fishWeight - sharkWeight;
        if (remaining <= 0f)
            return;

        float caught = Mathf.Min(catchRate * Time.deltaTime, remaining);

        if (Random.value < sharkChance)
            sharkWeight += caught;
        else
            fishWeight += caught;
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
