using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;

    public int width = 5;
    public int height = 5;
    public float spacing = 1.5f;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        float xOffset = (width - 1) * spacing / 2f;
        float zOffset = (height - 1) * spacing / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(
                    x * spacing - xOffset,
                    0f,
                    z * spacing - zOffset
                );

                GameObject cellObject = Instantiate(
                    cellPrefab,
                    position,
                    Quaternion.identity,
                    transform
                );

                cellObject.name = "Cell_" + x + "_" + z;

                GridCell cell = cellObject.GetComponent<GridCell>();
                cell.x = x;
                cell.z = z;
            }
        }
    }
}