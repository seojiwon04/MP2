using UnityEngine;

public class Seed : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GridCell cell = collision.gameObject.GetComponent<GridCell>();

        if (cell != null)
        {
            cell.TryPlant(gameObject);
        }
    }
}