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

    [HideInInspector] public LiftNetInstaller sandboxOwner;

    private bool collected, repaired;
    private bool completing;

    void Update()
    {
        durability = Mathf.Max(
            0f, durability - durabilityLossRate * Time.deltaTime
        );

        if (durability <= 0f && sandboxOwner == null)
        {
            Destroy(gameObject);
            return;
        }

        if (durability > 0f)
            fishWeight = Mathf.Min(
                capacity, fishWeight + catchRate * Time.deltaTime
            );

        catchWeightSlider.value = fishWeight / capacity;
        durabilitySlider.value = durability / maxDurability;
    }

    public void CollectFish()
    {
        if (completing) return;
        int amount = Mathf.FloorToInt(fishWeight);
        if (amount < 1) return;

        fishWeight -= amount;

        if (sandboxOwner != null)
        {
            collected = true;
            sandboxOwner.sandboxFeedback.Show("Fish collected!");
            CheckSandbox();
        }
        else
            FishingResources.Instance.fishWeight += amount;
    }

    public void Repair(float amount)
    {
        if (completing) return;
        bool wasDamaged = durability < maxDurability;
        durability = Mathf.Clamp(durability + amount, 0f, maxDurability);

        if (sandboxOwner != null && wasDamaged &&
            durability == maxDurability)
        {
            repaired = true;
            sandboxOwner.sandboxFeedback.Show("Net fully repaired!");
            CheckSandbox();
        }
    }

    private void CheckSandbox()
    {
        if (collected && repaired && !completing)
        {
            completing = true;
            Invoke(nameof(FinishSandbox), 2f);
        }
    }

    private void FinishSandbox()
    {
        sandboxOwner.CompleteSandbox();
    }
}
