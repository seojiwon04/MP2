using UnityEngine;

public class ShopButton : MonoBehaviour
{
    public enum ItemType
    {
        Seed,
        Sprinkler
    }

    public ItemType itemType;

    public int woodCost = 1;

    public GameObject seedPrefab;
    public GameObject sprinklerPrefab;

    public Transform spawnPoint;

    private PlayerResources resources;

    void Start()
    {
        resources = FindFirstObjectByType<PlayerResources>();
    }

    public void BuyItem()
    {
        if (resources == null)
            return;

        if (!resources.SpendWood(woodCost))
        {
            Debug.Log("Not enough wood!");
            return;
        }

        GameObject prefabToSpawn = null;

        if (itemType == ItemType.Seed)
        {
            prefabToSpawn = seedPrefab;
        }
        else if (itemType == ItemType.Sprinkler)
        {
            prefabToSpawn = sprinklerPrefab;
        }

        if (prefabToSpawn != null)
        {
            Instantiate(
                prefabToSpawn,
                spawnPoint.position,
                spawnPoint.rotation
            );
        }
    }
}