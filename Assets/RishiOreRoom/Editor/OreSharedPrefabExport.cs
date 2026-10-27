using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;
namespace Rishi.OreRoom.Editor {
public static class OreSharedPrefabExport {
    public const string Path="Assets/RishiOreRoom/Prefabs/RishiOreRoom_Hub.prefab";
    public const string DesktopScene="Assets/RishiOreRoom/Scenes/RishiOreRoom_Desktop.unity";
    public const string XRScene="Assets/RishiOreRoom/Scenes/RishiOreRoom_SharedXRTest.unity";
    const string RigPath="Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    [MenuItem("Tools/Rishi Ore Room/Prepare TEAM Prefab (One Time)",false,0)]
    public static void Prepare() {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before saving the team prefab.");
        if(PrefabStageUtility.GetCurrentPrefabStage()!=null)throw new InvalidOperationException("Exit Prefab Mode before preparing the team prefab.");
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(Path);
        if(!source || !source.transform.Find("Cave floor"))throw new InvalidOperationException("The full saved room prefab is missing. Restore Assets/RishiOreRoom/Prefabs/RishiOreRoom_Hub.prefab from your current main branch first.");
        var root=PrefabUtility.LoadPrefabContents(Path);
        try {
            CheckNoSceneSingletons(root);
            var room=root.GetComponent<OreRoom>();
            if(!room)throw new InvalidOperationException("Expected OreRoom on the prefab root.");
            room.spawnTestPlayer=false;room.integratedRoom=true;room.player=null;
            room.useDesktopControls=true;
            if(!root.GetComponent<OreProgression>())root.AddComponent<OreProgression>();
            var adapter=root.GetComponent<OreSharedRigAdapter>();if(!adapter)adapter=root.AddComponent<OreSharedRigAdapter>();
            adapter.sharedOrigin=null;adapter.sharedCamera=null;adapter.rightController=null;adapter.leftController=null;
            var entry=root.transform.Find("Ore Entry Point");
            if(!entry) { entry=new GameObject("Ore Entry Point").transform;entry.SetParent(root.transform,false); }
            entry.localPosition=room.spawn;entry.localRotation=Quaternion.identity;
            var floor=root.transform.Find("Cave floor");
            var area=floor.GetComponent<TeleportationArea>();if(!area)area=floor.gameObject.AddComponent<TeleportationArea>();
            area.colliders.Clear();area.colliders.Add(floor.GetComponent<Collider>());
            area.interactionLayers=InteractionLayerMask.GetMask("Default","Teleport");
            area.teleportationProvider=null;
            foreach(var light in root.GetComponentsInChildren<Light>(true)) {
                if(light.name=="Warm cave lamp") { light.intensity=.04f;light.range=3; }
                if(light.name=="Equipment bench glow") { light.intensity=.18f;light.range=2.4f; }
            }
            PrefabUtility.SaveAsPrefabAsset(root,Path,out bool success);
            if(!success)throw new IOException("Unity could not save the team prefab.");
        } finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
        Validate();
        Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(Path);
        EditorGUIUtility.PingObject(Selection.activeObject);
        Debug.Log("TEAM ORE PREFAB READY: "+Path+". Commit this prefab, its existing .meta, and the updated RishiOreRoom scripts. No hub scene was changed. This is a one-time export; ordinary Play/build needs no tool.");
    }
    [MenuItem("Tools/Rishi Ore Room/Check TEAM Prefab",false,1)]
    public static void Validate() {
        var root=AssetDatabase.LoadAssetAtPath<GameObject>(Path);
        if(!root)throw new InvalidOperationException("Team prefab missing.");
        CheckNoSceneSingletons(root);
        var room=root.GetComponent<OreRoom>();
        Require(room && !room.spawnTestPlayer && room.integratedRoom,"No test player may be spawned by the prefab.");
        Require(root.GetComponent<OreSharedRigAdapter>(),"Prepare the team prefab first: shared-rig adapter missing.");
        Require(root.GetComponent<OreProgression>(),"Mine progression missing.");
        Require(root.transform.Find("Ore Entry Point"),"Entry point missing.");
        var floor=root.transform.Find("Cave floor");Require(floor,"Cave floor missing.");
        Require(floor.GetComponent<TeleportationArea>(),"Teleportable cave floor missing.");
        int equipment=0,deposits=0;
        foreach(var item in root.GetComponentsInChildren<OreInteractable>(true)) {
            if(item.grabbable)equipment++;
            if(item.kind==OreKind.Iron || item.kind==OreKind.Coal || item.kind==OreKind.Crystal)deposits++;
        }
        Require(equipment>=3 && deposits>=9,"The full saved mine must include equipment and mineral deposits.");
        Require(root.GetComponentInChildren<OreMinecart>(true),"Minecart missing.");
        Debug.Log("TEAM ORE PREFAB CHECKS PASSED: saved cave, equipment, ore, cart, progression and shared-rig adapter; no rig/camera/listener/manager/EventSystem/directional light.");
    }
    static void Require(bool ok,string error) { if(!ok)throw new InvalidOperationException(error); }
    static void CheckNoSceneSingletons(GameObject root) {
        Require(root.GetComponentsInChildren<XROrigin>(true).Length==0,"Remove XR Origin from ROOM prefab.");
        Require(root.GetComponentsInChildren<Camera>(true).Length==0,"Remove Camera from ROOM prefab.");
        Require(root.GetComponentsInChildren<AudioListener>(true).Length==0,"Remove AudioListener from ROOM prefab.");
        Require(root.GetComponentsInChildren<EventSystem>(true).Length==0,"Remove EventSystem from ROOM prefab.");
        Require(root.GetComponentsInChildren<XRInteractionManager>(true).Length==0,"Remove XRInteractionManager from ROOM prefab.");
        Require(root.GetComponentsInChildren<XRBaseInteractor>(true).Length==0,"Remove controller/interactor objects from ROOM prefab.");
        Require(root.GetComponentsInChildren<OrePlayer>(true).Length==0,"Remove test-player driver from saved ROOM prefab.");
        Require(root.GetComponentsInChildren<OreHubIntegration>(true).Length==0,"Do not export the connected demonstration hub as a room.");
        foreach(var light in root.GetComponentsInChildren<Light>(true))Require(light.type!=LightType.Directional,"Remove Directional Light from ROOM prefab.");
        foreach(var t in root.GetComponentsInChildren<Transform>(true))Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"Missing script on "+t.name);
    }
    [MenuItem("Tools/Rishi Ore Room/Create Separate DESKTOP Test Scene",false,2)]
    static void CreateDesktop() { CreateTest(false); }
    [MenuItem("Tools/Rishi Ore Room/Create Separate SHARED XR Test Scene",false,3)]
    static void CreateXR() { CreateTest(true); }
    static void CreateTest(bool xr) {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        Prepare();
        GameObject rigPrefab=xr?AssetDatabase.LoadAssetAtPath<GameObject>(RigPath):null;
        if(xr && !rigPrefab)throw new InvalidOperationException("Import XRI Starter Assets 3.5.1 before creating the separate shared-rig test scene.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var mine=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Path),scene);
        var room=mine.GetComponent<OreRoom>();
        // Offset and rotate deliberately to expose world-origin assumptions during testing.
        mine.transform.SetPositionAndRotation(new Vector3(25,0,0),Quaternion.Euler(0,30,0));
        if(xr) {
            var rig=(GameObject)PrefabUtility.InstantiatePrefab(rigPrefab,scene);
            rig.transform.SetPositionAndRotation(room.World(room.spawn),mine.transform.rotation);
            if(!UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>())new GameObject("Shared XR Interaction Manager - test scene only").AddComponent<XRInteractionManager>();
        } else {
            var preview=new GameObject("Desktop preview setup - DO NOT include in room prefab").AddComponent<OreStandalonePreview>();preview.room=room;
        }
        Directory.CreateDirectory("Assets/RishiOreRoom/Scenes");
        string path=xr?XRScene:DesktopScene;
        if(!EditorSceneManager.SaveScene(scene,path))throw new IOException("Could not save test scene.");
        Selection.activeGameObject=mine;
        Debug.Log("Saved "+path+". This separate test scene uses the TEAM prefab. Open it normally and press Play on later runs. Cross-room supply goals require the actual other rooms and their resource accounts.");
    }
}
}
