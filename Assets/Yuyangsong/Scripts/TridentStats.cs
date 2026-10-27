using UnityEngine;
using UnityEngine.UI;

public class TridentStats : MonoBehaviour
{
    [Range(0f, 1f)]
    public float sharpness = 1f;

    public float maxSharpness = 1f;

    public float sharpnessLoss = 0.1f;

    public Slider sharpnessSlider;

    public FishingInteractionFeedback feedback;

    void OnTriggerEnter(Collider other)
    {
        FishStats fish = other.GetComponent<FishStats>();

        if (fish == null)
            return;

        if (sharpness <= 0f)
        {
            feedback.Show("Trident dull\nUse Grindstone");
            return;
        }

        if (Random.value < sharpness)
        {
            FishingResources.Instance.AddFish(fish);
            feedback.Show($"+{fish.Weight} kg Fish", 1.2f);
            Destroy(fish.gameObject);
        }
        else
        {
            feedback.Show("Miss", 1.2f);
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
