using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
namespace Rishi.OreRoom {
[DefaultExecutionOrder(-200)]
public class OreDesktopBridge : MonoBehaviour {
    public OreHubIntegration hub;
    OrePlayer player;
    XRDirectInteractor interactor;
    XRGrabInteractable held;
    bool activated;
    void Start() {
        player=GetComponent<OrePlayer>();
        if(!FindAnyObjectByType<XRInteractionManager>())new GameObject("Shared XR interaction manager").AddComponent<XRInteractionManager>();
        var hand=new GameObject("Desktop interaction hand");hand.transform.SetParent(player.view.transform,false);hand.transform.localPosition=new Vector3(.25f,-.25f,.8f);
        var trigger=hand.AddComponent<SphereCollider>();trigger.isTrigger=true;trigger.radius=.1f;
        var rb=hand.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
        interactor=hand.AddComponent<XRDirectInteractor>();
        foreach(var canvas in FindObjectsByType<Canvas>()) {
            if(canvas.renderMode!=RenderMode.WorldSpace)continue;
            canvas.worldCamera=player.view;
            if(!canvas.GetComponent<GraphicRaycaster>())canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    void Update() {
        if(!player || !hub)return;
        var k=Keyboard.current;var m=Mouse.current;
        if(k!=null) {
            if(k.qKey.wasPressedThisFrame && held)Drop();
        }
        if(Cursor.lockState!=CursorLockMode.Locked) { if(activated)Deactivate();return; }
        bool interact=k!=null && k.eKey.wasPressedThisFrame;
        bool click=m!=null && m.leftButton.wasPressedThisFrame;
        if(held) {
            player.consumedInputFrame=Time.frameCount;
            bool down=m!=null && m.leftButton.isPressed;
            if(down && !activated) { held.activated.Invoke(new ActivateEventArgs{interactableObject=held,interactorObject=interactor});activated=true; }
            if(!down && activated)Deactivate();
        }
        if(!interact && !click)return;
        if(TryButton()) { player.consumedInputFrame=Time.frameCount;return; }
        var ray=new Ray(player.view.transform.position,player.view.transform.forward);
        var hits=Physics.RaycastAll(ray,4,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits) {
            if(hit.collider.transform.IsChildOf(transform) || held && hit.collider.transform.IsChildOf(held.transform))continue;
            var action=hit.collider.GetComponentInParent<OreHubAction>();
            if(action) { player.consumedInputFrame=Time.frameCount;action.Perform();return; }
            var native=hit.collider.GetComponentInParent<OreInteractable>();if(native)return;
            var log=hit.collider.GetComponentInParent<LogPickup>();if(log && interact) { log.Collect();player.consumedInputFrame=Time.frameCount;return; }
            if(held && click && UseTool(hit.collider))return;
            var grab=hit.collider.GetComponentInParent<XRGrabInteractable>();
            if(grab && interact) {
                player.consumedInputFrame=Time.frameCount;Drop();if(player.held)player.Drop();
                interactor.StartManualInteraction((IXRSelectInteractable)grab);held=grab;
                hub.room.Notify("Holding "+grab.name+". Left click uses the tool; E interacts; Q drops.");return;
            }
            if(interact) {
                var meat=hit.collider.GetComponentInParent<FishMeatCollector>();if(meat) { meat.Collect();player.consumedInputFrame=Time.frameCount;return; }
                var net=hit.collider.GetComponentInParent<FishingLiftNet>();if(net) { net.CollectFish();player.consumedInputFrame=Time.frameCount;return; }
                var simple=hit.collider.GetComponentInParent<XRSimpleInteractable>();
                if(simple) { interactor.StartManualInteraction((IXRSelectInteractable)simple);interactor.EndManualInteraction();player.consumedInputFrame=Time.frameCount;return; }
            }
            if(!hit.collider.isTrigger)break;
        }
    }
    bool TryButton() {
        if(!EventSystem.current)return false;
        var data=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
        foreach(var hit in hits) {
            var button=hit.gameObject.GetComponentInParent<Button>();
            if(!button || !button.IsInteractable())continue;
            if(Vector3.Distance(player.view.transform.position,button.transform.position)>5)continue;
            button.onClick.Invoke();return true;
        }
        return false;
    }
    bool UseTool(Collider hit) {
        var farmHoe=held.GetComponent<SheepFarm.Hoe>();
        if(farmHoe && hit.name=="Ground") {
            Vector3 p=SheepFarm.GridSnap.Snap(player.view.transform.position+player.view.transform.forward*2);p.y=0;
            if(SheepFarm.GridSnap.IsFree(p))Instantiate(farmHoe.plotPrefab,p,Quaternion.identity);
            return true;
        }
        var seedBag=held.GetComponent<SheepFarm.SeedBag>();var plot=hit.GetComponentInParent<SheepFarm.TilledPlot>();
        if(seedBag && plot && !plot.grass && SheepFarm.PlayerResources.Instance.Spend("Seeds",1)) {
            var g=Instantiate(seedBag.grassPrefab,plot.transform.position+Vector3.up*.2f,Quaternion.identity);
            var grass=g.GetComponent<SheepFarm.Grass>();grass.growth.value=5;grass.plot=plot;plot.grass=grass;return true;
        }
        var grindstone=hit.GetComponentInParent<TridentSharpener>();
        if(grindstone && held.GetComponent<TridentStats>()) { held.GetComponent<TridentStats>().Sharpen(grindstone.sharpenAmount);return true; }
        var cutter=held.GetComponent<FishCutter>();var cutFish=hit.GetComponentInParent<FishStats>();
        if(cutter && cutFish && !cutFish.canGrow) { cutter.CutFish(cutFish);return true; }
        var repair=held.GetComponent<HammerRepair>();var repairNet=hit.GetComponentInParent<FishingLiftNet>();
        if(repair && repairNet) { repairNet.Repair(repair.repairAmount);return true; }
        var tree=hit.GetComponentInParent<TreeGrowth>();
        if(tree && held.GetComponent<AxeTool>()) { tree.TakeChopDamage(held.GetComponent<AxeTool>().damage);return true; }
        var cell=hit.GetComponentInParent<global::GridCell>();
        if(cell && held.GetComponent<HoeTool>()) { cell.Till();return true; }
        if(cell && held.GetComponent<Seed>()) { var seed=held.gameObject;Drop();cell.TryPlant(seed);return true; }
        if(tree && held.GetComponent<Bucket>()) { tree.AddMoisture(held.GetComponent<Bucket>().moistureAmount);return true; }
        var sheep=hit.GetComponentInParent<SheepFarm.Sheep>();
        if(sheep && held.GetComponent<SheepFarm.Shears>()) { sheep.Shear();return true; }
        if(held.GetComponent<TridentStats>() && hit.GetComponentInParent<FishStats>()) {
            var fish=hit.GetComponentInParent<FishStats>();held.SendMessage("OnTriggerEnter",fish.GetComponent<Collider>(),SendMessageOptions.DontRequireReceiver);return true;
        }
        return false;
    }
    void Deactivate() { if(held)held.deactivated.Invoke(new DeactivateEventArgs{interactableObject=held,interactorObject=interactor});activated=false; }
    public void Drop() { Deactivate();if(interactor && interactor.isPerformingManualInteraction)interactor.EndManualInteraction();held=null; }
    void OnDisable() { Drop(); }
}
}
