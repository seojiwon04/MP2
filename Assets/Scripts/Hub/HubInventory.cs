using TMPro;
using UnityEngine;
using Rishi.OreRoom;

[DefaultExecutionOrder(-400)]
public class HubInventory : MonoBehaviour
{
    public TMP_Text inventoryText;
    public bool startWithDebugResources = false; // turn off before submitting
    public int debugAmount = 100;

    [Header("Ore room connection")]
    public OreRoom oreRoom;
    public GameObject oreRoomPrefab;
    public Vector3 oreRoomPosition = new Vector3(-40, 0, 0);
    public Vector3 oreRoomEulerAngles = new Vector3(0, -90, 0);

    PlayerResources masonResources; // bc Mason already has a UseResource func

    private void Awake()
    {
        EnsureOreRoom();
    }

    // Also used as a build-time fallback when the editor has not baked the instance yet.
    public void EnsureOreRoom()
    {
        if (!oreRoom)
        {
            foreach (var candidate in FindObjectsByType<OreRoom>())
                if (candidate.gameObject.scene == gameObject.scene) { oreRoom = candidate; break; }
        }
        if (!oreRoom && oreRoomPrefab)
        {
            var instance = Instantiate(oreRoomPrefab, oreRoomPosition, Quaternion.Euler(oreRoomEulerAngles));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
            oreRoom = instance.GetComponent<OreRoom>();
        }
        if (oreRoom && oreRoomPrefab) OreHubConnection.Prepare(oreRoom);
    }

    private void Start()
    {
        if (inventoryText == null)
        {
            inventoryText = GetComponentInChildren<TMP_Text>();
        }

        if (startWithDebugResources)
        {
            AddDebugResources();
        }
    }

    // gives every room debugAmount of its resource
    private void AddDebugResources()
    {
        masonResources = FindAnyObjectByType<PlayerResources>();
        var fish = FishingResources.Instance;
        var farm = SheepFarm.PlayerResources.Instance;

        if (masonResources != null)
        {
            // set directly instead of AddWood so it doesn't unlock Mason's seed shop
            masonResources.wood += debugAmount;
            masonResources.SpendWood(0); // refreshes Mason's wood text
        }
        if (fish != null)
        {
            fish.fishMeatWeight += debugAmount;
        }
        if (farm != null)
        {
            farm.Add("Wool", debugAmount);
        }

        Debug.Log("HubInventory: added " + debugAmount + " Wood, Wool and Fish Meat for debugging");
    }
    public bool UseResource(string resource, int amount)
    {
        if (amount < 0) return false;
        if (amount == 0) return true;
        if (masonResources == null)
        {
            masonResources = FindAnyObjectByType<PlayerResources>();
        }
        var fish = FishingResources.Instance;
        var farm = SheepFarm.PlayerResources.Instance;

        if (resource == "Wood")
        {
            return masonResources != null && masonResources.UseResource("Wood", amount);
        }
        if (resource == "FishMeat")
        {
            return fish != null && fish.UseResource("FishMeat", amount);
        }
        if (resource == "Wool")
        {
            return farm != null && farm.UseResource("Wool", amount);
        }
        if (resource == "Iron")
        {
            return oreRoom != null && oreRoom.Spend("RefinedOre", amount);
        }
        return false;
    }

    public int GetResource(string resource)
    {
        if (masonResources == null)
        {
            masonResources = FindAnyObjectByType<PlayerResources>();
        }
        var fish = FishingResources.Instance;
        var farm = SheepFarm.PlayerResources.Instance;

        if (resource == "Wood")
        {
            return masonResources != null ? masonResources.wood : 0;
        }
        if (resource == "FishMeat")
        {
            return fish != null ? fish.fishMeatWeight : 0;
        }
        if (resource == "Wool")
        {
            return farm != null ? Mathf.FloorToInt(farm.Get("Wool")) : 0;
        }
        if (resource == "Iron")
        {
            return oreRoom != null ? oreRoom.Count("RefinedOre") : 0;
        }
        return 0;
    }

    // spends all three at once, or nothing if any of them isn't enough
    public bool UseResources(int wood, int wool, int fishMeat)
    {
        return UseResources(wood, wool, fishMeat, 0);
    }

    // Check every cost before deducting anything; a shortage cannot consume other materials.
    public bool UseResources(int wood, int wool, int fishMeat, int iron)
    {
        if (wood < 0 || wool < 0 || fishMeat < 0 || iron < 0) return false;
        if (GetResource("Wood") < wood || GetResource("Wool") < wool || GetResource("FishMeat") < fishMeat || GetResource("Iron") < iron)
        {
            return false;
        }

        UseResource("Wood", wood);
        UseResource("Wool", wool);
        UseResource("FishMeat", fishMeat);
        UseResource("Iron", iron);
        return true;
    }

    private void Update()
    {
        if (masonResources == null) masonResources = FindAnyObjectByType<PlayerResources>();
        var fish = FishingResources.Instance;
        var farm = SheepFarm.PlayerResources.Instance;

        int wood = masonResources != null ? masonResources.wood : 0;
        int fishMeat = fish != null ? fish.fishMeatWeight : 0;
        float wool = farm != null ? farm.Get("Wool") : 0;

        if (inventoryText == null) return;
        inventoryText.text = "Inventory\n\n"
            + "Wood: " + wood
            + "\nWool: " + wool
            + "\nFish Meat: " + fishMeat
            + "\nIRON: " + GetResource("Iron");
    }
}
