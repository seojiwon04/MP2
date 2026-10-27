using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine;
namespace Rishi.OreRoom {
[DefaultExecutionOrder(-500)]
public class OreHubIntegration : MonoBehaviour {
    public OreRoom room;
    public bool useDesktopControls=true;
    [Tooltip("0 starts in the hub; 1 starts in the mine. Both use the same connected world.")]
    public int initialDestination;
    public GameObject[] visitRooms=new GameObject[4];
    public bool delayUnvisitedRooms=true;
    public Camera sharedCamera;
    public Transform sharedRig, rightController, leftController;
    public Vector3 hubCenter=new Vector3(-80,0,0);
    public Vector3[] destinations={new Vector3(-80,.2f,-8),new Vector3(-135,.2f,-3.7f),new Vector3(65,.2f,-5),new Vector3(0,.2f,32),new Vector3(0,.2f,-4.5f)};
    public OreSharedInventory Inventory { get; private set; }
    public OrePlayer Player { get; private set; }
    readonly string[] travelNames={"Hub","Ore mine","Forest","Fishing coast","Sheep farm"};
    XRInteractionManager interactionManager;
    void Awake() {
        if(delayUnvisitedRooms)for(int i=1;i<visitRooms.Length;i++)if(visitRooms[i])visitRooms[i].SetActive(false);
    }
    IEnumerator Start() {
        if(!room)room=FindAnyObjectByType<OreRoom>();
        if(!room) { Debug.LogError("Ore hub has no room. Let the automatic ore-room repair finish before pressing Play.");yield break; }
        room.hub=this;Inventory=new OreSharedInventory(room);
        var ironWall=GetComponent<OreIronWall>();
        if(!ironWall)ironWall=gameObject.AddComponent<OreIronWall>();
        ironWall.hub=this;
        interactionManager=FindAnyObjectByType<XRInteractionManager>();
        if(!interactionManager)interactionManager=new GameObject("Shared XR interaction manager").AddComponent<XRInteractionManager>();
        if(!SheepFarm.PlayerResources.Instance && !FindAnyObjectByType<SheepFarm.PlayerResources>(FindObjectsInactive.Include)) {
            var account=new GameObject("Shared farm resource account");account.transform.SetParent(transform,false);
            account.AddComponent<TextMeshPro>().enabled=false;account.AddComponent<SheepFarm.PlayerResources>();
        }
        yield return null;
        if(useDesktopControls) {
            foreach(var camera in FindObjectsByType<Camera>()) {
                camera.enabled=false;var listener=camera.GetComponent<AudioListener>();if(listener)listener.enabled=false;
            }
            var actor=new GameObject("Shared desktop player");actor.transform.position=destinations[0];
            room.useDesktopControls=true;Player=actor.AddComponent<OrePlayer>();Player.Init(room);room.player=Player;
            actor.AddComponent<OreDesktopBridge>().hub=this;
        } else {
            if(!sharedCamera)sharedCamera=Camera.main;
            if(!sharedCamera) { Debug.LogError("Assign the hub XR camera to Ore Hub Integration.");yield break; }
            if(!sharedRig)sharedRig=sharedCamera.transform.root;
            if(!rightController || !leftController)foreach(var t in sharedRig.GetComponentsInChildren<Transform>(true)) {
                string n=t.name.ToLowerInvariant();
                if(n.Contains("controller") && n.Contains("right") && !rightController)rightController=t;
                if(n.Contains("controller") && n.Contains("left") && !leftController)leftController=t;
            }
            var driver=new GameObject("Ore interactions for existing hub rig");driver.transform.SetParent(transform,false);
            Player=driver.AddComponent<OrePlayer>();Player.InitExternal(room,sharedCamera,rightController,leftController);room.player=Player;
        }
        foreach(var root in visitRooms)if(root && root.activeInHierarchy)BindRoomInteractions(root);
        CreateTravel();
        Travel(Mathf.Clamp(initialDestination,0,destinations.Length-1));
    }
    void CreateTravel() {
        var positions=new[]{new Vector3(-11,1,0),new Vector3(11,1,0),new Vector3(0,1,-11),new Vector3(0,1,11)};
        for(int i=1;i<5;i++) {
            var a=OreHubAction.Create(transform,"Travel: "+travelNames[i],hubCenter+positions[i-1],OreActionKind.Portal);a.index=i;a.room=room;a.hub=this;
        }
        var back=OreHubAction.Create(room.transform,"RETURN TO HUB - E / Tab",new Vector3(4.7f,1,-3.6f),OreActionKind.Portal);back.index=0;back.room=room;back.hub=this;
        back.GetComponent<Renderer>().material.color=new Color(.12f,.6f,.3f);
        var sign=back.GetComponentInChildren<TextMesh>();
        if(sign) {
            sign.text="RETURN TO HUB\nE to travel | Tab shortcut\nInventory stays with you";
            var billboard=sign.GetComponent<OreBillboard>();billboard.requireFocus=false;billboard.showDistance=7;
        }
    }
    void BindRoomInteractions(GameObject root) {
        foreach(var other in root.GetComponentsInChildren<XRInteractionManager>(true))if(other!=interactionManager)other.enabled=false;
        foreach(var item in root.GetComponentsInChildren<XRBaseInteractable>(true))item.interactionManager=interactionManager;
        if(useDesktopControls && Player)foreach(var canvas in root.GetComponentsInChildren<Canvas>(true)) {
            if(canvas.renderMode!=RenderMode.WorldSpace)continue;
            canvas.worldCamera=Player.view;
            if(!canvas.GetComponent<GraphicRaycaster>())canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    public void Travel(int where) {
        if(where<0 || where>=destinations.Length || !Player)return;
        if(where>0 && where-1<visitRooms.Length && visitRooms[where-1]) {
            visitRooms[where-1].SetActive(true);BindRoomInteractions(visitRooms[where-1]);
        }
        var bridge=Player.GetComponent<OreDesktopBridge>();if(bridge)bridge.Drop();
        if(useDesktopControls)Player.Teleport(destinations[where],Quaternion.identity);
        else if(sharedRig && sharedCamera) {
            Vector3 offset=sharedCamera.transform.position-sharedRig.position;offset.y=0;
            var controller=sharedRig.GetComponent<CharacterController>();if(controller)controller.enabled=false;
            sharedRig.position=destinations[where]-offset;
            if(controller)controller.enabled=true;
        }
        room.Notify("Entered "+travelNames[where]+".");
    }
    void Update() {
        if(useDesktopControls && Player && UnityEngine.InputSystem.Keyboard.current!=null) {
            var keys=UnityEngine.InputSystem.Keyboard.current;
            if(keys.tabKey.wasPressedThisFrame || keys.f2Key.wasPressedThisFrame)Travel(0);
            else if(keys.f3Key.wasPressedThisFrame)Travel(1);
            else if(keys.f4Key.wasPressedThisFrame)Travel(2);
            else if(keys.f5Key.wasPressedThisFrame)Travel(3);
            else if(keys.f6Key.wasPressedThisFrame)Travel(4);
        }
    }
}
}
