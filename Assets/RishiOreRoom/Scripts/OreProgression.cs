using System.Collections;
using UnityEngine;
namespace Rishi.OreRoom {
public class OreProgression : MonoBehaviour {
    public OreRoom room;
    public int stage, earnedIron, craftedBars, repairs, refuels;
    public bool timber, insulation, ration;
    public readonly bool[] sandboxes=new bool[3];
    bool revealing;
    float revealEnd;
    int nextStage;
    GameObject furnaceSample;
    TextMesh vrGoal;
    public string revealText;
    OreInteractable practiceRock, practiceDrill;
    public int practiceLoads;
    readonly string[] reveals={
        "", "PRACTICE VEIN: a reusable rock lets you rehearse mining safely.",
        "SMELTER: a furnace turns raw minerals into useful metal Bars.",
        "MINE WORKSHOP: repair worn tools and build lights for safer work.",
        "AUTOMATIC DRILL: a foundation supports a machine that mines for you.",
        "TIMBER BRACING: forest Wood can support your mine machinery.",
        "WOOL INSULATION: farm Wool protects your lamps from wasting fuel.",
        "MINER'S RATION: fish from the coast feeds the miner for efficient work.",
        "ORE EXPORT: the rail cart turns Bars into Refined Ore for the settlement."
    };
    readonly string[] goals={
        "Equip the Helmet and hold the Pickaxe.",
        "Mine real iron and collect 3 Iron Ore. The green practice vein is optional.",
        "Mine Coal or Crystal, then smelt your first Bar.",
        "Repair the worn Pickaxe with 1 Bar AND refuel the Lantern with 1 Coal.",
        "Build Foundation (4), then Drill (1) on the same tile near ore. Reach 2 Drill Output.",
        "Bring 1 forest Wood + 1 Bar to the timber order at the supply desk.",
        "Bring 1 farm Wool + 1 Coal to the insulation order.",
        "Bring 5 kg Fish + 1 Coal to cook the miner's ration.",
        "Trade Bars on the cart for Refined Ore. Return to the hub to build roads and civilizations."
    };
    public bool Revealing { get { return revealing; } }
    public string Goal { get { return revealing?"Read the new unlock introduction...":goals[stage]; } }
    public string LockedReason(OreKind kind) {
        return "Locked: "+goals[Mathf.Clamp(stage,0,goals.Length-1)];
    }
    void Start() {
        room=GetComponent<OreRoom>(); room.progression=this;
        practiceRock=Instantiate(Resources.Load<GameObject>("RishiOre/Prefabs/Iron"),room.World(new Vector3(-2,0,-.8f)),room.transform.rotation,room.transform).GetComponent<OreInteractable>();
        practiceRock.name="Practice vein - no resources"; practiceRock.practiceVein=true; practiceRock.Bind(room);
        foreach(var renderer in practiceRock.GetComponentsInChildren<Renderer>())if(!renderer.GetComponent<TextMesh>())renderer.material.color=Color.green;
        practiceDrill=Instantiate(Resources.Load<GameObject>("RishiOre/Prefabs/Drill"),room.World(new Vector3(2,0,-.5f)),room.transform.rotation,room.transform).GetComponent<OreInteractable>();
        practiceDrill.name="Loan training drill"; practiceDrill.practiceMachine=true; practiceDrill.Bind(room);
        CreateAction("Practice furnace",new Vector3(-3,1,-1.8f),OreActionKind.PracticeForge);
        CreateAction("Practice hopper",new Vector3(2,1,-1.8f),OreActionKind.PracticeHopper);
        CreateAction("Timber bracing",new Vector3(-2,1,-5.5f),OreActionKind.Timber);
        CreateAction("Wool insulation",new Vector3(0,1,-5.5f),OreActionKind.Insulation);
        CreateAction("Miner ration",new Vector3(2,1,-5.5f),OreActionKind.Ration);
        ApplyVisibility();
    }
    void CreateAction(string label,Vector3 p,OreActionKind kind) {
        var a=OreHubAction.Create(transform,label,p,kind); a.room=room;
    }
    public void RecordResource(string resource,int n) { if(resource=="IronOre")earnedIron+=n; }
    public void RecordBuilding(int type) { }
    public bool CanBuild(int type) { return type==1?stage>=3:type==2?stage>=8:stage>=4; }
    public bool CanUse(OreKind kind) {
        switch(kind) {
        case OreKind.Iron: case OreKind.Chunk:return stage>=1;
        case OreKind.Coal: case OreKind.Crystal: case OreKind.Smelter:return stage>=2;
        case OreKind.WorkLight:return stage>=3;
        case OreKind.Foundation: case OreKind.Drill:return stage>=4;
        case OreKind.Cart: case OreKind.Gate: case OreKind.Rail:return stage>=8;
        default:return true;
        }
    }
    void Update() {
        if(!room)return;
        if(room.player && room.player.VR) {
            if(!vrGoal) { vrGoal=OreVisuals.Label(room.transform,"",Vector3.zero,.012f);vrGoal.GetComponent<OreBillboard>().showDistance=2; }
            vrGoal.gameObject.SetActive(room.player.InMine);
            var eye=room.player.view.transform;
            vrGoal.transform.position=eye.position+eye.forward*1.2f+eye.up*.25f;
            vrGoal.text=Wrap(revealing?revealText:"MINE GOAL "+stage+"/8: "+Goal,42);
        }
        if(revealing) {
            if(Time.time>=revealEnd) { stage=nextStage; if(stage==3){repairs=0;refuels=0;} revealing=false; ApplyVisibility(); room.Notify("New goal: "+Goal); }
            return;
        }
        bool pass=false;
        var p=room.player;
        switch(stage) {
        case 0:pass=p && p.HasHelmet && p.held && p.held.kind==OreKind.Pickaxe;break;
        case 1:pass=earnedIron>=3;break;
        case 2:pass=craftedBars>=1;break;
        case 3:pass=repairs>0 && refuels>0;break;
        case 4:
            int output=0;foreach(var drill in room.GetComponentsInChildren<OreInteractable>())if(drill.kind==OreKind.Drill&&!drill.practiceMachine)output+=drill.drillOutput;
            pass=output>=2;break;
        case 5:pass=timber;break;
        case 6:pass=insulation;break;
        case 7:pass=ration;break;
        }
        if(pass && stage<8) { nextStage=stage+1; revealText=reveals[nextStage]; revealEnd=Time.time+3; revealing=true; room.Notify("Mastery threshold cleared!"); }
    }
    void ApplyVisibility() {
        // Keep the actual mine visible. Lessons unlock use, not the existence of the room.
        // Practice props and new supply orders are still introduced one at a time.
        foreach(var item in room.GetComponentsInChildren<OreInteractable>(true)) {
            if(item.practiceVein)item.gameObject.SetActive(stage>=1);
            else if(item.practiceMachine)item.gameObject.SetActive(stage>=4);
            else if(!item.collected)item.gameObject.SetActive(true);
        }
        foreach(var a in room.GetComponentsInChildren<OreHubAction>(true)) {
            bool show=true;
            switch(a.kind) {
            case OreActionKind.PracticeForge:show=stage>=2;break;
            case OreActionKind.PracticeHopper:show=stage>=4;break;
            case OreActionKind.Timber:show=stage>=5;break;
            case OreActionKind.Insulation:show=stage>=6;break;
            case OreActionKind.Ration:show=stage>=7;break;
            }
            a.gameObject.SetActive(show);
        }
    }
    public void PracticeForge() {
        if(stage<2)return;
        practiceLoads++;
        if(sandboxes[1]) { room.Notify("Practice forging already passed. Use the real Smelter to earn Bars.");return; }
        if(!furnaceSample) { furnaceSample=new GameObject("Loan furnace sample");furnaceSample.transform.SetParent(room.transform,false);furnaceSample.transform.localPosition=new Vector3(-3,1.5f,-1.8f); }
        if(practiceLoads<2) {
            OreVisuals.Part(furnaceSample.transform,"Loan iron",PrimitiveType.Sphere,new Vector3(-.15f,0,0),Vector3.one*.17f,OreVisuals.Material("IronOre",Color.gray),false);
            OreVisuals.Part(furnaceSample.transform,"Loan coal",PrimitiveType.Sphere,new Vector3(.15f,0,0),Vector3.one*.17f,OreVisuals.Material("Coal",Color.black),false);
            room.Notify("Practice furnace loaded with loan ingredients. Use it again to complete the heat cycle.");
        } else {
            foreach(Transform child in furnaceSample.transform)Destroy(child.gameObject);
            OreVisuals.Part(furnaceSample.transform,"Practice bar - no inventory value",PrimitiveType.Cube,Vector3.zero,new Vector3(.45f,.12f,.18f),OreVisuals.Material("Metal",Color.gray),false);
            CompleteSandbox(1);
        }
    }
    public void PracticeHopper() {
        if(stage<4 || !practiceDrill)return;
        if(sandboxes[2]) { room.Notify("Hopper practice already complete. Feed real Coal to your built drill.");return; }
        var parcel=Instantiate(Resources.Load<GameObject>("RishiOre/Prefabs/Chunk"),practiceDrill.transform.TransformPoint(new Vector3(.5f,2,0)),Quaternion.identity,room.transform).GetComponent<OreInteractable>();
        parcel.resource="Coal"; parcel.practiceFuel=true; parcel.reservedForDrill=true; parcel.Bind(room);
        if(parcel.status)parcel.status.text="Loan practice Coal";
        Destroy(parcel.gameObject,8); room.Notify("Loan Coal dropped. Watch it enter the yellow practice hopper.");
    }
    public void CompleteSandbox(int index) {
        if(sandboxes[index])return; sandboxes[index]=true;
        room.Notify(new[]{"Practice mining passed! No tool wear or resources spent.","Practice forging passed! Loan ingredients became a sample; real inventory is unchanged.","Practice hopper passed! The loan fragment entered the receiver and raised its Fuel."}[index]);
        OreVisuals.Burst(room.player?room.player.view.transform.position+room.player.view.transform.forward*1.5f:transform.position,Color.green);
    }
    public string InventoryText() {
        if(stage<1)return "Equipment lesson | Inventory unlocks after equipping the Helmet and Pickaxe.";
        string t="Iron Ore "+room.Count("IronOre");
        if(stage>=2)t+=" | Coal "+room.Count("Coal")+" | Crystal "+room.Count("Crystal")+"\nBars "+room.Count("IronBar");
        if(stage>=8)t+=" | Refined Ore "+room.Count("RefinedOre");
        return t;
    }
    static string Wrap(string text,int width) {
        var result=new System.Text.StringBuilder();int col=0;
        foreach(var word in text.Split(' ')) { if(col>0 && col+word.Length>width){result.Append('\n');col=0;}if(col>0){result.Append(' ');col++;}result.Append(word);col+=word.Length; }return result.ToString();
    }
    void OnGUI() {
        if(!room || !room.player || !room.player.InMine || room.player.VR)return;
        float w=Mathf.Min(490,Screen.width*.4f);
        var style=new GUIStyle(GUI.skin.box){fontSize=16,wordWrap=true,alignment=TextAnchor.UpperLeft};
        GUI.Box(new Rect(Screen.width-w-12,12,w,112),"MINE MASTERY "+stage+" / 8\n"+Goal,style);
        if(revealing)GUI.Box(new Rect(Screen.width*.2f,Screen.height*.3f,Screen.width*.6f,140),revealText+"\n\nThe new activity unlocks after this introduction.",style);
        if(stage>=5 && room.hub && room.hub.Inventory!=null)GUI.Box(new Rect(Screen.width-w-12,204,w,115),room.hub.Inventory.Summary+"\nDrill burn: "+room.drillBurnRate.ToString("0.0")+" / s | Lamp burn: "+room.lightBurnRate.ToString("0.0")+" / s\nRation: "+Mathf.CeilToInt(room.rationSeconds)+" s",style);
        string awards="";
        for(int i=0;i<3;i++)if(sandboxes[i])awards+="Practice "+(i+1)+" passed  ";
        if(stage>=8)awards+="Ore supplier unlocked";
        if(awards!="")GUI.Box(new Rect(Screen.width-w-12,130,w,65),awards,style);
    }
}
}
