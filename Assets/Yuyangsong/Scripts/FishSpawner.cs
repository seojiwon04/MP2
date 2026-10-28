using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject[] fishPrefabs;
    public int initialCount = 10;
    public int maxCount = 20;
    public float spawnRate = 0.1f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < initialCount; i++)
            SpawnFish();
    }

    void SpawnFish()
    {
        GameObject fishPrefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
        GameObject fish = Instantiate(fishPrefab, transform);

        FishPath fishPath = fish.GetComponent<FishPath>();
        Vector3[] path = fishPath.SamplePath();

        for (int i = 0; i < path.Length; i++) // change path points from local to world space (for hub integration)
        {
            path[i] = transform.TransformPoint(path[i]);
        }

        int startIndex = Random.Range(0, path.Length);
        fish.transform.position = path[startIndex];
        fishPath.SetPath(path, startIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount >= maxCount)
            return;

        if (Random.value < spawnRate * Time.deltaTime)
            SpawnFish();
    }
}
