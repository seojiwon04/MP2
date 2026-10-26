using UnityEngine;
using UnityEngine.UI;

public class TridentStats : MonoBehaviour
{
    [Range(0f, 1f)]
    public float sharpness = 1f;

    public float maxSharpness = 1f;

    public float sharpnessLoss = 0.1f;

    public Slider sharpnessSlider;

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

        if (sharpness <= 0f)
        {
            FishingProgression.Instance.OnTridentDepleted();
        }
    }

    public void Sharpen(float amount)
    {
        sharpness = Mathf.Min(maxSharpness, sharpness + amount);
    }

    public void Update()
    {
        sharpnessSlider.value = sharpness;
    }
}
