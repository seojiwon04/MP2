using UnityEngine;

public class FishCutter : MonoBehaviour
{
    public Transform roomRoot;

    public GameObject fishMeatPrefab;
    public Transform spawnPoint;
    public FishDispenser dispenser;

    public void CutFish(FishStats fish)
    {
        GameObject meat = Instantiate(
            fishMeatPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            roomRoot
        );

        meat.transform.localScale = fishMeatPrefab.transform.localScale;

        dispenser.currentMeat = meat;

        FishMeatCollector collection = meat.GetComponent<FishMeatCollector>();
        collection.weight = fish.Weight;

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
