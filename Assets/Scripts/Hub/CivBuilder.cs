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
    public FirstCivFeeder firstFeeder;
    public SheepFarm.ErrorPopup errorPopup;
    public Material aliveMaterial;
    public GameObject civPrefab;

    public Renderer[] platforms;
    public GameObject[] civs;
    public Road[] roads;

    public int roadWood = 2;
    public int roadWool = 2;
    public int roadFishMeat = 2;

    public int civWood = 6;
    public int civWool = 6;
    public int civFishMeat = 6;
    public int civIron = 6;

    public TMP_Text instructionText;
    bool firstRoadBuilt = false;
    bool firstCivBuilt = false;
    bool won = false;
    public GameObject OreRoom;

    private void Start()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<HubInventory>();
        }
        if (feeder == null)
        {
            feeder = GetComponent<CivFeeder>();
        }
        if (firstFeeder == null)
        {
            firstFeeder = GetComponent<FirstCivFeeder>();
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
        if (!inventory.UseResources(roadWood, roadWool, roadFishMeat, 0))
        {
            ShowError(road.road, "Not enough resources!");
            return;
        }

        road.road.sharedMaterial = aliveMaterial;
        if (!firstRoadBuilt)
        {
            firstRoadBuilt = true;
            instructionText.text = "Great! Now, try to build a civilization! Collect some iron and gather enough resources for a civ. Click on an empty platform stemming from a road to build a civ.";
            OreRoom.SetActive(true);
        }
    }

    private void TryBuildCiv(Renderer platform)
    {
        if (IsBuilt(platform))
        {
            if (won)
            {
                return;
            }
            if (firstFeeder != null && firstFeeder.TryFeed(platform))
            {
                return;
            }
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

        if (!firstCivBuilt)
        {
            firstCivBuilt = true;
            instructionText.text = "Great! You built your first civilization! Build all 4 civilizations to complete the challenge. Remember, civs need to be kept alive!";
        }

        if (AllCivsBuilt())
        {
            Win();
        }
    }

    private bool AllCivsBuilt()
    {
        foreach (Renderer platform in platforms)
        {
            if (!IsBuilt(platform))
            {
                return false;
            }
        }
        return true;
    }

    private void Win()
    {
        won = true;
        instructionText.text = "You win!";

        if (feeder != null)
        {
            feeder.enabled = false;
            feeder.hungerText.gameObject.SetActive(false);
        }
        if (firstFeeder != null)
        {
            firstFeeder.enabled = false;
            firstFeeder.hungerText.gameObject.SetActive(false);
        }
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