using UnityEditor;
using UnityEngine;
namespace Rishi.OreRoom.Editor {
// Compatibility with the previous demo tools. Never open/rebuild a scene on import or Play.
public static class OreNativeInstaller {
    public static void Schedule() { }
    public static bool Ready() {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(OreHubSetup.PrefabPath);
        return prefab && prefab.GetComponent<OreRoom>() && prefab.transform.Find("Cave floor");
    }
    [MenuItem("Tools/Rishi Ore Room/7 - Retry Native Repair (Only If Repair Failed)")]
    static void Retry() { OreSharedPrefabExport.Prepare(); }
}
}
