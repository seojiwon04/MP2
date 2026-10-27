using UnityEngine;

public class DockBuildingCell : MonoBehaviour
{
    public GameObject dockPrefab;
    public int woodCost = 5;
    public float yOffset = -2.43f;

    public FishingInteractionFeedback feedback;

    private bool occupied;

    public void BuildDock()
    {
        if (occupied)
            return;

        FishingResources resources = FishingResources.Instance;
        
        if (resources == null) return;

        if (resources.wood < woodCost)
        {
            feedback.Show(
                $"Not enough Wood\nNeed: {woodCost} / Have: {resources.wood}"
            );
            return;
        }

        resources.wood -= woodCost;

        Vector3 position = transform.position + Vector3.up * yOffset;
        GameObject dock = Instantiate(dockPrefab, position, transform.rotation);

        occupied = true;

        FishingProgression.Instance.OnDockBuilt(
            dock.GetComponentInChildren<LiftNetInstaller>()
        );
    }
}
