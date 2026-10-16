using UnityEngine;

public class FishCutter : MonoBehaviour
{
    public GameObject fishMeatPrefab;
    public GameObject sharkMeatPrefab;
    public Transform spawnPoint;
    public FishDispenser dispenser;
    public float meatRatio = 0.8f;

    public void CutFish(FishStats fish)
    {
        GameObject prefab = fish.fishType == FishStats.FishType.Fish
            ? fishMeatPrefab
            : sharkMeatPrefab;

        GameObject meat = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        dispenser.currentMeat = meat;

        FishMeatCollector collection = meat.GetComponent<FishMeatCollector>();
        collection.type = fish.fishType;
        collection.weight = fish.Weight * meatRatio;

        Destroy(fish.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        FishStats fish = other.GetComponent<FishStats>();

        if (fish == null || fish.canGrow)
            return;

        CutFish(fish);
    }
}
