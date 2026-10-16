using UnityEngine;
using UnityEngine.UI;

public class TreeGrowth : MonoBehaviour
{

    public float chopHealth = 3f;
    public GameObject logPrefab;
    public int logCount = 3;

    public GridCell currentCell;
    public PlayerResources playerResources;
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
        playerResources = FindFirstObjectByType<PlayerResources>();
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

    

    public void TakeChopDamage(float damage)
    {
        chopHealth -= damage;

        if (chopHealth <= 0f)
        {
            HarvestTree();
        }
    }

    private void HarvestTree()
    {
        int logCount;

        if (growth < 25f)
        {
            logCount = 1;
        }
        else if (growth < 50f)
        {
            logCount = 2;
        }
        else if (growth < 75f)
        {
            logCount = 3;
        }
        else
        {
            logCount = 4;
        }

        for (int i = 0; i < logCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                0.5f,
                Random.Range(-0.3f, 0.3f)
            );

            Instantiate(
                logPrefab,
                transform.position + offset,
                Random.rotation
            );
        }

        if (currentCell != null)
        {
            currentCell.currentObject = null;
            currentCell.Untill();
        }

        Destroy(gameObject);
    }
    private void UpdateUI()
    {
        if (growthBar != null)
            growthBar.value = growth;

        if (moistureBar != null)
            moistureBar.value = moisture;
    }
}