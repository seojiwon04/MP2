using UnityEngine;

public class FishMeatCollector : MonoBehaviour
{
    public float weight;

    public void Collect()
    {
        FishingResources resources = FishingResources.Instance;

        if (resources == null)
            return;

        resources.fishMeatWeight += weight;
        
        Destroy(gameObject);
    }
}
