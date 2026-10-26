using UnityEngine;

public class FishingResources : MonoBehaviour
{   
    public static FishingResources Instance { get; private set; }

    [Header("Fish")]
    public float fishWeight;
    public float fishMeatWeight;

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
        fishWeight += stats.Weight;
    }
}
