using UnityEngine;
using UnityEngine.UI;

public class TreeGrowth : MonoBehaviour
{
    [Header("Tree Visual")]
    public Transform treeModel;

    [Header("Growth")]
    public float growth = 0f;
    public float maxGrowth = 100f;
    public float baseGrowthRate = 5f;

    [Header("Moisture")]
    public float moisture = 0.5f;
    public float moistureDecayRate = 0.02f;

    [Header("UI")]
    public Slider growthBar;
    public Slider moistureBar;

    private Vector3 startScale;

    void Start()
    {
        startScale = treeModel.localScale;

        // Start as a small sapling
        treeModel.localScale = startScale * 0.3f;

        if (growthBar != null)
        {
            growthBar.minValue = 0f;
            growthBar.maxValue = maxGrowth;
        }

        if (moistureBar != null)
        {
            moistureBar.minValue = 0f;
            moistureBar.maxValue = 1f;
        }

        UpdateUI();
    }

    void Update()
    {
        // Moisture slowly decreases
        moisture -= moistureDecayRate * Time.deltaTime;
        moisture = Mathf.Clamp01(moisture);

        // Tree grows until it reaches max growth
        if (growth < maxGrowth)
        {
            float growthRate = baseGrowthRate * (0.25f + moisture);

            growth += growthRate * Time.deltaTime;
            growth = Mathf.Clamp(growth, 0f, maxGrowth);

            float growthPercent = growth / maxGrowth;

            float scaleMultiplier = Mathf.Lerp(
                0.3f,
                1f,
                growthPercent
            );

            treeModel.localScale = startScale * scaleMultiplier;
        }

        UpdateUI();
    }

    public void AddMoisture(float amount)
    {
        moisture += amount;
        moisture = Mathf.Clamp01(moisture);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (growthBar != null)
            growthBar.value = growth;

        if (moistureBar != null)
            moistureBar.value = moisture;
    }
}