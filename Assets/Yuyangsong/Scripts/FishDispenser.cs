using UnityEngine;

public class FishDispenser : MonoBehaviour
{
    public GameObject[] fishPrefabs;
    public Transform spawnPoint;

    public GameObject currentMeat;

    public FishingInteractionFeedback feedback;

    private GameObject currentFish;

    public void TakeFish(int weight)
    {
        if (currentFish != null || currentMeat != null)
            return;
        
        FishingResources resources = FishingResources.Instance;

        if (resources.fishWeight < weight)
        {
            feedback.Show(
                $"Not enough Fish\nNeed: {weight} kg / Have: {resources.fishWeight} kg"
            );
            return;
        }

        resources.fishWeight -= weight;

        GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];

        currentFish = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        FishStats stats = currentFish.GetComponent<FishStats>();

        stats.canGrow = false;
        stats.growth = 1f;
        stats.baseWeight = weight / 10f;

        currentFish.transform.localScale = prefab.transform.localScale * 0.5f;

        FishPath path = currentFish.GetComponent<FishPath>();
        if (path != null)
            path.enabled = false;
    }
}
