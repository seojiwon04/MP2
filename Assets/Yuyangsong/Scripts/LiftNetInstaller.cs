using UnityEngine;

public class LiftNetInstaller : MonoBehaviour
{
    public GameObject liftNetPrefab;
    public Transform spawnPoint;
    public int woodCost = 5;
    public int ironCost = 5;

    private GameObject currentNet;

    public void InstallNet()
    {
        if (currentNet != null)
            return;

        FishingResources resources = FishingResources.Instance;
        if (resources == null ||
            resources.wood < woodCost ||
            resources.iron < ironCost)
            return;

        resources.wood -= woodCost;
        resources.iron -= ironCost;

        currentNet = Instantiate(
            liftNetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}
