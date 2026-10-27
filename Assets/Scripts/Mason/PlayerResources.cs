using UnityEngine;
using TMPro;

public class PlayerResources : MonoBehaviour
{

    public int totalWoodCollected = 0;

    private ProgressionManager progressionManager;
    public int wood = 0;

    public TMP_Text woodText;

    void Start()
    {
        progressionManager = FindFirstObjectByType<ProgressionManager>();

        UpdateUI();
    }

    public void AddWood(int amount)
    {
        wood += amount;
        totalWoodCollected += amount;

        UpdateUI();

        if (progressionManager != null)
        {
            progressionManager.NotifyWoodCollected(totalWoodCollected);
        }
    }

    public bool SpendWood(int amount)
    {
        if (wood < amount)
            return false;

        wood -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (woodText != null)
        {
            woodText.text = "WOOD: " + wood;
        }
    }
}