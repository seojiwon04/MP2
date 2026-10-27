using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Rishi.OreRoom.Editor {
// One-time migration of the earlier malformed prefab and scenes. Player builds never run this.
public static class OreNativeInstaller {
    const string Version="ore-native-connected-v3";
    const string Stamp="Assets/RishiOreRoom/Editor/OreNativeInstallation.txt";
    static bool queued,failed,waitingPrinted;
    static double nextCheck;
    [InitializeOnLoadMethod] static void Load() {
        Schedule();EditorApplication.playModeStateChanged+=OnPlayModeChanged;
    }
    public static void Schedule() {
        if(queued || failed)return;
        queued=true;EditorApplication.update+=Tick;
    }
    static void Stop() { queued=false;EditorApplication.update-=Tick; }
    public static bool Ready() {
        if(!File.Exists(Stamp) || File.ReadAllText(Stamp).Trim()!=Version)return false;
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(OreHubSetup.PrefabPath);
        return prefab && prefab.GetComponent<OreRoom>() && prefab.GetComponent<OreProgression>() && prefab.transform.Find("Cave floor") &&
            File.Exists(OreHubSetup.ScenePath) && File.Exists(OreHubSetup.MineScenePath);
    }
    static void Tick() {
        if(EditorApplication.timeSinceStartup<nextCheck)return;
        nextCheck=EditorApplication.timeSinceStartup+1;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(Ready()) { Stop();return; }
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty) {
            if(!waitingPrinted) {
                Debug.LogWarning("Ore native repair is waiting for unsaved scene changes. Save your current scene (Ctrl+S), then wait. It will automatically save a connected mine and hub; no Tools menu is required.");waitingPrinted=true;
            }
            return;
        }
        Stop();
        try {
            OreHubSetup.CheckDependencies();
            Backup(OreHubSetup.PrefabPath);Backup(OreHubSetup.ScenePath);Backup(OreHubSetup.MineScenePath);
            OreHubSetup.WriteNativePrefab();
            OreHubSetup.Generate(true);
            OreHubChecks.Validate();
            File.WriteAllText(Stamp,Version+Environment.NewLine);AssetDatabase.ImportAsset(Stamp);
            Debug.Log("ORE REPAIR COMPLETE: Unity saved the room prefab and both connected scenes with native references. Open either scene and press Play. Tab / F2 returns to the Hub; ore stays in shared inventory. Commit these generated assets and their .meta files along with your scripts.");
        } catch(Exception exception) {
            failed=true;Debug.LogError("Ore native repair stopped: "+exception.Message+"\nFix the first earlier Console error, then use optional Tools > Rishi Ore Room > 7 - Retry Native Repair.");Debug.LogException(exception);
        }
    }
    static void Backup(string path) {
        if(!File.Exists(path))return;
        const string folder="Assets/RishiOreRoom/Editor/RepairBackup";Directory.CreateDirectory(folder);
        string destination=folder+"/"+Path.GetFileName(path)+".before-native.txt";
        if(!File.Exists(destination))File.Copy(path,destination);
    }
    static void OnPlayModeChanged(PlayModeStateChange state) {
        if(state!=PlayModeStateChange.ExitingEditMode || Ready())return;
        EditorApplication.isPlaying=false;Schedule();
        Debug.LogWarning("Wait for ORE REPAIR COMPLETE before Play. If repair is waiting for unsaved work, save the current scene with Ctrl+S. The repair runs once, then scenes open and build normally.");
    }
    [MenuItem("Tools/Rishi Ore Room/7 - Retry Native Repair (Only If Repair Failed)")]
    static void Retry() { failed=false;waitingPrinted=false;Schedule(); }
}
public class OreNativeImportHook : AssetPostprocessor {
    static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] movedFrom,bool didDomainReload) {
        OreNativeInstaller.Schedule();
    }
}
public class OreNativeBuildCheck : IPreprocessBuildWithReport {
    public int callbackOrder { get { return -1000; } }
    public void OnPreprocessBuild(BuildReport report) {
        bool includesOre=false;
        foreach(var scene in EditorBuildSettings.scenes)if(scene.enabled && (scene.path==OreHubSetup.ScenePath || scene.path==OreHubSetup.MineScenePath))includesOre=true;
        if(!includesOre)return;
        if(!OreNativeInstaller.Ready())throw new BuildFailedException("Let the automatic ore repair complete before building. Save any unsaved scene, wait for ORE REPAIR COMPLETE, then add RishiIntegratedHub.unity to the Build Profile Scene List.");
    }
}
}
