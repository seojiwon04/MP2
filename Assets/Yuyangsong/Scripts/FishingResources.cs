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
}
