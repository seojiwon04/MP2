using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;

    public int width = 5;
    public int height = 5;
    public float spacing = 1.5f;

    private GridCell[,] cells;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        cells = new GridCell[width, height];

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
                    transform
                );

                // use grid manager's position as the parent position for the cell
                cellObject.transform.localPosition = position;
                cellObject.transform.localRotation = Quaternion.identity; 

                cellObject.name = "Cell_" + x + "_" + z;

                GridCell cell = cellObject.GetComponent<GridCell>();

                cell.x = x;
                cell.z = z;

                cells[x, z] = cell;
            }
        }
    }

    public GridCell GetCell(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height)
            return null;

        return cells[x, z];
    }
}