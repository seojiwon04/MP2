using UnityEngine;

public class FishingResources : MonoBehaviour
{   
    public static FishingResources Instance { get; private set; }

    [Header("Fish")]
    public float fishWeight;
    public float sharkWeight;
    public float fishMeatWeight;
    public float sharkMeatWeight;

    [Header("Other Resources")]
    public int coins;
    public int wood;
    public int iron;

    void Awake()
    {
        Instance = this;
    }

    public void AddFish(FishStats stats)
    {
        if (stats.fishType == FishStats.FishType.Shark)
            sharkWeight += stats.Weight;
        else
            fishWeight += stats.Weight;
    }
}
