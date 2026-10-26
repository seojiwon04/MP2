using UnityEngine;

public class FishMeatCollector : MonoBehaviour
{
    public int weight;

    public void Collect()
    {
        FishingResources resources = FishingResources.Instance;

        if (resources == null)
            return;

        resources.fishMeatWeight += weight;
        
        FishingProgression.Instance.OnMeatCollected(weight);
        
        Destroy(gameObject);
    }
}
