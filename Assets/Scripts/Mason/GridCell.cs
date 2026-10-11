using UnityEngine;

public class GridCell : MonoBehaviour
{
    public int x;
    public int z;

    public bool isTilled = false;
    public GameObject currentObject;

    public Material untilledMaterial;
    public Material tilledMaterial;

    public GameObject treePrefab;

    private Renderer cellRenderer;

    void Start()
    {
        cellRenderer = GetComponent<Renderer>();
        UpdateAppearance();
    }

    public bool IsEmpty()
    {
        return currentObject == null;
    }

    public void Till()
    {
        if (!IsEmpty())
            return;

        isTilled = true;
        UpdateAppearance();
    }

    public void Untill()
    {
        isTilled = false;
        UpdateAppearance();
    }

    public void TryPlant(GameObject seedObject)
    {
        if (!isTilled)
            return;

        if (!IsEmpty())
            return;

        Vector3 treePosition = transform.position + Vector3.up * 0.5f;

        GameObject tree = Instantiate(
            treePrefab,
            treePosition,
            Quaternion.identity
        );

        currentObject = tree;

        Destroy(seedObject);
    }

    private void UpdateAppearance()
    {
        if (isTilled)
            cellRenderer.material = tilledMaterial;
        else
            cellRenderer.material = untilledMaterial;
    }
}