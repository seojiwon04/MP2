using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Rishi.OreRoom;
namespace Rishi.OreRoom.Editor {
public static class OreRoomSetup {
    const string Root="Assets/RishiOreRoom/Resources/RishiOre";
    static readonly string[] Prefabs={"Pickaxe","Helmet","Lantern","Iron","Coal","Crystal","Chunk","Smelter","Drill","WorkLight","Rail","Minecart","Gate","Foundation"};
    [InitializeOnLoadMethod] static void OnLoad() { EditorApplication.delayCall+=Ensure; }
    static void Ensure() {
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        Directory.CreateDirectory(Root+"/Materials"); Directory.CreateDirectory(Root+"/Prefabs"); AssetDatabase.Refresh();
        Mat("Stone",new Color(.18f,.2f,.23f)); Mat("Ground",new Color(.25f,.23f,.21f)); Mat("Wood",new Color(.26f,.13f,.055f));
        Mat("Metal",new Color(.25f,.29f,.33f)); Mat("Gold",new Color(1,.65f,.12f)); Mat("IronOre",new Color(.5f,.37f,.27f));
        Mat("Coal",new Color(.07f,.08f,.09f)); Mat("Crystal",new Color(.12f,.8f,.95f)); Mat("Fire",new Color(1,.3f,.03f));
        Mat("Preview",new Color(.25f,.8f,.55f)); Mat("Ray",Color.cyan); Mat("Spark",Color.yellow);
        foreach(var name in Prefabs) if(!AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+name+".prefab")) Create(name);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
    }
    [MenuItem("Tools/Rishi Ore Room/Generate - Open Test Room")]
    static void Open() {
        Ensure(); if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/RishiOreRoom/Scenes/RishiOreRoom.unity");
        Debug.Log("Ore room ready. Press Play. All generated assets stay inside Assets/RishiOreRoom.");
    }
    [MenuItem("Tools/Rishi Ore Room/Validate Room Assets")]
    static void Validate() {
        Ensure(); int count=0;
        foreach(var name in Prefabs) {
            var p=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+name+".prefab");
            if(!p || !p.GetComponent<OreInteractable>()) Debug.LogError("Invalid prefab "+name); else count++;
        }
        var planner=new OreRailPlanner();
        foreach(var c in new[]{new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0),new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(2,1)}) planner.rails.Add(c);
        var initial=planner.Find(Vector2Int.zero,new Vector2Int(2,0)); planner.blocked.Add(new Vector2Int(1,0));
        var detour=planner.Find(Vector2Int.zero,new Vector2Int(2,0));
        Debug.Assert(initial!=null && initial.Count==3 && detour!=null && detour.Count==5,"A* detour regression");
        planner.blocked.Add(new Vector2Int(0,1)); Debug.Assert(planner.Find(Vector2Int.zero,new Vector2Int(2,0))==null,"A* unreachable regression");
        Debug.Log("Ore room validation: "+count+" / 14 prefabs; A* direct, detour, disconnected cases checked.");
    }
    static Material Mat(string name,Color color) {
        string path=Root+"/Materials/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(path); if(m) return m;
        m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
        if(name=="Fire" || name=="Crystal" || name=="Ray" || name=="Spark") { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor",color*1.2f); }
        AssetDatabase.CreateAsset(m,path); return m;
    }
    static Material M(string name) { return AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+name+".mat"); }
    static GameObject Part(Transform p,string name,PrimitiveType t,Vector3 pos,Vector3 scale,string material,bool solid=true) { return OreVisuals.Part(p,name,t,pos,scale,M(material),solid); }
    static void Create(string name) {
        var g=new GameObject(name); var item=g.AddComponent<OreInteractable>(); var t=g.transform;
        item.kind=name=="Minecart"?OreKind.Cart:(OreKind)System.Enum.Parse(typeof(OreKind),name);
        item.tutorial=name; item.grabbable=name=="Pickaxe" || name=="Helmet" || name=="Lantern";
        switch(name) {
        case "Pickaxe":
            Part(t,"Wooden handle",PrimitiveType.Cylinder,new Vector3(0,0,0),new Vector3(.05f,.45f,.05f),"Wood");
            Part(t,"Iron pick head",PrimitiveType.Cube,new Vector3(0,.35f,0),new Vector3(.65f,.09f,.1f),"Metal");
            item.status=OreVisuals.Label(t,"Durability: 100",new Vector3(0,.53f,0),.035f); break;
        case "Helmet":
            Part(t,"Helmet shell",PrimitiveType.Sphere,Vector3.zero,new Vector3(.4f,.25f,.4f),"Gold");
            Part(t,"Helmet rim",PrimitiveType.Cylinder,new Vector3(0,-.07f,0),new Vector3(.46f,.018f,.46f),"Gold");
            Part(t,"Headlamp",PrimitiveType.Sphere,new Vector3(0,0,.21f),Vector3.one*.11f,"Fire",false);
            var headlamp=new GameObject("Headlamp light"); headlamp.transform.SetParent(t,false); headlamp.transform.localPosition=new Vector3(0,0,.28f);
            item.lamp=headlamp.AddComponent<Light>(); item.lamp.type=LightType.Spot; item.lamp.range=10; item.lamp.intensity=2; item.lamp.spotAngle=60; break;
        case "Lantern": case "WorkLight":
            Part(t,"Lantern base",PrimitiveType.Cylinder,new Vector3(0,.12f,0),new Vector3(.3f,.07f,.3f),"Metal");
            Part(t,"Lantern flame",PrimitiveType.Sphere,new Vector3(0,.32f,0),new Vector3(.17f,.27f,.17f),"Fire",false);
            Part(t,"Lantern top",PrimitiveType.Cylinder,new Vector3(0,.51f,0),new Vector3(.32f,.04f,.32f),"Metal");
            for(int s=-1;s<=1;s+=2) Part(t,"Cage",PrimitiveType.Cube,new Vector3(s*.13f,.33f,0),new Vector3(.025f,.35f,.025f),"Metal");
            var light=new GameObject("Lantern light"); light.transform.SetParent(t,false); light.transform.localPosition=Vector3.up*.33f;
            item.lamp=light.AddComponent<Light>(); item.lamp.type=LightType.Point; item.lamp.color=new Color(1,.65f,.25f); item.lamp.range=8; item.lamp.intensity=2.2f;
            item.status=OreVisuals.Label(t,"Fuel: 100",new Vector3(0,.7f,0),.04f); break;
        case "Iron": case "Coal": case "Crystal":
            item.resource=name=="Iron"?"IronOre":name; item.remaining=6;
            Part(t,"Ore rock",PrimitiveType.Sphere,new Vector3(0,.35f,0),new Vector3(.8f,.7f,.8f),"Stone");
            for(int i=0;i<5;i++) { float a=i*1.2566f; var crystal=Part(t,"Mineral seam",PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.25f,.53f,Mathf.Sin(a)*.25f),new Vector3(.18f,.4f,.2f),item.resource); crystal.transform.localRotation=Quaternion.Euler(0,i*37,25); }
            item.status=OreVisuals.Label(t,item.resource+"\nRemaining: 6",Vector3.up*1.05f,.04f); break;
        case "Chunk":
            Part(t,"Ore fragment",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.18f,"IronOre"); item.status=OreVisuals.Label(t,"Collect ore",Vector3.up*.25f,.025f); break;
        case "Smelter":
            Part(t,"Furnace",PrimitiveType.Cylinder,new Vector3(0,.5f,0),new Vector3(1.1f,.5f,1.1f),"Stone");
            Part(t,"Furnace mouth",PrimitiveType.Cube,new Vector3(0,.45f,-.56f),new Vector3(.5f,.45f,.025f),"Fire");
            Part(t,"Chimney",PrimitiveType.Cylinder,new Vector3(0,1.3f,0),new Vector3(.4f,.5f,.4f),"Metal");
            OreVisuals.Label(t,"SMELTER\nIron + Coal / Crystal",new Vector3(0,2,0),.06f); break;
        case "Drill":
            item.charge=40; Part(t,"Drill body",PrimitiveType.Cube,new Vector3(0,.4f,0),new Vector3(.6f,.8f,.6f),"Metal");
            Part(t,"Drill bit",PrimitiveType.Cylinder,new Vector3(0,.15f,0),new Vector3(.14f,.35f,.14f),"Gold");
            item.status=OreVisuals.Label(t,"Drill fuel: 40",Vector3.up*1.1f,.04f); break;
        case "Foundation":
            Part(t,"Metal foundation plate",PrimitiveType.Cube,new Vector3(0,.05f,0),new Vector3(.92f,.1f,.92f),"Metal");
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2)
                Part(t,"Foundation bolt",PrimitiveType.Cylinder,new Vector3(x*.34f,.11f,z*.34f),new Vector3(.07f,.015f,.07f),"Gold",false);
            item.status=OreVisuals.Label(t,"Foundation\nSupported Machines: 0",new Vector3(0,.35f,0),.04f); break;
        case "Rail":
            // Each grid tile has crossing rails, so both axes are connectable.
            for(int s=-1;s<=1;s+=2) {
                Part(t,"North-south rail",PrimitiveType.Cube,new Vector3(s*.28f,.075f,0),new Vector3(.04f,.06f,1.02f),"Metal",false);
                Part(t,"East-west rail",PrimitiveType.Cube,new Vector3(0,.075f,s*.28f),new Vector3(1.02f,.06f,.04f),"Metal",false);
            }
            for(int z=-1;z<=1;z++) Part(t,"Rail sleeper",PrimitiveType.Cube,new Vector3(0,.025f,z*.35f),new Vector3(.85f,.05f,.12f),"Wood",false); break;
        case "Minecart":
            Part(t,"Cart chassis",PrimitiveType.Cube,new Vector3(0,.25f,0),new Vector3(.85f,.12f,1),"Metal");
            for(int s=-1;s<=1;s+=2) {
                Part(t,"Cart side",PrimitiveType.Cube,new Vector3(s*.4f,.5f,0),new Vector3(.08f,.5f,1),"Metal");
                Part(t,"Cart end",PrimitiveType.Cube,new Vector3(0,.5f,s*.46f),new Vector3(.85f,.5f,.08f),"Metal");
            }
            var cart=g.AddComponent<OreMinecart>(); cart.wheels=new Transform[4]; int index=0;
            for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2) { var wheel=Part(t,"Cart wheel",PrimitiveType.Cylinder,new Vector3(x*.45f,.08f,z*.32f),new Vector3(.24f,.055f,.24f),"Coal",false); wheel.transform.localRotation=Quaternion.Euler(0,0,90); cart.wheels[index++]=wheel.transform; }
            Part(t,"Cart load",PrimitiveType.Sphere,new Vector3(0,.6f,0),new Vector3(.55f,.35f,.65f),"IronOre",false);
            cart.status=OreVisuals.Label(t,"Minecart",Vector3.up*1.25f,.05f); break;
        case "Gate":
            Part(t,"Junction stand",PrimitiveType.Cube,new Vector3(0,.45f,0),new Vector3(.6f,.9f,.5f),"Wood");
            Part(t,"Junction lever",PrimitiveType.Cylinder,new Vector3(0,1,0),new Vector3(.06f,.3f,.06f),"Gold");
            OreVisuals.Label(t,"JUNCTION\nSwitch route",Vector3.up*1.6f,.05f); break;
        }
        if(item.grabbable || name=="Chunk" || name=="Minecart") {
            var rb=g.AddComponent<Rigidbody>(); rb.mass=name=="Minecart"?30:.5f;
            rb.isKinematic=name=="Minecart"; rb.useGravity=name!="Minecart";
            if(item.grabbable) rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        }
        PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/"+name+".prefab"); Object.DestroyImmediate(g);
    }
}
}
