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

        ProgressionManager progressionManager =
            FindFirstObjectByType<ProgressionManager>();

        if (progressionManager != null)
        {
            progressionManager.NotifyWoodCollected(totalWoodCollected);
        }

        AchievementManager achievements =
            FindFirstObjectByType<AchievementManager>();

        if (achievements != null)
        {
            if (totalWoodCollected >= 1)
            {
                achievements.UnlockAchievement("First Wood");
            }

            if (totalWoodCollected >= 10)
            {
                achievements.UnlockAchievement("Wood Collector");
            }
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

    // called by the hub when a resource from this room is used there
    public bool UseResource(string resource, int amount)
    {
        if (resource == "Wood")
            return SpendWood(amount);

        return false;
    }

    private void UpdateUI()
    {
        if (woodText != null)
        {
            woodText.text = "WOOD: " + wood;
        }
    }
}