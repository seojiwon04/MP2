using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace Rishi.OreRoom {
// Lives on the room. It only reads the team's existing rig; it never creates or edits one.
[DisallowMultipleComponent]
public class OreSharedRigAdapter : MonoBehaviour {
    [Tooltip("Optional scene-instance override. Leave empty in the prefab for automatic binding.")]
    public XROrigin sharedOrigin;
    public Camera sharedCamera;
    public Transform rightController, leftController;
    public int Iron { get { var mine=GetComponent<OreRoom>();return mine?mine.SharedInventory.Iron:0; } }
    public bool TrySpendIron(int amount) { var mine=GetComponent<OreRoom>();return mine && mine.SharedInventory.SpendIron(amount); }
    OreRoom room;
    OrePlayer driver;
    float nextSearch;
    bool warned;
    void Awake() { room=GetComponent<OreRoom>(); }
    void Update() {
        if(!room || room.spawnTestPlayer || room.player || room.hub)return;
        if(Time.unscaledTime<nextSearch)return;
        nextSearch=Time.unscaledTime+1;
        if(!sharedOrigin) {
            var main=Camera.main;
            if(main)sharedOrigin=main.GetComponentInParent<XROrigin>();
            if(!sharedOrigin)foreach(var origin in FindObjectsByType<XROrigin>()) {
                if(origin.isActiveAndEnabled && origin.Camera && origin.Camera.isActiveAndEnabled) { sharedOrigin=origin;break; }
            }
        }
        if(!sharedOrigin || !sharedOrigin.isActiveAndEnabled)return;
        if(!sharedCamera)sharedCamera=sharedOrigin.Camera;
        if(!rightController)rightController=FindHand(sharedOrigin.transform,true);
        if(!leftController)leftController=FindHand(sharedOrigin.transform,false);
        if(!sharedCamera || !rightController || !leftController) {
            if(!warned)Debug.LogWarning("Ore room found the shared XR Origin, but needs its Camera and both tracked controller transforms. Assign them on the ROOM's Ore Shared Rig Adapter. No controller scripts are needed.",this);
            warned=true;return;
        }
        var actor=new GameObject("Ore interaction driver - no rig or camera");
        actor.transform.SetParent(transform,false);
        driver=actor.AddComponent<OrePlayer>();
        driver.InitExternal(room,sharedCamera,rightController,leftController);
        room.player=driver;
        Debug.Log("Ore room bound to the existing shared XR Origin. Locomotion stays with the hub rig.",this);
    }
    public static Transform FindHand(Transform rig,bool right) {
        foreach(var t in rig.GetComponentsInChildren<Transform>(true)) {
            string name=t.name.Replace(" ","").Replace("_","").ToLowerInvariant();
            if(name==(right?"rightcontroller":"leftcontroller") || name==(right?"righthand":"lefthand"))return t;
        }
        foreach(var interactor in rig.GetComponentsInChildren<XRBaseInteractor>(true))
            if(interactor.handedness==(right?InteractorHandedness.Right:InteractorHandedness.Left))return interactor.transform;
        return null;
    }
    void OnDestroy() {
        if(room && room.player==driver)room.player=null;
    }
}
}
