using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;
namespace Rishi.OreRoom.Editor {
public static class OreHubSetup {
    public const string PrefabPath="Assets/RishiOreRoom/Prefabs/RishiOreRoom_Hub.prefab";
    public const string ScenePath="Assets/RishiOreRoom/Scenes/RishiIntegratedHub.unity";
    public const string MineScenePath="Assets/RishiOreRoom/Scenes/RishiOreRoom.unity";
    const string ForestPath="Assets/Prefabs/Mason/MasonScencePrefab.prefab";
    const string FishingPath="Assets/Yuyangsong/Prefabs/YuyangsongScenePrefab.prefab";
    const string FarmPath="Assets/Scenes/SheepFarm.unity";

    [MenuItem("Tools/Rishi Ore Room/1 - Create Visible Hub Prefab")]
    public static void MakePrefab() {
        var existing=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if(existing && existing.transform.Find("Cave floor"))return;
        WriteNativePrefab();
    }
    // Unity authors the fileIDs. Do not assemble a prefab YAML file outside Unity.
    public static void WriteNativePrefab() {
        OreRoomSetup.Ensure();Directory.CreateDirectory("Assets/RishiOreRoom/Prefabs");
        const string temporary="Assets/RishiOreRoom/Prefabs/OreNativeTemporary.prefab";
        if(File.Exists(temporary))AssetDatabase.DeleteAsset(temporary);
        var preview=EditorSceneManager.NewPreviewScene();
        try {
            var root=new GameObject("RishiOreRoom_Hub");SceneManager.MoveGameObjectToScene(root,preview);
            var room=root.AddComponent<OreRoom>();room.integratedRoom=true;room.spawnTestPlayer=false;room.useDesktopControls=true;
            room.CreateLayout();root.AddComponent<OreProgression>();
            PrefabUtility.SaveAsPrefabAsset(root,temporary,out bool success);
            if(!success)throw new InvalidOperationException("Unity could not save the native ore prefab.");
            // Keep the destination .meta/GUID, replacing only the malformed contents.
            File.Copy(temporary,PrefabPath,true);
            AssetDatabase.ImportAsset(PrefabPath,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            var saved=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if(!saved || !saved.transform.Find("Cave floor"))throw new InvalidOperationException("Native ore prefab did not import. See the first earlier Console error.");
        } finally {
            EditorSceneManager.ClosePreviewScene(preview);
            if(File.Exists(temporary))AssetDatabase.DeleteAsset(temporary);
        }
    }
    [MenuItem("Tools/Rishi Ore Room/2 - Open Integrated Desktop Hub")]
    public static void Desktop() {
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene(ScenePath);
    }
    [MenuItem("Tools/Rishi Ore Room/3 - Open Integrated XR Hub")]
    public static void VR() {
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        Generate(false);
    }
    [MenuItem("Tools/Rishi Ore Room/6 - Rebuild Integrated Desktop Scene (Optional)")]
    public static void RebuildDesktop() {
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        Generate(true);
    }
    public static void CheckDependencies() {
        foreach(var path in new[]{"Assets/Scenes/Hub.unity",FarmPath,ForestPath,FishingPath})
            if(!File.Exists(path))throw new FileNotFoundException("The integrated room needs the group's source asset: "+path);
    }
    // Called once by automatic repair; the resulting normal scenes are saved to disk.
    public static void Generate(bool desktop) {
        CheckDependencies();MakePrefab();
        var target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var hubRoot=CopyRoomScene("Assets/Scenes/Hub.unity",target,"Hub layout copy",false);
        hubRoot.transform.position=new Vector3(-80,0,0);
        var mine=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),target);
        mine.transform.position=new Vector3(-135,0,0);
        var forest=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ForestPath),target);
        forest.transform.position=new Vector3(65,0,0);
        foreach(var e in forest.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true)) {
            e.enabled=false;foreach(var input in e.GetComponents<UnityEngine.EventSystems.BaseInputModule>())input.enabled=false;
        }
        var fix=new GameObject("Forest placement adapter (Rishi)").AddComponent<OreForestPlacement>();
        fix.forestGrid=forest.GetComponentInChildren<GridManager>();fix.forestRoot=forest;
        var fishing=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(FishingPath),target);
        var farm=CopyRoomScene(FarmPath,target,"Sheep farm source copy",true);
        var integration=new GameObject("Rishi hub connections").AddComponent<OreHubIntegration>();
        integration.room=mine.GetComponent<OreRoom>();integration.useDesktopControls=desktop;
        integration.visitRooms=new[]{mine,forest,fishing,farm};integration.initialDestination=0;
        if(!desktop) {
            foreach(var camera in hubRoot.GetComponentsInChildren<Camera>()) {
                camera.enabled=false;var listener=camera.GetComponent<AudioListener>();if(listener)listener.enabled=false;
            }
            var xrPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab");
            if(!xrPrefab)throw new InvalidOperationException("Import XRI Starter Assets or assign the team's existing XR rig before making the XR scene.");
            var rig=(GameObject)PrefabUtility.InstantiatePrefab(xrPrefab,target);rig.transform.position=integration.destinations[0];
            integration.sharedRig=rig.transform;integration.sharedCamera=rig.GetComponentInChildren<Camera>();
        }
        Save(target,ScenePath);
        // The familiar mine scene is also connected: opening it can no longer strand the player.
        integration.initialDestination=1;Save(target,MineScenePath,true);
        integration.initialDestination=0;Save(target,ScenePath);
        Selection.activeGameObject=integration.gameObject;
        Debug.Log("Ore room and connected hub saved. Open either scene in Assets/RishiOreRoom/Scenes and press Play. Tab or the Return to Hub button preserves inventory. No setup tools are required on later runs.");
    }
    static void Save(Scene scene,string path,bool copy=false) {
        if(!EditorSceneManager.SaveScene(scene,path,copy))throw new IOException("Unity could not save "+path);
    }
    static GameObject CopyRoomScene(string path,Scene target,string title,bool farm) {
        var source=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        try {
            SceneManager.SetActiveScene(source);
            var originals=source.GetRootGameObjects();var container=new GameObject(title);
            foreach(var original in originals) {
                if(farm) {
                    if(original.GetComponentInChildren<XROrigin>(true) || original.GetComponentInChildren<Camera>(true) || original.GetComponent<UnityEngine.EventSystems.EventSystem>())continue;
                    var light=original.GetComponent<Light>();if(light && light.type==LightType.Directional)continue;
                }
                original.transform.SetParent(container.transform,true);
            }
            // Clone once so references between the included roots are remapped together.
            var clone=Object.Instantiate(container);clone.name=title;SceneManager.MoveGameObjectToScene(clone,target);
            return clone;
        } finally {
            SceneManager.SetActiveScene(target);EditorSceneManager.CloseScene(source,true);
        }
    }
    [MenuItem("Tools/Rishi Ore Room/4 - Add Ore Room To CURRENT Hub")]
    static void AddToCurrent() {
        MakePrefab();
        var mine=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        Undo.RegisterCreatedObjectUndo(mine,"Add ore room");mine.transform.position=new Vector3(-55,0,0);
        var link=Object.FindAnyObjectByType<OreHubIntegration>();
        if(!link) { var connection=new GameObject("Rishi hub connections");Undo.RegisterCreatedObjectUndo(connection,"Add ore connections");link=connection.AddComponent<OreHubIntegration>(); }
        link.room=mine.GetComponent<OreRoom>();link.hubCenter=Vector3.zero;
        link.destinations[0]=new Vector3(0,.2f,-8);link.destinations[1]=new Vector3(-55,.2f,-3.7f);
        link.sharedCamera=Camera.main;
        var origin=link.sharedCamera?link.sharedCamera.GetComponentInParent<XROrigin>():null;
        link.useDesktopControls=!origin;if(origin)link.sharedRig=origin.transform;
        Selection.activeGameObject=link.gameObject;EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Ore travel connections added to the current hub. Inspect destinations and shared player, then save the scene. Existing hub UI and building behavior are left to the original hub scripts.");
    }
}
}
