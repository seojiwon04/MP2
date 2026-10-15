using UnityEngine;

public class LogPickup : MonoBehaviour
{
    private bool collected = false;

    public void Collect()
    {
        if (collected)
            return;

        collected = true;

        PlayerResources resources =
            FindFirstObjectByType<PlayerResources>();

        if (resources != null)
        {
            resources.AddWood(1);
        }

        Destroy(gameObject);
    }
}