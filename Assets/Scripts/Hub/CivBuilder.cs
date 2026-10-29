using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CivBuilder : MonoBehaviour
{
    [System.Serializable]
    public class Road
    {
        public Renderer road;
        public Renderer civA; 
        public Renderer civB;
    }

    public HubInventory inventory;
    public CivFeeder feeder;
    public SheepFarm.ErrorPopup errorPopup;
    public Material aliveMaterial;
    public GameObject civPrefab;
    public TMP_Text costText;

    public Renderer[] platforms;
    public GameObject[] civs;
    public Road[] roads;

    [Header("Road Cost")]
    public int roadWood = 2;
    public int roadWool = 2;
    public int roadFishMeat = 2;
    public int roadIron = 2;

    [Header("Civ Cost")]
    public int civWood = 8;
    public int civWool = 8;
    public int civFishMeat = 8;
    public int civIron = 8;

    private void Start()
    {
        if (costText != null)
        {
            costText.text = "Cost:\n\n1 Road:\n" + roadWood + " Wood + " + roadWool + " Wool\n"
                + roadFishMeat + " Fish Meat + " + roadIron + " IRON\n\n1 Civ:\n"
                + civWood + " Wood + " + civWool + " Wool\n"
                + civFishMeat + " Fish Meat + " + civIron + " IRON";
        }
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<HubInventory>();
        }
        if (feeder == null)
        {
            feeder = GetComponent<CivFeeder>();
        }

        foreach (Renderer platform in platforms)
        {
            Renderer p = platform;
            GetClickable(p).selectEntered.AddListener(args => TryBuildCiv(p));
        }
        foreach (Road road in roads)
        {
            Road r = road;
            GetClickable(r.road).selectEntered.AddListener(args => TryBuildRoad(r));
        }
    }

    private XRSimpleInteractable GetClickable(Renderer target)
    {
        XRSimpleInteractable clickable = target.GetComponent<XRSimpleInteractable>();
        if (clickable == null)
        {
            clickable = target.gameObject.AddComponent<XRSimpleInteractable>();
        }
        return clickable;
    }

    private bool IsBuilt(Renderer target)
    {
        return target.sharedMaterial == aliveMaterial;
    }

    private void TryBuildRoad(Road road)
    {
        if (IsBuilt(road.road))
        {
            ShowError(road.road, "Plot already taken!");
            return;
        }
        if (!IsBuilt(road.civA) && !IsBuilt(road.civB))
        {
            ShowError(road.road, "Road not connected to a civ!");
            return;
        }
        if (!inventory.UseResources(roadWood, roadWool, roadFishMeat, roadIron))
        {
            ShowError(road.road, "Not enough resources!");
            return;
        }

        road.road.sharedMaterial = aliveMaterial;
    }

    private void TryBuildCiv(Renderer platform)
    {
        if (IsBuilt(platform))
        {
            if (feeder != null && feeder.TryFeed(platform))
            {
                return;
            }
            ShowError(platform, "Plot already taken!");
            return;
        }
        if (!HasBuiltRoad(platform))
        {
            ShowError(platform, "No road connected!");
            return;
        }
        if (!inventory.UseResources(civWood, civWool, civFishMeat, civIron))
        {
            ShowError(platform, "Not enough resources!");
            return;
        }

        platform.sharedMaterial = aliveMaterial;

        // place the civ on top of the platform
        Bounds top = platform.bounds;
        Vector3 position = new Vector3(top.center.x, top.max.y, top.center.z);
        int index = System.Array.IndexOf(platforms, platform);
        civs[index] = Instantiate(civPrefab, position, civPrefab.transform.rotation);
    }

    public void ShowError(Renderer target, string message)
    {
        Bounds top = target.bounds;
        errorPopup.transform.position = new Vector3(top.center.x, top.max.y + 2f, top.center.z);
        errorPopup.Show(message);
    }

    private bool HasBuiltRoad(Renderer platform)
    {
        foreach (Road road in roads)
        {
            if ((road.civA == platform || road.civB == platform) && IsBuilt(road.road))
            {
                return true;
            }
        }
        return false;
    }
}
