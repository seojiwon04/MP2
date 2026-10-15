using UnityEngine;
using System.Collections;

public class Sprinkler : MonoBehaviour
{
    public GridCell currentCell;

    public float waterAmount = 0.1f;
    public float waterInterval = 2f;

    private GridManager gridManager;

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        GridCell cell = collision.gameObject.GetComponent<GridCell>();

        if (cell != null)
        {
            cell.TryPlaceSprinkler(gameObject);
        }
    }

    public void AttachToCell(GridCell cell)
    {
        currentCell = cell;

        transform.position = cell.transform.position;
        transform.rotation = Quaternion.identity;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        StartCoroutine(WaterNeighbors());
    }

    private IEnumerator WaterNeighbors()
    {
        while (true)
        {
            yield return new WaitForSeconds(waterInterval);

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int zOffset = -1; zOffset <= 1; zOffset++)
                {
                    if (xOffset == 0 && zOffset == 0)
                        continue;

                    GridCell neighbor = gridManager.GetCell(
                        currentCell.x + xOffset,
                        currentCell.z + zOffset
                    );

                    if (neighbor == null)
                        continue;

                    if (neighbor.currentObject == null)
                        continue;

                    TreeGrowth tree =
                        neighbor.currentObject.GetComponent<TreeGrowth>();

                    if (tree != null)
                    {
                        tree.AddMoisture(waterAmount);
                    }
                }
            }
        }
    }
}