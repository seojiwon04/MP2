using UnityEngine;

public class FishMeatCollector : MonoBehaviour
{
    public FishStats.FishType type;
    public float weight;

    public void Collect()
    {
        FishingResources resources = FishingResources.Instance;

        if (resources == null)
            return;

        if (type == FishStats.FishType.Fish)
            resources.fishMeatWeight += weight;
        else
            resources.sharkMeatWeight += weight;
        
        Destroy(gameObject);
    }
}
