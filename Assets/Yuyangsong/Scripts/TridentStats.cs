using UnityEngine;

public class TridentStats : MonoBehaviour
{
    [Range(0f, 1f)]
    public float sharpness = 1f;

    public float maxSharpness = 1f;

    public float sharpnessLoss = 0.1f;

    void OnTriggerEnter(Collider other)
    {
        FishStats fish = other.GetComponent<FishStats>();

        if (fish == null)
            return;

        if (Random.value < sharpness)
        {
            FishingResources.Instance.AddFish(fish);
            Destroy(fish.gameObject);
        }

        sharpness = Mathf.Max(0f, sharpness - sharpnessLoss);
    }

    public void Sharpen(float amount)
    {
        sharpness = Mathf.Min(maxSharpness, sharpness + amount);
    }
}
