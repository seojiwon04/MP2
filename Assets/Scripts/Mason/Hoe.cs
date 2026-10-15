using UnityEngine;

public class HoeTool : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GridCell cell = collision.gameObject.GetComponent<GridCell>();

        if (cell != null && !cell.isTilled)
        {
            cell.Till();
        }
    }
}
