using UnityEngine;

public class DockBuildingCell : MonoBehaviour
{
    public Transform roomRoot;
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

        if (!resources.SpendMaterials(woodCost, 0))
        {
            feedback.Show(
                $"Not enough Wood\nNeed: {woodCost} / Have: {resources.GetWood()}"
            );
            return;
        }

        Vector3 position = transform.position
            + roomRoot.TransformVector(Vector3.up * yOffset);

        GameObject dock = Instantiate(
            dockPrefab, position, transform.rotation, roomRoot
        );

        dock.transform.localScale = dockPrefab.transform.localScale;

        LiftNetInstaller installer = dock.GetComponentInChildren<LiftNetInstaller>();
        installer.roomRoot = roomRoot;

        occupied = true;

        FishingProgression.Instance.OnDockBuilt(
            dock.GetComponentInChildren<LiftNetInstaller>()
        );
    }
}
