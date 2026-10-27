using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace Rishi.OreRoom {
public enum OreActionKind { Portal, PracticeForge, PracticeHopper, Timber, Insulation, Ration, Export, Build }
public class OreHubAction : MonoBehaviour {
    public OreActionKind kind;
    public OreRoom room;
    public OreHubIntegration hub;
    public int index;
    public string caption;
    public static OreHubAction Create(Transform parent,string title,Vector3 p,OreActionKind kind) {
        var g=OreVisuals.Part(parent,title,PrimitiveType.Cube,p,new Vector3(.65f,.45f,.45f),OreVisuals.Material("Metal",Color.gray));
        var action=g.AddComponent<OreHubAction>();action.kind=kind;action.caption=title;
        var label=OreVisuals.Label(g.transform,title,new Vector3(0,.9f,0),.04f);label.transform.localScale=new Vector3(1/.65f,1/.45f,1/.45f);
        return action;
    }
    void Start() {
        if(!hub)hub=FindAnyObjectByType<OreHubIntegration>();
        if(!room && hub)room=hub.room;
        var xr=GetComponent<XRSimpleInteractable>();if(!xr)xr=gameObject.AddComponent<XRSimpleInteractable>();
        xr.selectEntered.AddListener(_=>Perform());
    }
    public void Perform() {
        if(kind==OreActionKind.Portal) { if(hub)hub.Travel(index);return; }
        if(kind==OreActionKind.Build)return; // Ignore legacy construction actions from the removed adapter.
        if(!room || !room.progression)return;
        var p=room.progression;var inv=hub?hub.Inventory:null;
        switch(kind) {
        case OreActionKind.Portal: if(hub)hub.Travel(index);break;
        case OreActionKind.PracticeForge:p.PracticeForge();break;
        case OreActionKind.PracticeHopper:p.PracticeHopper();break;
        case OreActionKind.Timber:
            if(p.timber) { room.Notify("Timber bracing is already installed. Save Wood for the hub.");return; }
            if(inv==null || inv.Wood<1 || room.Count("IronBar")<1) { room.Notify("Timber bracing needs 1 Wood + 1 Bar. Chop Mason's Trees, collect Logs, and smelt a Bar.");return; }
            inv.SpendWood(1);room.Spend("IronBar",1);p.timber=true;room.drillBurnRate=1.2f;
            room.Notify("Forest Wood installed as timber bracing. Drill fuel burn falls from 2 to 1.2 per second.");break;
        case OreActionKind.Insulation:
            if(p.insulation) { room.Notify("Wool insulation is already fitted. Save Wool for construction.");return; }
            if(inv==null || inv.Wool<1 || room.Count("Coal")<1) { room.Notify("Insulation needs 1 Wool + 1 Coal. Shear Jiwon's Sheep and mine Coal.");return; }
            inv.SpendWool(1);room.Spend("Coal",1);p.insulation=true;room.lightBurnRate=.3f;
            room.Notify("Farm Wool installed as lamp insulation. Light fuel burn falls from 0.6 to 0.3 per second.");break;
        case OreActionKind.Ration:
            if(p.ration) { room.Notify("Ration lesson is complete. Continue to ore export.");return; }
            if(inv==null || inv.Fish<5 || room.Count("Coal")<1) { room.Notify("Cooked ration needs 5 kg Fish + 1 Coal. Catch fish in Yuyangsong's room and mine Coal.");return; }
            inv.SpendFish(5);room.Spend("Coal",1);p.ration=true;room.rationSeconds=60;
            room.Notify("Coastal Fish cooked into a miner's ration. For 60 seconds mining is faster and wears the Pickaxe half as much.");break;
        case OreActionKind.Export:
            if(p.stage<8) { room.Notify("Ore export is locked. Finish the mine's supply lessons first.");return; }
            if(!inv.Fishing || room.Count("RefinedOre")<1) { room.Notify("Need 1 Refined Ore. Trade a Bar on the cart first; the fishing inventory must be present.");return; }
            room.Spend("RefinedOre",1);inv.Fishing.iron++;room.Notify("1 Refined Ore delivered as 1 Iron to the fishing inventory. It can now fund Dock / Net construction.");break;
        }
        if(kind==OreActionKind.Timber || kind==OreActionKind.Insulation || kind==OreActionKind.Ration) {
            OreVisuals.Burst(transform.position,Color.green);
            var t=GetComponentInChildren<TextMesh>();if(t)t.text=caption+"\nINSTALLED";
        }
    }
}
}
