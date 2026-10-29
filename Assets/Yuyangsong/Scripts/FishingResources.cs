using UnityEngine;

public class FishingResources : MonoBehaviour
{   
    public static FishingResources Instance { get; private set; }

    [Header("Fish")]
    public int fishWeight;
    public int fishMeatWeight;

    [Header("Other Resources")]
    public int coins;

    [Header("Shared Resource Connections")]
    [SerializeField] private HubInventory hubInventory;
    [SerializeField] private PlayerResources forestResources;

    public int totalFish;
    public int totalFishMeat;
    public int totalCoins;

    void Awake()
    {
        Instance = this;
    }

    public void AddFish(FishStats stats)
    {
        fishWeight += stats.Weight;
        totalFish += stats.Weight;
    }

    public int GetWood()
    {
        return hubInventory.GetResource("Wood");
    }

    public int GetIron()
    {
        return hubInventory.GetResource("Iron");
    }

    public bool SpendMaterials(int woodCost, int ironCost)
    {
        return hubInventory.UseResources(woodCost, 0, 0, ironCost);
    }

    public bool AddWood(int amount)
    {
        if (amount <= 0)
            return false;

        // Purchased Wood does not count toward forest harvesting progress.
        forestResources.wood += amount;
        forestResources.SpendWood(0);
        return true;
    }

    public bool AddIron(int amount)
    {
        if (amount <= 0)
            return false;

        hubInventory.oreRoom.Add("RefinedOre", amount);
        return true;
    }

    // called by the hub when a resource from this room is used there
    public bool UseResource(string resource, int amount)
    {
        if (resource != "FishMeat" || fishMeatWeight < amount)
            return false;

        fishMeatWeight -= amount;
        return true;
    }
}
