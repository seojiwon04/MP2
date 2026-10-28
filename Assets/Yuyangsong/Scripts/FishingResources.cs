using UnityEngine;

public class FishingResources : MonoBehaviour
{   
    public static FishingResources Instance { get; private set; }

    [Header("Fish")]
    public int fishWeight;
    public int fishMeatWeight;

    [Header("Other Resources")]
    public int coins;
    public int wood;
    public int iron;

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

    // called by the hub when a resource from this room is used there
    public bool UseResource(string resource, int amount)
    {
        if (resource != "FishMeat" || fishMeatWeight < amount)
            return false;

        fishMeatWeight -= amount;
        return true;
    }
}
