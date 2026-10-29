using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Rishi.OreRoom.Editor {
// Save the room instance with Unity's own serializer; never hand-author prefab/scene fileIDs.
[InitializeOnLoad]
public static class OreMainHubInstaller {
    const string HubPath="Assets/Scenes/Hub.unity";
    static bool preparing,failed,waitingPrinted;
    static double nextCheck;
    static OreMainHubInstaller() {
        EditorApplication.update+=Tick;
    }
    static void Tick() {
        if(failed || preparing || EditorApplication.timeSinceStartup<nextCheck)return;
        nextCheck=EditorApplication.timeSinceStartup+1;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(PrefabStageUtility.GetCurrentPrefabStage()!=null)return;
        for(int i=0;i<SceneManager.sceneCount;i++) {
            var scene=SceneManager.GetSceneAt(i);
            if(!scene.isLoaded || scene.path!=HubPath)continue;
            HubInventory inventory=null;
            foreach(var root in scene.GetRootGameObjects()) {
                inventory=root.GetComponentInChildren<HubInventory>(true);if(inventory)break;
            }
            if(!inventory || !inventory.oreRoomPrefab)return;
            if(inventory.oreRoom && inventory.oreRoom.GetComponent<OreSharedRigAdapter>() && inventory.oreRoom.transform.Find(OreHubConnection.EntranceName))return;
            if(scene.isDirty) {
                if(!waitingPrinted)Debug.Log("Ore hub connection is waiting for your unsaved Hub changes. Save the Hub with Ctrl+S; the one-time connection will then save automatically.");
                waitingPrinted=true;return;
            }
            preparing=true;
            try {
                if(!inventory.oreRoom)foreach(var root in scene.GetRootGameObjects()) {
                    inventory.oreRoom=root.GetComponentInChildren<OreRoom>(true);if(inventory.oreRoom)break;
                }
                if(!inventory.oreRoom) {
                    var source=inventory.oreRoomPrefab.GetComponent<OreRoom>();
                    if(!source || source.spawnTestPlayer || !source.transform.Find("Cave floor"))
                        throw new System.InvalidOperationException("The saved ore prefab must contain the full cave and have Spawn Test Player OFF.");
                    var mine=(GameObject)PrefabUtility.InstantiatePrefab(inventory.oreRoomPrefab,scene);
                    mine.transform.SetPositionAndRotation(inventory.oreRoomPosition,Quaternion.Euler(inventory.oreRoomEulerAngles));
                    inventory.oreRoom=mine.GetComponent<OreRoom>();
                }
                OreHubConnection.Prepare(inventory.oreRoom);
                EditorUtility.SetDirty(inventory);
                foreach(var component in inventory.oreRoom.GetComponentsInChildren<Component>(true)) {
                    if(component && PrefabUtility.IsPartOfPrefabInstance(component))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))throw new System.IO.IOException("Unity could not save the Hub scene.");
                Debug.Log("ORE HUB CONNECTED: the saved Hub now includes the mine on the west side, its open entrance and teleportable path, and bindings to the team's existing XR Origin. No extra rig or hub UI was added.");
            } catch(System.Exception exception) {
                failed=true;Debug.LogException(exception);
                Debug.LogError("Ore hub connection could not be saved. The runtime fallback is still available; fix the first Console error and reopen Unity to retry.");
            } finally { preparing=false; }
            return;
        }
    }
}
}
