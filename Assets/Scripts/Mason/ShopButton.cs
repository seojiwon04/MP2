using UnityEngine;

public class ShopButton : MonoBehaviour
{
    public enum ItemType
    {
        Seed,
        Sprinkler
    }

private ErrorPopupManager errorPopup;
    public ItemType itemType;

    public int woodCost = 1;

    public GameObject seedPrefab;
    public GameObject sprinklerPrefab;

    public Transform spawnPoint;

    private PlayerResources resources;

    void Start()
    {
        errorPopup = FindFirstObjectByType<ErrorPopupManager>();
        resources = FindFirstObjectByType<PlayerResources>();
    }

    public void BuyItem()
    {
        if (resources == null)
            return;

        if (!resources.SpendWood(woodCost))
{
    if (errorPopup != null)
        {
            errorPopup.ShowError(
                "You need more Wood to buy this item."
            );
        }

        return;
    }

        GameObject prefabToSpawn = null;

        if (itemType == ItemType.Seed)
        {
            prefabToSpawn = seedPrefab;
        }
        else if (itemType == ItemType.Sprinkler)
        {
            prefabToSpawn = sprinklerPrefab;
        }

        if (prefabToSpawn != null)
        {
            Instantiate(
                prefabToSpawn,
                spawnPoint.position,
                spawnPoint.rotation
            );

            AchievementManager achievements =
                FindFirstObjectByType<AchievementManager>();

            if (achievements != null)
            {
                if (itemType == ItemType.Seed)
                {
                    achievements.UnlockAchievement("First Seed");
                }
                else if (itemType == ItemType.Sprinkler)
                {
                    achievements.UnlockAchievement("First Sprinkler");
                }
            }
        }
    }
}