using UnityEngine;

public class FishDispenser : MonoBehaviour
{
    public GameObject[] fishPrefabs;
    public GameObject sharkPrefab;
    public Transform spawnPoint;

    public GameObject currentMeat;
    private GameObject currentFish;

    public void TakeFish(FishStats.FishType type, float weight)
    {
        if (currentFish != null || currentMeat != null)
            return;
        
        FishingResources resources = FishingResources.Instance;

        if (type == FishStats.FishType.Fish)
        {
            if (resources.fishWeight < weight)
                return;

            resources.fishWeight -= weight;
        }
        else
        {
            if (resources.sharkWeight < weight)
                return;

            resources.sharkWeight -= weight;
        }

        GameObject prefab = type == FishStats.FishType.Fish
            ? fishPrefabs[Random.Range(0, fishPrefabs.Length)]
            : sharkPrefab;

        currentFish = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
        GameObject fish = currentFish;

        FishStats stats = fish.GetComponent<FishStats>();
        stats.canGrow = false;
        stats.growth = 1f;
        stats.baseWeight = weight / 10f;

        fish.transform.localScale = prefab.transform.localScale * 0.5f;

        FishPath path = fish.GetComponent<FishPath>();
        if (path != null)
            path.enabled = false;
    }
}
