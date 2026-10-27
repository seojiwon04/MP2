using UnityEngine;

public class AxeTool : MonoBehaviour
{
    public float damage = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        TreeGrowth tree = collision.gameObject.GetComponentInParent<TreeGrowth>();

        if (tree != null)
        {
            tree.TakeChopDamage(damage);
        }
    }
}