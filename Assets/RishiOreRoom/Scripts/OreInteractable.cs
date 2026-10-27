using UnityEngine;
namespace Rishi.OreRoom {
public enum OreKind { Pickaxe, Helmet, Lantern, Iron, Coal, Crystal, Chunk, Smelter, Drill, WorkLight, Rail, Cart, Gate, Foundation }
public class OreInteractable : MonoBehaviour {
    public OreKind kind;
    public string resource = "IronOre";
    public string tutorial;
    public float remaining = 6, charge = 100, durability = 100;
    public bool grabbable;
    public float smelterHeat;
    public int drillOutput, supportedMachines;
    [System.NonSerialized] public bool reservedForDrill;
    [System.NonSerialized] public bool collected;
    Renderer chunkRenderer;
    Transform drillBit;
    Mesh generatedDrillMesh;
    public TextMesh status;
    public Light lamp;
    float cooldown;
    public OreRoom room;
    public bool practiceVein, practiceMachine, practiceFuel;
    public void Bind(OreRoom r) {
        room=r;
        if(grabbable) {
            // A generous single pickup volume makes thin handles and helmet rims selectable.
            var pickup=gameObject.AddComponent<BoxCollider>(); pickup.isTrigger=true;
            if(kind==OreKind.Pickaxe) { pickup.center=new Vector3(0,.05f,0); pickup.size=new Vector3(.8f,1.05f,.32f); }
            else if(kind==OreKind.Helmet) { pickup.center=Vector3.zero; pickup.size=new Vector3(.58f,.4f,.58f); }
            else { pickup.center=new Vector3(0,.32f,0); pickup.size=new Vector3(.48f,.72f,.48f); }
            var rb=GetComponent<Rigidbody>();
            // Equipment stays neatly displayed until first picked up; dropped equipment uses physics.
            if(rb) { rb.isKinematic=true; rb.useGravity=false; }
        }
        if(kind==OreKind.Chunk) {
            var fragment=transform.Find("Ore fragment");
            if(fragment) { fragment.localScale*=1.65f; chunkRenderer=fragment.GetComponent<Renderer>(); }
        }
        if(kind==OreKind.Drill) BuildPointedDrill();
        if(kind==OreKind.Smelter) {
            foreach(var label in GetComponentsInChildren<TextMesh>()) { label.text="SMELTER\nOre + Coal / Ore + Crystal"; if(!status) status=label; }
        }
        if(kind==OreKind.Cart) {
            var oldLoad=transform.Find("Cart load"); if(oldLoad) oldLoad.gameObject.SetActive(false);
            var existingBars=transform.Find("Trade bars");
            if(!existingBars) {
                var stack=new GameObject("Trade bars"); stack.transform.SetParent(transform,false);
                var metal=OreVisuals.Material("IronBar",new Color(.67f,.73f,.8f));
                for(int layer=0;layer<2;layer++) for(int index=0;index<3;index++) {
                    var bar=OreVisuals.Part(stack.transform,"Bar trade target",PrimitiveType.Cube,
                        new Vector3(layer==0?(index-1)*.2f:0,.48f+layer*.13f,layer==1?(index-1)*.2f:0),new Vector3(.17f,.11f,.65f),metal);
                    // A bar hit resolves to this cart's interactable and spends a player Bar.
                    bar.transform.localRotation=Quaternion.Euler(0,layer*90,0);
                }
            }
        }
        if(lamp) {
            if(kind==OreKind.Helmet) {
                lamp.enabled=false; lamp.type=LightType.Spot; lamp.range=12; lamp.intensity=5;
                lamp.spotAngle=68; lamp.innerSpotAngle=38; lamp.color=new Color(1,.91f,.76f);
                lamp.shadows=Application.platform==RuntimePlatform.Android?LightShadows.Hard:LightShadows.Soft; lamp.shadowBias=.03f; lamp.shadowNormalBias=.15f;
            } else if(kind==OreKind.Lantern || kind==OreKind.WorkLight) {
                lamp.range=kind==OreKind.WorkLight?5:4.5f; lamp.intensity=kind==OreKind.WorkLight?2:1.5f; lamp.shadows=Application.platform==RuntimePlatform.Android?LightShadows.None:LightShadows.Soft;
                lamp.shadowBias=.03f; lamp.shadowNormalBias=.15f;
            }
        }
        Refresh();
    }
    void Update() {
        if (!room) return;
        cooldown -= Time.deltaTime;
        if ((kind == OreKind.Lantern || kind == OreKind.WorkLight) && lamp && lamp.enabled) {
            charge = Mathf.Max(0, charge - room.lightBurnRate * Time.deltaTime); lamp.intensity = charge > 0 ? (kind==OreKind.WorkLight?2:1.5f) : 0;
        }
        if (kind == OreKind.Drill && !practiceMachine && charge > 0) {
            if(drillBit) drillBit.Rotate(Vector3.up,220*Time.deltaTime,Space.Self);
            charge = Mathf.Max(0, charge - room.drillBurnRate * Time.deltaTime);
            if (cooldown <= 0) {
                foreach (var v in room.nodes) if (v && v.isActiveAndEnabled && !v.practiceVein && Vector3.Distance(transform.position, v.transform.position) < 2.3f && v.remaining > 0) { if(v.Mine(false)) drillOutput++; break; }
                cooldown = 3;
            }
        }
        if(kind==OreKind.Smelter) smelterHeat=Mathf.Max(0,smelterHeat-5*Time.deltaTime);
        if (status) Refresh();
    }
    void BuildPointedDrill() {
        var body=transform.Find("Drill body");
        if(body) { body.localPosition=new Vector3(0,.8f,0); body.localScale=new Vector3(.6f,.6f,.6f); }
        var oldBit=transform.Find("Drill bit"); if(oldBit) oldBit.gameObject.SetActive(false);
        // The motor sits above a visible shaft and a cone whose apex faces the ground.
        OreVisuals.Part(transform,"Drill shaft",PrimitiveType.Cylinder,new Vector3(0,.49f,0),new Vector3(.09f,.075f,.09f),OreVisuals.Material("Metal",Color.gray));
        var tip=new GameObject("Pointed downward drill bit"); tip.transform.SetParent(transform,false); drillBit=tip.transform;
        const int segments=16;
        var vertices=new Vector3[segments*6]; var triangles=new int[segments*6];
        for(int i=0;i<segments;i++) {
            float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;
            Vector3 current=new Vector3(Mathf.Cos(a)*.18f,.45f,Mathf.Sin(a)*.18f);
            Vector3 next=new Vector3(Mathf.Cos(b)*.18f,.45f,Mathf.Sin(b)*.18f);
            int v=i*6;
            vertices[v]=new Vector3(0,.04f,0); vertices[v+1]=current; vertices[v+2]=next;
            vertices[v+3]=new Vector3(0,.45f,0); vertices[v+4]=next; vertices[v+5]=current;
            for(int k=0;k<6;k++) triangles[v+k]=v+k;
        }
        generatedDrillMesh=new Mesh { name="Downward drill cone",vertices=vertices,triangles=triangles };
        generatedDrillMesh.RecalculateNormals(); generatedDrillMesh.RecalculateBounds();
        tip.AddComponent<MeshFilter>().sharedMesh=generatedDrillMesh;
        tip.AddComponent<MeshRenderer>().sharedMaterial=OreVisuals.Material("Gold",new Color(.75f,.65f,.3f));
        var collider=tip.AddComponent<MeshCollider>(); collider.sharedMesh=generatedDrillMesh; collider.convex=true;
        if(status) status.transform.localPosition=Vector3.up*1.4f;
        var hopper=new GameObject("Coal hopper collision receiver"); hopper.transform.SetParent(transform,false);
        hopper.transform.localPosition=new Vector3(.5f,1.08f,0);
        var trigger=hopper.AddComponent<BoxCollider>(); trigger.isTrigger=true; trigger.size=new Vector3(.48f,.4f,.48f);
        var receiver=hopper.AddComponent<OreDrillFuelReceiver>(); receiver.drill=this;
        var rimMaterial=OreVisuals.Material("Gold",Color.yellow);
        for(int side=-1;side<=1;side+=2) {
            OreVisuals.Part(hopper.transform,"Coal hopper rim",PrimitiveType.Cube,new Vector3(side*.21f,0,0),new Vector3(.04f,.09f,.44f),rimMaterial,false);
            OreVisuals.Part(hopper.transform,"Coal hopper rim",PrimitiveType.Cube,new Vector3(0,0,side*.21f),new Vector3(.44f,.09f,.04f),rimMaterial,false);
        }
    }
    void OnDestroy() { if(generatedDrillMesh) Destroy(generatedDrillMesh); }
    public string Hint() {
        switch (kind) {
        case OreKind.Iron: case OreKind.Coal: case OreKind.Crystal: return "Use held pickaxe to mine " + resource;
        case OreKind.Pickaxe: return "Grab pickaxe; aim at ore to mine";
        case OreKind.Helmet: return "Pick up to wear helmet; H / left primary toggles light";
        case OreKind.Lantern: return "Grab lantern; use toggles; alternate refuels (1 Coal)";
        case OreKind.Chunk: return "Collect " + resource;
        case OreKind.Smelter: return "Use: 2 Iron Ore + 1 Coal -> 1 Bar; alternate: 1 Iron Ore + 1 Crystal -> 1 Bar";
        case OreKind.Drill: return "Use: refuel (1 Coal); alternate: drop coal into hopper";
        case OreKind.Foundation: return "Foundation: required before placing a drill on this tile";
        case OreKind.WorkLight: return "Use: toggle light; alternate: refuel (1 Coal)";
        case OreKind.Gate: return "Use: switch minecart route";
        case OreKind.Cart: return "Use cart bars: 1 Bar -> 2 Refined Ore";
        default: return "Rail tile";
        }
    }
    public void Use(OrePlayer player, bool alternate = false) {
        if (!room) return;
        if(room.progression && !room.progression.CanUse(kind)) { room.Notify(room.progression.LockedReason(kind)); return; }
        switch (kind) {
        case OreKind.Iron: case OreKind.Coal: case OreKind.Crystal:
            if (player.held && player.held.kind == OreKind.Pickaxe && player.held.durability > 0 && cooldown <= 0 && remaining > 0) { if(!practiceVein)player.held.durability = Mathf.Max(0, player.held.durability - (room.rationSeconds>0?.5f:1)); Mine(true); }
            else if(player.held && player.held.durability<=0)room.Notify("Pickaxe is worn out. Smelt a Bar, then right-click the held pickaxe to repair it.");
            else if(!player.held || player.held.kind!=OreKind.Pickaxe) room.Notify("Grab the pickaxe first."); break;
        case OreKind.Pickaxe:
            if (alternate && room.Spend("IronBar",1)) { durability = 100; if(room.progression)room.progression.repairs++; room.Notify("Pickaxe repaired."); } break;
        case OreKind.Helmet: player.EquipHelmet(this); break;
        case OreKind.Lantern: case OreKind.WorkLight:
            if (alternate) { if(charge>=100)room.Notify("Fuel is full. Turn the light on and let Fuel fall before refueling.");
                else if (room.Spend("Coal",1)) { charge = Mathf.Min(100,charge+40); if(room.progression)room.progression.refuels++; room.Notify("Fuel restored."); } }
            else if (lamp) { lamp.enabled = !lamp.enabled; room.Notify(lamp.enabled?"Light on.":"Light off."); } break;
        case OreKind.Chunk: if(collected || reservedForDrill) return; collected=true; room.Add(resource,1); OreVisuals.Burst(transform.position, new Color(.8f,.6f,.3f)); Destroy(gameObject); break;
        case OreKind.Smelter:
            if (alternate) { if (room.Exchange("IronOre",1,"Crystal",1,"IronBar",1)) { smelterHeat=Mathf.Min(100,smelterHeat+25); OreVisuals.Burst(transform.position+Vector3.up,Color.cyan); } }
            else if (room.Exchange("IronOre",2,"Coal",1,"IronBar",1)) { smelterHeat=Mathf.Min(100,smelterHeat+25); OreVisuals.Burst(transform.position+Vector3.up,Color.yellow); } break;
        case OreKind.Drill:
            if(practiceMachine) {
                if(alternate && room.progression)room.progression.PracticeHopper();
                else room.Notify("Training drill: alternate-use drops loan Coal. It spends no real resources.");
                break;
            }
            if(alternate) room.FeedDrill(this);
            else if(charge<100 && room.Spend("Coal",1)) charge=Mathf.Min(100,charge+40);
            break;
        case OreKind.Foundation: room.Notify("Foundation supports "+supportedMachines+" drill. Place a drill on this tile with 1 / the build menu."); break;
        case OreKind.Gate: room.ToggleRoute(); break;
        case OreKind.Cart: room.Exchange("IronBar",1,null,0,"RefinedOre",2); break;
        }
        Refresh();
    }
    public bool Mine(bool shatter) {
        if(room && room.progression && !room.progression.CanUse(kind))return false;
        if (cooldown > 0 || remaining <= 0) return false;
        if(practiceVein) { if(room.progression)room.progression.CompleteSandbox(0); OreVisuals.Burst(transform.position,Color.green); return true; }
        cooldown = room.rationSeconds>0?.25f:.45f; remaining -= 1;
        OreVisuals.Burst(transform.position + Vector3.up*.5f, kind == OreKind.Crystal ? Color.cyan : Color.yellow);
        if (shatter) room.SpawnChunk(resource, transform.position + Vector3.up*.7f);
        else room.Add(resource,1);
        transform.localScale *= .95f;
        if (remaining <= 0) { room.nodes.Remove(this); Destroy(gameObject); } else Refresh();
        return true;
    }
    public bool AcceptCoalFragment(OreInteractable chunk) {
        if(!room || kind!=OreKind.Drill || charge>=100 || !chunk || chunk.kind!=OreKind.Chunk || chunk.resource!="Coal" || chunk.collected) return false;
        if(chunk.practiceFuel!=practiceMachine)return false;
        if(practiceMachine && room.progression)room.progression.CompleteSandbox(2);
        chunk.collected=true; charge=Mathf.Min(100,charge+40);
        OreVisuals.Burst(transform.position+new Vector3(.5f,1.08f,0),Color.yellow);
        room.Notify("Coal fragment consumed by hopper. Drill Fuel: "+charge.ToString("0"));
        Refresh(); Destroy(chunk.gameObject); return true;
    }
    void OnCollisionEnter(Collision collision) {
        if (!room || (kind != OreKind.Iron && kind != OreKind.Coal && kind != OreKind.Crystal)) return;
        if(room.progression && !room.progression.CanUse(kind))return;
        var tool = collision.collider.GetComponentInParent<OreInteractable>();
        if (tool && tool.kind == OreKind.Pickaxe && room.player && room.player.held == tool && collision.relativeVelocity.magnitude > .5f && tool.durability > 0) {
            if(!practiceVein)tool.durability = Mathf.Max(0,tool.durability-1); Mine(true);
        }
    }
    public void Refresh() {
        if(kind==OreKind.Chunk && chunkRenderer) {
            // SpawnChunk assigns the resource after Bind; refresh the tint on the next frame.
            string material=resource=="Crystal"?"Crystal":resource=="Coal"?"Coal":"IronOre";
            if(chunkRenderer.sharedMaterial.name!=material) chunkRenderer.sharedMaterial=OreVisuals.Material(material,Color.white);
        }
        if (!status) return;
        if(room && room.progression && !room.progression.CanUse(kind)) {
            status.text=kind+" - LOCKED\nComplete the current mine goal";return;
        }
        switch(kind) {
        case OreKind.Iron: case OreKind.Coal: case OreKind.Crystal: if(practiceVein) { status.text="Practice vein\nNo wear / no resources";break; } status.text = resource + "\nRemaining: " + remaining.ToString("0"); break;
        case OreKind.Lantern: case OreKind.WorkLight: status.text = "Fuel: " + charge.ToString("0"); break;
        case OreKind.Drill: status.text = "Drill Fuel: " + charge.ToString("0")+"\nDrill Output: "+drillOutput; break;
        case OreKind.Smelter: status.text="SMELTER\nHeat: "+smelterHeat.ToString("0"); break;
        case OreKind.Foundation: status.text="Foundation\nSupported Machines: "+supportedMachines; break;
        case OreKind.Pickaxe: status.text = "Durability: " + durability.ToString("0"); break;
        }
    }
}
}
