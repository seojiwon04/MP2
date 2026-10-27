using System.Collections.Generic;
using UnityEngine;
namespace Rishi.OreRoom {
public class OreRoom : MonoBehaviour {
    public bool spawnTestPlayer = true;
    [Header("Player Controls")]
    [Tooltip("Only used when Spawn Test Player is enabled. Shared-rig prefabs bind the hub XR Origin automatically.")]
    public bool useDesktopControls = true;
    public bool integratedRoom;
    [System.NonSerialized] public OreProgression progression;
    [System.NonSerialized] public OreHubIntegration hub;
    public float drillBurnRate=2, lightBurnRate=.6f, rationSeconds;
    public Vector3 World(Vector3 local) { return transform.TransformPoint(local); }
    public Vector3 Local(Vector3 world) { return transform.InverseTransformPoint(world); }
    public int Count(string key) { return resources.TryGetValue(key,out int n)?n:0; }
    public Vector3 spawn = new Vector3(0,.1f,-3.7f);
    public int randomSeed = 417;
    public OrePlayer player;
    OreSharedInventory sharedInventory;
    public OreSharedInventory SharedInventory { get {
        if(hub && hub.Inventory!=null)return hub.Inventory;
        return sharedInventory??(sharedInventory=new OreSharedInventory(this));
    } }
    public List<OreInteractable> nodes = new List<OreInteractable>();
    [System.NonSerialized] public OreRailPlanner planner = new OreRailPlanner();
    public Vector2Int entrance = new Vector2Int(-5,9), exit = new Vector2Int(5,9);
    public string message;
    readonly Dictionary<string,int> resources = new Dictionary<string,int>();
    readonly HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    readonly HashSet<string> seen = new HashSet<string>();
    readonly Dictionary<Vector2Int,OreInteractable> foundations=new Dictionary<Vector2Int,OreInteractable>();
    OreMinecart cart;
    Collider buildFloor;
    TextMesh ledger, popup;
    float popupTime, wildTime, messageTime;
    OreInteractable tutorialOwner;
    bool alternateRoute;
    System.Random random;
    public static readonly string[] names = { "Drill", "WorkLight", "Rail", "Foundation" };
    void Awake() {
        random = new System.Random(randomSeed);
        foreach(var key in new[]{"IronOre","Coal","Crystal","IronBar","RefinedOre"}) resources[key]=0;
        if(transform.Find("Cave floor")) BindLayout(); else CreateLayout();
        if(spawnTestPlayer) {
            var p=new GameObject("Rishi test player (desktop + XR)"); p.transform.position=World(spawn);
            player=p.AddComponent<OrePlayer>(); player.Init(this);
        }
        ledger=OreVisuals.Label(transform,"",new Vector3(0,2.4f,5.1f),.07f);
        popup=OreVisuals.Label(transform,"",Vector3.zero,.028f);
        wildTime=20; Notify("Welcome to the ore mine. Equipment is on the workbench.");
        if(!GetComponent<OreCaveLighting>())gameObject.AddComponent<OreCaveLighting>();
        if(integratedRoom && !spawnTestPlayer && !GetComponent<OreSharedRigAdapter>())gameObject.AddComponent<OreSharedRigAdapter>();
        if(integratedRoom && !GetComponent<OreIronWall>()) {
            var wall=gameObject.AddComponent<OreIronWall>();wall.room=this;
        }
    }
    public void CreateLayout() {
        random=new System.Random(randomSeed);
        Geometry();
        Spawn("Pickaxe",new Vector3(-2,1.55f,-3)); Spawn("Helmet",new Vector3(0,1,-3)); Spawn("Lantern",new Vector3(2,1,-3));
        Spawn("Smelter",new Vector3(-5,0,-2)); Spawn("Gate",new Vector3(0,0,4.2f));
        for(int i=0;i<3;i++) {
            SpawnNode("Iron",new Vector3(-5.8f+(float)random.NextDouble(),0,-.4f+i*.7f));
            SpawnNode("Coal",new Vector3(5.5f+(float)random.NextDouble()*.5f,0,-.8f+i*.9f));
            SpawnNode("Crystal",new Vector3(-2+i*2,0,4.3f+(float)random.NextDouble()*.3f));
        }
        for(int z=2;z<=9;z++) { AddRail(new Vector2Int(-5,z)); AddRail(new Vector2Int(5,z)); }
        for(int x=-4;x<=4;x++) AddRail(new Vector2Int(x,2));
        for(int x=-2;x<=2;x++) if(x!=0) AddRail(new Vector2Int(x,3));
        var c=Spawn("Minecart",Local(RailPosition(entrance))); cart=c.GetComponent<OreMinecart>(); if(Application.isPlaying) cart.Bind(this);
    }
    void BindLayout() {
        nodes.Clear(); planner.rails.Clear();
        buildFloor=transform.Find("Cave floor").GetComponent<Collider>();
        foreach(var item in GetComponentsInChildren<OreInteractable>(true)) {
            item.Bind(this);
            if(item.kind==OreKind.Iron || item.kind==OreKind.Coal || item.kind==OreKind.Crystal) nodes.Add(item);
            if(item.kind==OreKind.Rail) planner.rails.Add(GridCell(Local(item.transform.position)));
        }
        cart=GetComponentInChildren<OreMinecart>(true); if(cart && Application.isPlaying) cart.Bind(this);
    }
    GameObject Spawn(string name,Vector3 p) {
        var prefab=Resources.Load<GameObject>("RishiOre/Prefabs/"+name);
        if(!prefab) { Debug.LogError("Missing ore prefab: "+name+". Run Tools > Rishi Ore Room > Generate / Open Test Room."); enabled=false; return new GameObject("Missing "+name); }
        var g=Instantiate(prefab,p,Quaternion.identity,transform);
        var interact=g.GetComponent<OreInteractable>(); g.transform.localPosition=p; g.transform.localRotation=Quaternion.identity;
        if(interact && Application.isPlaying)interact.Bind(this); return g;
    }
    void SpawnNode(string type,Vector3 p) { var g=Spawn(type,p); var node=g.GetComponent<OreInteractable>(); if(node) nodes.Add(node); }
    void Update() {
        rationSeconds=Mathf.Max(0,rationSeconds-Time.deltaTime);
        messageTime-=Time.deltaTime;
        if(messageTime<=0) message="";
        if(ledger) ledger.text="ORE MINE\n"+ResourcesText();
        if(popup) {
            popupTime-=Time.deltaTime;
            bool close=player && tutorialOwner && Vector3.Distance(player.view.transform.position,tutorialOwner.transform.position)<2.5f;
            popup.gameObject.SetActive(popupTime>0 && close && player.IsFocused(tutorialOwner));
        }
        wildTime-=Time.deltaTime;
        if(wildTime<=0 && nodes.Count<12) { SpawnNode(random.Next(2)==0?"Iron":"Coal",new Vector3(-4+(float)random.NextDouble()*8,0,-.6f)); wildTime=18+(float)random.NextDouble()*14; }
    }
    public void Encounter(OreInteractable obj) {
        if(!obj || !player || !popup || popupTime>0 || Vector3.Distance(player.view.transform.position,obj.transform.position)>2.5f) return;
        switch(obj.kind) {
        case OreKind.Pickaxe:
            if(Teach("GrabTool","Grab the pickaxe to use it as a mining tool.",obj)) return;
            Teach("RepairTool","Alternate-use a held pickaxe to spend 1 Bar and restore Durability.",obj); break;
        case OreKind.Helmet:
            if(Teach("WearHelmet","Pick up the helmet to wear it and turn on its headlamp.",obj)) return;
            Teach("Headlamp","H / tap X toggles the headlamp; desktop X / hold controller X removes the helmet.",obj); break;
        case OreKind.Lantern:
            if(Teach("LanternToggle","Use a held lantern to toggle its light; its Fuel falls while lit.",obj)) return;
            Teach("LanternRefuel","Alternate-use the held lantern to spend 1 Coal and restore Fuel.",obj); break;
        case OreKind.Iron: case OreKind.Coal: case OreKind.Crystal:
            if(Teach("Harvest:"+obj.resource,"Mine "+obj.resource+" with the held pickaxe; collect the fragments for resources.",obj)) return;
            Teach("ShatterAndDeath","Mining creates collectible fragments. At zero Remaining Material, the deposit disappears.",obj); break;
        case OreKind.Chunk: Teach("CollectDrops","E / Y collects nearby dropped resources without precise aiming.",obj); break;
        case OreKind.Smelter:
            if(Teach("SmeltCoal","Normal use: 2 Iron Ore + 1 Coal makes 1 Bar.",obj)) return;
            if(Teach("SmeltCrystal","Alternate use: 1 Iron Ore + 1 Crystal makes 1 Bar.",obj)) return;
            Teach("SmelterHeat","Crafting raises Smelter Heat; it cools over time.",obj); break;
        case OreKind.Foundation: Teach("Foundation","Build this plate first, then place a drill on the SAME tile. Supported Machines rises to 1.",obj); break;
        case OreKind.Drill:
            if(Teach("AutomaticDrill","Drill Fuel falls as the bit turns. Nearby ore is mined automatically; Drill Output counts successful units.",obj)) return;
            Teach("CollisionFuel","Alternate-use this drill to drop 1 Coal into its yellow hopper. Contact consumes the fragment and raises Fuel.",obj); break;
        case OreKind.WorkLight:
            if(Teach("WorkLight","Place a work light on the grid, then use it to toggle illumination.",obj)) return;
            Teach("WorkLightRefuel","Alternate-use a work light to spend 1 Coal and restore its Fuel.",obj); break;
        case OreKind.Cart:
            if(Teach("CartTrade","Use the bars on this cart: spend 1 Bar to receive 2 Refined Ore.",obj)) return;
            Teach("CartPath","The cart follows the curved rail route into the fenced upper mine and back.",obj); break;
        case OreKind.Gate: Teach("BuildBypass","Build the missing rail at (0,3), then use this lever to close the main route and send the cart down the bypass.",obj); break;
        }
    }
    bool Teach(string key,string text,OreInteractable obj) {
        if(seen.Contains(key) || popupTime>0) return false;
        seen.Add(key); tutorialOwner=obj;
        popup.transform.position=obj.transform.position+Vector3.up*.8f;
        popup.text=WrapText(text,40); popupTime=4;
        foreach(var label in obj.GetComponentsInChildren<OreBillboard>()) label.suppressedUntil=Time.time+4;
        return true;
    }
    public void SpawnChunk(string resource,Vector3 p) {
        var g=Spawn("Chunk",Local(p)); var item=g.GetComponent<OreInteractable>(); item.resource=resource;
        if(item.status) item.status.text=resource+"\nCollect";
        var rb=g.GetComponent<Rigidbody>(); if(rb) rb.AddForce(new Vector3((float)random.NextDouble()-.5f,1.7f,(float)random.NextDouble()-.5f),ForceMode.Impulse);
    }
    public void FeedDrill(OreInteractable drill) {
        if(!drill || drill.kind!=OreKind.Drill) return;
        if(drill.charge>=100) { Notify("Drill is full. Wait for fuel to decrease."); return; }
        if(!Spend("Coal",1)) return;
        var parcel=Spawn("Chunk",Local(drill.transform.TransformPoint(new Vector3(.5f,2,0)))).GetComponent<OreInteractable>();
        parcel.resource="Coal"; parcel.reservedForDrill=true; if(parcel.status) parcel.status.text="Coal fuel parcel";
        var rb=parcel.GetComponent<Rigidbody>();
        if(rb) { rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero; }
        StartCoroutine(ReleaseFuelParcel(parcel));
        Notify("Coal falling into the yellow drill hopper.");
    }
    System.Collections.IEnumerator ReleaseFuelParcel(OreInteractable parcel) {
        yield return new WaitForSeconds(3);
        if(parcel) parcel.reservedForDrill=false;
    }
    public void Add(string key,int amount) { if(amount<=0)return; resources[key]=Count(key)+amount; if(progression)progression.RecordResource(key,amount); Notify("+"+amount+" "+(key=="RefinedOre"?"Refined Ore":key)); }
    public bool Spend(string key,int amount) {
        if(amount<0)return false;
        if(Count(key)<amount) { Notify("Need "+amount+" "+key+"; have "+Count(key)+". Mine deposits or smelt Bars first."); return false; } resources[key]-=amount; return true;
    }
    public bool Exchange(string a,int av,string b,int bv,string output,int quantity) {
        if(resources[a]<av || (b!=null && resources[b]<bv)) { Notify("Recipe needs "+av+" "+a+(b!=null?" + "+bv+" "+b:"")+". Gather the missing ingredients first."); return false; }
        resources[a]-=av; if(b!=null) resources[b]-=bv; Add(output,quantity);
        if(progression && output=="IronBar")progression.craftedBars+=quantity; return true;
    }
    public string ResourcesText() { if(progression)return progression.InventoryText(); return "Ore "+resources["IronOre"]+" | Coal "+resources["Coal"]+" | Crystal "+resources["Crystal"]+"\nBars "+resources["IronBar"]+" | Refined Ore "+resources["RefinedOre"]; }
    // Integration boundary: the shared player inventory can read or transfer the room's output.
    public int RefinedOre { get { return resources.TryGetValue("RefinedOre",out int value)?value:0; } }
    public int TakeRefinedOre() {
        int amount=RefinedOre;
        if(resources.ContainsKey("RefinedOre")) resources["RefinedOre"]=0;
        return amount;
    }
    public void Notify(string text) { message=text; messageTime=4; }
    static string WrapText(string text,int width) {
        var result=new System.Text.StringBuilder(); int column=0;
        foreach(var word in text.Split(' ')) {
            if(column>0 && column+word.Length+1>width) { result.Append('\n'); column=0; }
            if(column>0) { result.Append(' '); column++; }
            result.Append(word); column+=word.Length;
        }
        return result.ToString();
    }
    public string BuildName(int type) { return names[Mathf.Clamp(type,0,names.Length-1)]; }
    public bool TryGetGridTile(Ray ray,float range,out Vector3 position) {
        position=Vector3.zero;
        // Query only the real cave floor: tools, chunks, rails, and previews cannot steal the hit.
        if(!buildFloor || !buildFloor.Raycast(ray,out RaycastHit hit,range) || hit.normal.y<.8f) return false;
        var cell=GridCell(Local(hit.point));
        position=World(new Vector3(cell.x,.02f,cell.y));
        return true;
    }
    public static Vector2Int GridCell(Vector3 p) {
        return new Vector2Int(Mathf.FloorToInt(p.x+.5f),Mathf.FloorToInt(p.z+.5f));
    }
    public string BuildBlockedReason(Vector3 p,int type) {
        var cell=GridCell(Local(p));
        if(progression && !progression.CanBuild(type))return "This building is locked. Complete the current mine goal first.";
        if(type<0 || type>=names.Length) return "Choose a building first.";
        if(Mathf.Abs(cell.x)>4 || cell.y < -1 || cell.y>4) return "Outside the building area. Aim at the open central cave floor.";
        bool hasFoundation=foundations.ContainsKey(cell);
        if(type==0 && !hasFoundation) return "Build a Foundation first (4 / build menu).";
        if(type==3 && hasFoundation) return "This tile already has a Foundation.";
        if(hasFoundation && type!=0 && type!=3) return "This Foundation supports a Drill. Put lights and rails on a different tile.";
        if(occupied.Contains(cell)) return "A building already occupies this tile. Select a clear tile instead.";
        if(planner.rails.Contains(cell)) return "This tile already contains rail.";
        if(type==2 && cell.y<1) return "Place rails in the track area.";
        // Only solid structures block a tile. Loose ore, pickup triggers, and the moving cart do not.
        Vector3 center=World(new Vector3(cell.x,.55f,cell.y));
        foreach(var collider in Physics.OverlapBox(center,new Vector3(.4f,.45f,.4f),transform.rotation,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)) {
            if(collider==buildFloor) continue;
            if(player && player.IsPlayerBody(collider)) return "Step away from this tile.";
            var obj=collider.GetComponentInParent<OreInteractable>();
            if(obj && (obj.kind==OreKind.Chunk || obj.grabbable || obj.kind==OreKind.Cart || obj.kind==OreKind.Rail || (obj.kind==OreKind.Foundation && type==0))) continue;
            return "Clear the rock or structure from this tile.";
        }
        int price=type==0?2:1;
        if(resources["IronBar"]<price) return "Need "+price+" Bar"+(price==1?".":"s.")+" Smelt Iron Ore with Coal or Crystal first.";
        return "";
    }
    public bool CanBuild(Vector3 p,int type) { return string.IsNullOrEmpty(BuildBlockedReason(p,type)); }
    public void Build(Vector3 p,int type) {
        var cell=GridCell(Local(p)); p=World(new Vector3(cell.x,.02f,cell.y));
        string reason=BuildBlockedReason(p,type);
        if(!string.IsNullOrEmpty(reason)) { Notify(reason); return; }
        if(!Spend("IronBar",type==0?2:1)) return;
        if(type==2) AddRail(cell);
        else if(type==3) { var plate=Spawn("Foundation",Local(p)).GetComponent<OreInteractable>(); foundations[cell]=plate; }
        else {
            if(type==0) p+=Vector3.up*.12f;
            Spawn(names[type],Local(p)); occupied.Add(cell);
            if(type==0) { foundations[cell].supportedMachines++; foundations[cell].Refresh(); }
        }
        if(progression)progression.RecordBuilding(type);
        if(cart) cart.Replan(); Notify("Placed "+names[type]+" on grid "+cell);
    }
    void AddRail(Vector2Int cell) { if(planner.rails.Add(cell)) Spawn("Rail",Local(RailPosition(cell)-transform.up*.3f)); }
    public Vector3 RailPosition(Vector2Int c) { return World(new Vector3(c.x,.3f+Mathf.Max(0,c.y-5)*.42f,c.y)); }
    public void ToggleRoute() {
        alternateRoute=!alternateRoute; var junction=new Vector2Int(0,2);
        if(alternateRoute) planner.blocked.Add(junction); else planner.blocked.Remove(junction);
        var lever=GetComponentsInChildren<OreInteractable>();
        foreach(var obj in lever) if(obj.kind==OreKind.Gate) {
            var handle=obj.transform.Find("Junction lever"); if(handle) handle.localRotation=Quaternion.Euler(0,0,alternateRoute?35:-35);
        }
        if(cart) cart.Replan(); Notify(alternateRoute?"Main junction blocked. Cart needs a complete bypass at (0,3).":"Main junction open.");
    }
    void Geometry() {
        var stone=OreVisuals.Material("Stone",new Color(.18f,.2f,.23f)); var earth=OreVisuals.Material("Ground",new Color(.25f,.23f,.21f));
        var wood=OreVisuals.Material("Wood",new Color(.26f,.13f,.055f)); var metal=OreVisuals.Material("Metal",new Color(.25f,.29f,.33f));
        buildFloor=OreVisuals.Part(transform,"Cave floor",PrimitiveType.Cube,new Vector3(0,-.3f,0),new Vector3(16,.6f,13),earth).GetComponent<Collider>();
        OreVisuals.Part(transform,"Cave roof",PrimitiveType.Cube,new Vector3(0,4.7f,0),new Vector3(17,1,14),stone);
        OreVisuals.Part(transform,"Back wall",PrimitiveType.Cube,new Vector3(0,2,-6.5f),new Vector3(17,5,1),stone);
        OreVisuals.Part(transform,"Tunnel divider",PrimitiveType.Cube,new Vector3(0,2,6.5f),new Vector3(7,5,1),stone);
        for(int side=-1;side<=1;side+=2) {
            OreVisuals.Part(transform,"Cave side",PrimitiveType.Cube,new Vector3(side*8.2f,2,0),new Vector3(1.5f,5,15),stone);
            OreVisuals.Part(transform,"Tunnel outer wall",PrimitiveType.Cube,new Vector3(side*7,2,6.5f),new Vector3(2,5,1),stone);
            for(int z=-5;z<=5;z+=2) {
                var rock=OreVisuals.Part(transform,"Rock face",PrimitiveType.Sphere,new Vector3(side*7.6f,1.6f,z),new Vector3(2.6f,3.7f,2.8f),stone);
                rock.transform.localRotation=Quaternion.Euler(17,z*13,22);
            }
            // Uphill tracks are behind a fence; this is scenery, not a player route.
            for(int z=6;z<=10;z++) {
                float y=(z-5)*.42f;
                OreVisuals.Part(transform,"Upper tunnel bed",PrimitiveType.Cube,new Vector3(side*5,y-.25f,z),new Vector3(2,.5f,1.2f),earth);
                for(int edge=-1;edge<=1;edge+=2) OreVisuals.Part(transform,"Tunnel wall",PrimitiveType.Cube,new Vector3(side*5+edge*1.2f,y+1.5f,z),new Vector3(.5f,3,1.3f),stone);
                OreVisuals.Part(transform,"Tunnel roof",PrimitiveType.Cube,new Vector3(side*5,y+3,z),new Vector3(2.7f,.5f,1.3f),stone);
            }
            OreVisuals.Part(transform,"Dark end of upper mine",PrimitiveType.Cube,new Vector3(side*5,4.1f,10.2f),new Vector3(2,3,.4f),stone);
            // Layer 2 colliders exclude only the cart, not the player or tools.
            var barrier=OreVisuals.Part(transform,"Player safety fence collider",PrimitiveType.Cube,new Vector3(side*5,1.5f,5.55f),new Vector3(2,3,.15f),metal); barrier.layer=2; barrier.GetComponent<Renderer>().enabled=false;
            for(int x=-1;x<=1;x++) OreVisuals.Part(transform,"Fence post",PrimitiveType.Cube,new Vector3(side*5+x*.9f,.6f,5.5f),new Vector3(.08f,1.2f,.08f),wood);
            for(int h=0;h<2;h++) OreVisuals.Part(transform,"Fence beam",PrimitiveType.Cube,new Vector3(side*5,.35f+h*.55f,5.5f),new Vector3(2,.08f,.08f),wood,false);
        }
        OreVisuals.Part(transform,"Workbench",PrimitiveType.Cube,new Vector3(0,.85f,-3),new Vector3(5.5f,.15f,.9f),wood);
        for(int s=-1;s<=1;s+=2) OreVisuals.Part(transform,"Bench legs",PrimitiveType.Cube,new Vector3(s*2.3f,.4f,-3),new Vector3(.15f,.8f,.7f),wood);
        OreVisuals.Label(transform,"ORE MINE\nEquipment bench",new Vector3(0,2,-5.8f),.13f);
        OreVisuals.Label(transform,"VR: grip grab / release drop\nA interact | Y collect drops\nX headlamp / hold X remove helmet\nB build menu / hold B exit",new Vector3(4,1.8f,-5.7f),.045f);
        for(int x=-4;x<=4;x+=4) {
            var g=new GameObject("Warm cave lamp"); g.transform.SetParent(transform,false); g.transform.localPosition=new Vector3(x,3.1f,-1);
            var l=g.AddComponent<Light>(); l.type=LightType.Point; l.color=new Color(1,.69f,.34f); l.range=3; l.intensity=.04f;
        }
        // Small local bench light keeps gear findable without lighting the entire cave.
        var benchLamp=new GameObject("Equipment bench glow"); benchLamp.transform.SetParent(transform,false);
        benchLamp.transform.localPosition=new Vector3(0,2,-3);
        var benchLight=benchLamp.AddComponent<Light>(); benchLight.type=LightType.Point;
        benchLight.color=new Color(1,.72f,.4f); benchLight.range=2.4f; benchLight.intensity=.18f;
        if(!integratedRoom) {
        var sun=new GameObject("Ore room soft fill"); sun.transform.SetParent(transform,false); sun.transform.rotation=Quaternion.Euler(60,-25,0);
        var fill=sun.AddComponent<Light>(); fill.type=LightType.Directional; fill.intensity=.025f; fill.color=new Color(.57f,.66f,.8f);
        }
    }
}
}
