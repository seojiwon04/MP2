using UnityEngine;
using System.Collections;

public class TreeSpawnManager : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject treePrefab;

    public float respawnDelay = 15f;

    private bool waitingToRespawn = false;

    IEnumerator Start()
    {
        // Give GridManager one frame to generate the grid
        yield return null;

        SpawnRandomTree();

        StartCoroutine(CheckForTrees());
    }

    private IEnumerator CheckForTrees()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            TreeGrowth[] trees =
                FindObjectsByType<TreeGrowth>(FindObjectsSortMode.None);

            if (trees.Length == 0 && !waitingToRespawn)
            {
                StartCoroutine(RespawnTree());
            }
        }
    }

    private IEnumerator RespawnTree()
    {
        waitingToRespawn = true;

        yield return new WaitForSeconds(respawnDelay);

        TreeGrowth[] trees =
            FindObjectsByType<TreeGrowth>(FindObjectsSortMode.None);

        // Only spawn if there are STILL no trees
        if (trees.Length == 0)
        {
            SpawnRandomTree();
        }

        waitingToRespawn = false;
    }

    private void SpawnRandomTree()
    {
        // Try random cells until we find an empty one
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = Random.Range(0, gridManager.width);
            int z = Random.Range(0, gridManager.height);

            GridCell cell = gridManager.GetCell(x, z);

            if (cell == null || !cell.IsEmpty())
                continue;

            // Wild tree occupies this soil
            cell.Till();

            Vector3 spawnPosition =
                cell.transform.position;

            GameObject tree = Instantiate(
                treePrefab,
                spawnPosition,
                Quaternion.identity
            );

            cell.currentObject = tree;

            TreeGrowth treeGrowth =
                tree.GetComponent<TreeGrowth>();

            if (treeGrowth != null)
            {
                treeGrowth.currentCell = cell;
            }

            return;
        }

    }
}