using UnityEngine;
namespace Rishi.OreRoom {
public class OreSharedInventory {
    readonly OreRoom ore;
    public OreSharedInventory(OreRoom room) { ore=room; }
    public global::PlayerResources Forest { get { return Object.FindAnyObjectByType<global::PlayerResources>(); } }
    public FishingResources Fishing { get { return FishingResources.Instance; } }
    public SheepFarm.PlayerResources Farm { get { return SheepFarm.PlayerResources.Instance; } }
    public int Wood { get { return (Forest?Forest.wood:0)+(Fishing?Fishing.wood:0); } }
    public int Wool { get { return Farm?Mathf.FloorToInt(Farm.Get("Wool")):0; } }
    public int Fish { get { return Fishing?Fishing.fishWeight:0; } }
    public int Iron { get { return ore.Count("RefinedOre")+(Fishing?Fishing.iron:0); } }
    public bool SpendWood(int amount) {
        if(amount<0 || Wood<amount)return false;
        int forestAmount=Forest?Mathf.Min(Forest.wood,amount):0;
        if(forestAmount>0)Forest.SpendWood(forestAmount);
        if(amount>forestAmount)Fishing.wood-=amount-forestAmount;
        return true;
    }
    public bool SpendWool(int amount) { return amount>=0 && Farm && Farm.Spend("Wool",amount); }
    public bool SpendFish(int amount) { if(amount<0 || !Fishing || Fishing.fishWeight<amount)return false; Fishing.fishWeight-=amount;return true; }
    public bool SpendIron(int amount) {
        if(amount<0 || Iron<amount)return false;
        int local=Mathf.Min(ore.Count("RefinedOre"),amount);ore.Spend("RefinedOre",local);
        if(amount>local)Fishing.iron-=amount-local; return true;
    }
    public string Summary { get { return "Wood "+Wood+" | Wool "+Wool+" | Fish "+Fish+" kg | Iron "+Iron; } }
}
}
