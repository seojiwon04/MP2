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
        ErrorPopupManager errorPopup =
        FindFirstObjectByType<ErrorPopupManager>();

        if (!isTilled)
        {
            if (errorPopup != null)
            {
                errorPopup.ShowError(
                    "Till the soil before planting."
                );
            }

            return;
        }

        if (!IsEmpty())
        {
            if (errorPopup != null)
            {
                errorPopup.ShowError(
                    "This plot is already occupied."
                );
            }

            return;
        }

        Vector3 treePosition = transform.position;

        GameObject tree = Instantiate(
            treePrefab,
            treePosition,
            Quaternion.identity
        );

        currentObject = tree;

        TreeGrowth treeGrowth = tree.GetComponent<TreeGrowth>();

        if (treeGrowth != null)
        {
            treeGrowth.currentCell = this;
        }

        ProgressionManager progression =
    FindFirstObjectByType<ProgressionManager>();

        if (progression != null)
        {
            progression.NotifyTreePlanted();
        }

        AchievementManager achievements =
    FindFirstObjectByType<AchievementManager>();

        if (achievements != null)
        {
            achievements.UnlockAchievement("First Tree Planted");
        }


        Destroy(seedObject);
    }

    public void TryPlaceSprinkler(GameObject sprinklerObject)
    {
        ErrorPopupManager errorPopup =
        FindFirstObjectByType<ErrorPopupManager>();

        if (isTilled)
        {
            if (errorPopup != null)
            {
                errorPopup.ShowError(
                    "Sprinklers require untilled soil."
                );
            }

            return;
        }

        if (!IsEmpty())
        {
            if (errorPopup != null)
            {
                errorPopup.ShowError(
                    "This plot is already occupied."
                );
            }

            return;
        }

        Sprinkler sprinkler = sprinklerObject.GetComponent<Sprinkler>();

        if (sprinkler == null)
            return;

        currentObject = sprinklerObject;

        sprinkler.AttachToCell(this);

        ProgressionManager progression =
        FindFirstObjectByType<ProgressionManager>();

        if (progression != null)
        {
            progression.NotifySprinklerPlaced();
        }
    }
    private void UpdateAppearance()
    {
        if (isTilled)
            cellRenderer.material = tilledMaterial;
        else
            cellRenderer.material = untilledMaterial;
    }
}