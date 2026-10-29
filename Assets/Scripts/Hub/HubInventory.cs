using TMPro;
using UnityEngine;

public class HubInventory : MonoBehaviour
{
    public TMP_Text inventoryText;
    public bool startWithDebugResources = false; // turn off before submitting
    public int debugAmount = 100;

    PlayerResources masonResources; // bc Mason already has a UseResource func

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
        return 0;
    }

    // spends all three at once, or nothing if any of them isn't enough
    public bool UseResources(int wood, int wool, int fishMeat)
    {
        if (GetResource("Wood") < wood || GetResource("Wool") < wool || GetResource("FishMeat") < fishMeat)
        {
            return false;
        }

        UseResource("Wood", wood);
        UseResource("Wool", wool);
        UseResource("FishMeat", fishMeat);
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

        inventoryText.text = "Inventory\n\n"
            + "Wood: " + wood
            + "\nWool: " + wool
            + "\nFish Meat: " + fishMeat;
    }
}
