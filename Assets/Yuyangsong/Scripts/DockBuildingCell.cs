using UnityEngine;

public class DockBuildingCell : MonoBehaviour
{
    public GameObject dockPrefab;
    public int woodCost = 5;
    public float yOffset = -2.43f;

    private bool occupied;

    public void BuildDock()
    {
        if (occupied)
            return;

        FishingResources resources = FishingResources.Instance;
        if (resources == null || resources.wood < woodCost)
            return;

        resources.wood -= woodCost;

        Vector3 position = transform.position + Vector3.up * yOffset;
        Instantiate(dockPrefab, position, transform.rotation);

        occupied = true;
    }
}
