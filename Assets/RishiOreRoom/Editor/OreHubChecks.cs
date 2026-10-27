using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Rishi.OreRoom.Editor {
public static class OreHubChecks {
    [MenuItem("Tools/Rishi Ore Room/5 - Validate Hub Prefab")]
    public static void Validate() {
        var p=AssetDatabase.LoadAssetAtPath<GameObject>(OreHubSetup.PrefabPath);
        if(!p)throw new Exception("Room prefab is missing. Run menu item 1.");
        var room=p.GetComponent<OreRoom>();
        Check(room && room.integratedRoom && !room.spawnTestPlayer,"Room is configured to reuse the hub player");
        Check(p.GetComponent<OreProgression>(),"Mastery controller exists");
        Check(p.transform.Find("Cave floor"),"Cave is visible in Edit mode");
        Check(p.GetComponentsInChildren<Camera>(true).Length==0,"No camera in room prefab");
        Check(p.GetComponentsInChildren<AudioListener>(true).Length==0,"No AudioListener in room prefab");
        Check(p.GetComponentsInChildren<EventSystem>(true).Length==0,"No EventSystem in room prefab");
        Check(p.GetComponentsInChildren<OrePlayer>(true).Length==0,"No player rig in room prefab");
        foreach(var light in p.GetComponentsInChildren<Light>(true))Check(light.type!=LightType.Directional,"No directional light");
        foreach(var t in p.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"No missing script on "+t.name);
        var planner=new OreRailPlanner();
        foreach(var item in p.GetComponentsInChildren<OreInteractable>(true))if(item.kind==OreKind.Rail)planner.rails.Add(OreRoom.GridCell(p.transform.InverseTransformPoint(item.transform.position)));
        Check(planner.Find(room.entrance,room.exit)!=null,"Initial cart route is connected");
        planner.blocked.Add(new Vector2Int(0,2));Check(planner.Find(room.entrance,room.exit)==null,"Incomplete bypass blocks alternate route");
        planner.rails.Add(new Vector2Int(0,3));Check(planner.Find(room.entrance,room.exit)!=null,"Player rail reconnects the bypass");
        Check(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/RishiOreRoom/Scenes/RishiIntegratedHub.unity"),"Saved integrated scene exists");
        Debug.Log("Hub prefab checks passed. Double-click Scenes/RishiIntegratedHub.unity and press Play. Setup tools are optional; no regeneration is required between plays or builds.");
    }
    static void Check(bool condition,string message) { if(!condition)throw new Exception("FAILED: "+message); }
}
}
