using UnityEngine;
namespace Rishi.OreRoom {
// Reads and spends the team's real accounts. No resource copies are made on travel.
public class OreSharedInventory {
    readonly OreRoom ore;
    HubInventory hub;
    public OreSharedInventory(OreRoom room) { ore=room; }
    public HubInventory Hub {
        get {
            if(hub)return hub;
            foreach(var candidate in Object.FindObjectsByType<HubInventory>())
                if(candidate.gameObject.scene==ore.gameObject.scene) { hub=candidate;break; }
            return hub;
        }
    }
    public global::PlayerResources Forest { get { return Object.FindAnyObjectByType<global::PlayerResources>(); } }
    public FishingResources Fishing { get { return FishingResources.Instance; } }
    public SheepFarm.PlayerResources Farm { get { return SheepFarm.PlayerResources.Instance; } }
    public int Wood { get { return Hub?Hub.GetResource("Wood"):Forest?Forest.wood:0; } }
    public int Wool { get { return Hub?Hub.GetResource("Wool"):Farm?Mathf.FloorToInt(Farm.Get("Wool")):0; } }
    // Retain the old property name for existing mine scripts, now using processed Fish Meat.
    public int Fish { get { return Hub?Hub.GetResource("FishMeat"):Fishing?Fishing.fishMeatWeight:0; } }
    public int Iron { get { return Hub?Hub.GetResource("Iron"):ore.Count("RefinedOre"); } }
    public bool SpendWood(int amount) {
        if(amount<0)return false;if(amount==0)return true;
        return Hub?Hub.UseResource("Wood",amount):Forest && Forest.UseResource("Wood",amount);
    }
    public bool SpendWool(int amount) {
        if(amount<0)return false;if(amount==0)return true;
        return Hub?Hub.UseResource("Wool",amount):Farm && Farm.UseResource("Wool",amount);
    }
    public bool SpendFish(int amount) {
        if(amount<0)return false;if(amount==0)return true;
        return Hub?Hub.UseResource("FishMeat",amount):Fishing && Fishing.UseResource("FishMeat",amount);
    }
    public bool SpendIron(int amount) {
        if(amount<0)return false;if(amount==0)return true;
        return Hub?Hub.UseResource("Iron",amount):ore.Spend("RefinedOre",amount);
    }
    public string Summary { get { return "Wood "+Wood+" | Wool "+Wool+" | Fish Meat "+Fish+" kg | Iron "+Iron; } }
}
}
