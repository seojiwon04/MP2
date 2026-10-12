using UnityEngine;

public class Bucket : MonoBehaviour
{
    public float moistureAmount = 0.25f;

    private void OnTriggerEnter(Collider other)
    {
        TreeGrowth tree = other.GetComponentInParent<TreeGrowth>();

        if (tree != null)
        {
            tree.AddMoisture(moistureAmount);
        }
    }
}
