using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
namespace Rishi.OreRoom {
public class OrePlayer : MonoBehaviour {
    public OreRoom room;
    public bool externalRig;
    public int consumedInputFrame=-1;
    public bool HasHelmet { get { return wornHelmet!=null; } }
    public bool InMine { get { if(!room || !view)return false; var p=room.Local(view.transform.position); return Mathf.Abs(p.x)<8 && p.z> -6 && p.z<6 && p.y> -1 && p.y<4.5f; } }
    public OreInteractable held;
    public Camera view;

    public bool VR { get; private set; }
    public int buildType=3;
    public float interactDistance=3f;
    public float droppedResourceRange=6f;
    public float nearbyCollectRange=4f;
    CharacterController body;
    Transform hand,leftHand;
    Transform externalOrigin;
    bool wasInside;
    LineRenderer beam;
    GameObject desktopRig,xrRig;
    Camera desktopCamera,xrCamera;
    Transform desktopHand,desktopLeft,xrHand,xrLeft;
    OreVRPanel vrPanel;
    readonly OreVRControls vrControls=new OreVRControls();
    Transform trackingSpace;
    bool rigSelected,trackingReady,gripOwnsTool,waitingWarning,helmetWasLit;
    string lastMessage,notice;
    float noticeUntil;
    bool trackingOriginSet,buildMode,showHelp;
    float pitch,fall,snapCooldown,swingCooldown;
    Vector3 previousPickHead,placement;
    OreInteractable target,wornHelmet;
    bool validPlacement,hasPlacement;
    string placementReason;
    Material validTileMaterial,invalidTileMaterial;
    Renderer tileRenderer;
    GameObject ghost;
    GUIStyle hud;
    public void Init(OreRoom r) {
        room=r; body=gameObject.AddComponent<CharacterController>(); body.height=1.8f; body.radius=.25f; body.center=Vector3.up*.9f;
        desktopRig=new GameObject("Desktop Rig - Keyboard and Mouse"); desktopRig.transform.SetParent(transform,false);
        xrRig=new GameObject("XR Rig - Headset and Controllers"); xrRig.transform.SetParent(transform,false);
        desktopCamera=CreateCamera(desktopRig.transform,"Desktop Camera",false);
        xrCamera=CreateCamera(xrRig.transform,"Tracked Headset Camera",true);
        desktopHand=CreateHand(desktopRig.transform,"Desktop Hold",true);
        desktopLeft=CreateHand(desktopRig.transform,"Desktop Left",false);
        xrHand=CreateHand(xrRig.transform,"Tracked Right Controller",true);
        xrLeft=CreateHand(xrRig.transform,"Tracked Left Controller",false);
        beam=xrHand.gameObject.AddComponent<LineRenderer>(); beam.positionCount=2; beam.startWidth=.008f; beam.endWidth=.003f; beam.sharedMaterial=OreVisuals.Material("Ray",Color.cyan);
        SelectRig(r.useDesktopControls);
        ghost=OreVisuals.Part(null,"Grid placement preview",PrimitiveType.Cube,Vector3.zero,new Vector3(.85f,.025f,.85f),OreVisuals.Material("Preview",new Color(.25f,.8f,.55f)),false);
        validTileMaterial=ghost.GetComponent<Renderer>().sharedMaterial;
        invalidTileMaterial=new Material(validTileMaterial); invalidTileMaterial.color=new Color(.95f,.18f,.12f);
        tileRenderer=ghost.GetComponent<Renderer>();
        foreach(var collider in ghost.GetComponents<Collider>()) collider.enabled=false;
        ghost.SetActive(false);
    }
    public void InitExternal(OreRoom r, Camera camera, Transform right, Transform left) {
        room=r; externalRig=true; VR=true; rigSelected=true; view=camera;
        hand=right?right:camera.transform; leftHand=left?left:camera.transform;
        var origin=camera.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>();
        externalOrigin=origin?origin.transform:camera.transform.root;
        trackingSpace=origin && origin.CameraFloorOffsetObject?origin.CameraFloorOffsetObject.transform:externalOrigin;
        body=externalOrigin.GetComponentInChildren<CharacterController>();
        var beamObject=new GameObject("Ore interaction ray"); beamObject.transform.SetParent(transform,false);
        beam=beamObject.AddComponent<LineRenderer>(); beam.positionCount=2; beam.startWidth=.008f; beam.endWidth=.003f; beam.sharedMaterial=OreVisuals.Material("Ray",Color.cyan);
        ghost=OreVisuals.Part(null,"Ore grid preview",PrimitiveType.Cube,Vector3.zero,new Vector3(.85f,.025f,.85f),OreVisuals.Material("Preview",Color.green),false);
        validTileMaterial=ghost.GetComponent<Renderer>().sharedMaterial;
        invalidTileMaterial=new Material(validTileMaterial); invalidTileMaterial.color=Color.red; tileRenderer=ghost.GetComponent<Renderer>();
        foreach(var c in ghost.GetComponents<Collider>())c.enabled=false; ghost.SetActive(false);
    }
    public void Teleport(Vector3 feet, Quaternion rotation) {
        if(held)Drop();
        if(body)body.enabled=false;
        transform.SetPositionAndRotation(feet,rotation);
        if(body)body.enabled=true; fall=0;
    }
    Camera CreateCamera(Transform parent,string name,bool stereo) {
        var eye=new GameObject(name); eye.tag="MainCamera"; eye.transform.SetParent(parent,false);
        eye.transform.localPosition=Vector3.up*1.65f;
        var camera=eye.AddComponent<Camera>(); camera.nearClipPlane=.05f;
        if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline==null)
            camera.stereoTargetEye=stereo?StereoTargetEyeMask.Both:StereoTargetEyeMask.None;
        else if(!stereo) {
            var data=eye.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            data.allowXRRendering=false;
        }
        eye.AddComponent<AudioListener>(); return camera;
    }
    Transform CreateHand(Transform parent,string name,bool right) {
        var t=new GameObject(name).transform; t.SetParent(parent,false);
        t.localPosition=new Vector3(right ? 0.3f : -0.3f,1.25f,.5f);
        OreVisuals.Part(t,"Controller",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.09f,OreVisuals.Material("Metal",Color.gray),false);
        return t;
    }
    void SelectRig(bool desktop) {
        // Keep carried objects alive while deactivating their previous parent rig.
        if(held) held.transform.SetParent(room.transform,true);
        if(wornHelmet) wornHelmet.transform.SetParent(room.transform,true);
        desktopRig.SetActive(false); xrRig.SetActive(false);
        VR=!desktop;
        if(desktop) { view=desktopCamera; hand=desktopHand; leftHand=desktopLeft; desktopRig.SetActive(true); }
        else { view=xrCamera; hand=xrHand; leftHand=xrLeft; xrRig.SetActive(true); }
        if(held) AttachHeld();
        if(wornHelmet) { wornHelmet.transform.SetParent(view.transform,false); wornHelmet.transform.localPosition=Vector3.up*.14f; wornHelmet.transform.localRotation=Quaternion.identity; }
        vrControls.Suspend();if(vrPanel!=null)vrPanel.Hide();
        gripOwnsTool=false; trackingReady=false; trackingOriginSet=false; waitingWarning=false;
        buildMode=false; target=null; fall=0; rigSelected=true;
        if(held) previousPickHead=held.transform.TransformPoint(Vector3.up*.35f);
        Cursor.lockState=desktop?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=false;
    }
    void AttachHeld() {
        if(externalRig) { held.transform.SetParent(room.transform,true);FollowExternalEquipment();return; }
        held.transform.SetParent(hand,false);
        held.transform.localPosition=held.kind==OreKind.Lantern?new Vector3(0,-.32f,.08f):new Vector3(0,-.02f,.1f);
        held.transform.localRotation=Quaternion.identity;
    }
    void EnsurePanel() { if(vrPanel==null)vrPanel=new OreVRPanel(room.transform); }
    void UpdateVRDisplay(bool anchor=false) {
        if(!VR || !InMine)return;
        if(room.message!=lastMessage) {
            lastMessage=room.message;
            if(!string.IsNullOrEmpty(lastMessage)) {
                notice=lastMessage;noticeUntil=Time.unscaledTime+3.5f;
                if(vrControls.Menu==OreVRMenu.None)anchor=true;
            }
        }
        string content=null;
        if(vrControls.Menu==OreVRMenu.Building) {
            content="BUILDING MODE - B closes\nAim at the open central floor.\n\n"
                +"Right trigger: Foundation (1 Bar)\nLeft trigger: Drill (2 Bars)\n"
                +"X: Work Light (1 Bar)\nY: Rail (1 Bar)\n\n"
                +"Preview: "+room.BuildName(buildType)+" | Bars: "+room.Count("IronBar")+"\n"
                +WrapWrist(hasPlacement?(validPlacement?"Ready on this tile.":placementReason):placementReason,46);
            if(Time.unscaledTime<noticeUntil)content+="\n\n"+WrapWrist(notice,46);
        } else if(vrControls.Menu==OreVRMenu.Information) {
            string[] options={"Inventory and goal","Controller controls","Use held equipment","Repair / refuel held equipment","Remove helmet","Recover equipment to bench"};
            string detail;
            switch(vrControls.ToolOption) {
            case 0:detail=room.ResourcesText()+"\n\n"+(room.progression?room.progression.Goal:"")+"\n\n"+room.SharedInventory.Summary;break;
            case 1:detail="Right grip: hold equipment; release: drop\nRight trigger: interact / mine / use\nLeft trigger: alternate aimed object\nB: building mode | X: headlamp\nY: collect drops | Left grip: this panel\nA and joysticks: shared rig controls";break;
            case 2:detail="Right trigger here operates the item you hold, regardless of where you aim.";break;
            case 3:detail="Right trigger here repairs the held Pickaxe (1 Bar), or refuels the held Lantern (1 Coal). Aim does not change this action.";break;
            case 4:detail="Right trigger here returns the helmet to the bench.";break;
            default:detail="Right trigger here returns equipment to its bench. Fuel, durability and inventory are not reset.";break;
            }
            content="MINE INFORMATION / TOOLS\nLeft grip closes | Left trigger: next page\nRight trigger: use selected option\n\n"
                +(vrControls.ToolOption+1)+" / "+options.Length+": "+options[vrControls.ToolOption]+"\n\n"+WrapWrist(detail,46);
            if(Time.unscaledTime<noticeUntil)content+="\n\n"+WrapWrist(notice,46);
        } else if(Time.unscaledTime<noticeUntil && !string.IsNullOrEmpty(notice))
            content=WrapWrist(notice,46)+"\n\nLeft grip: information / tools | B: build";
        if(content==null) { if(vrPanel!=null)vrPanel.Hide();return; }
        EnsurePanel();if(anchor)vrPanel.Anchor(view);vrPanel.Show(content);
    }
    void ConfirmToolOption() {
        switch(vrControls.ToolOption) {
        case 0:case 1:return;
        case 2:UseEquipment(false);break;
        case 3:
            if(held && (held.kind==OreKind.Pickaxe || held.kind==OreKind.Lantern))held.Use(this,true);
            else room.Notify("Hold the Pickaxe to repair it, or the Lantern to refuel it.");
            break;
        case 4:if(wornHelmet)RemoveHelmet();else room.Notify("No helmet is equipped.");break;
        case 5:RecoverEquipment();break;
        }
    }
    void RecoverEquipment() {
        if(held)ReturnHeldToBench();if(wornHelmet)RemoveHelmet();
        foreach(var item in room.GetComponentsInChildren<OreInteractable>(true)) {
            if(!item.grabbable)continue;
            Vector3 position=item.kind==OreKind.Pickaxe?new Vector3(-2,1.55f,-3):
                item.kind==OreKind.Helmet?new Vector3(0,1,-3):new Vector3(2,1,-3);
            item.transform.SetPositionAndRotation(room.World(position),room.transform.rotation);
            foreach(var collider in item.GetComponentsInChildren<Collider>())collider.enabled=true;
            foreach(var renderer in item.GetComponentsInChildren<Renderer>())renderer.enabled=true;
            var rb=item.GetComponent<Rigidbody>();if(rb) { if(!rb.isKinematic) { rb.linearVelocity=Vector3.zero;rb.angularVelocity=Vector3.zero; } rb.isKinematic=true;rb.useGravity=false; }
        }
        room.Notify("Equipment returned to the bench. Fuel, durability and resources are unchanged.");
    }
    Ray AimRay() {
        // A bad tracking-space offset would put the pointer far from the actual hand.
        // Use the shared rig's tracked controller transform in that case.
        if(VR && externalRig && OreVRInput.TryAim(trackingSpace,out Ray ray) &&
            hand && Vector3.Distance(ray.origin,hand.position)<.75f)return ray;
        var aim=VR?hand:view.transform;return new Ray(aim.position,aim.forward);
    }
    static string WrapWrist(string text,int width) {
        if(string.IsNullOrEmpty(text))return "";
        var builder=new System.Text.StringBuilder(); int column=0;
        var lines=text.Split('\n');
        for(int line=0;line<lines.Length;line++) {
            if(line>0)builder.Append('\n');
            column=0;
            foreach(var word in lines[line].Split(new[]{' '},System.StringSplitOptions.RemoveEmptyEntries)) {
                if(column>0 && column+word.Length+1>width) { builder.Append('\n'); column=0; }
                if(column>0) { builder.Append(' '); column++; }
                builder.Append(word); column+=word.Length;
            }
        }
        return builder.ToString();
    }
    void Pose(UnityEngine.XR.InputDevice device,Transform t) {
        if(device.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p)) t.localPosition=p+(trackingOriginSet?Vector3.zero:Vector3.up*1.65f);
        if(device.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q)) t.localRotation=q;
    }
    void Update() {
        if(!room || !view) return;
        if(externalRig) {
            bool inside=InMine;
            if(!inside) {
                if(wasInside && held)ReturnHeldToBench();
                if(wasInside && wornHelmet && wornHelmet.lamp) { helmetWasLit=wornHelmet.lamp.enabled;wornHelmet.lamp.enabled=false; }
                wasInside=false;target=null;hasPlacement=false;
                if(ghost)ghost.SetActive(false);if(beam)beam.enabled=false;
                vrControls.Suspend();buildMode=false;if(vrPanel!=null)vrPanel.Hide();
                noticeUntil=0;lastMessage=room.message;
                return;
            }
            if(!wasInside) {
                if(wornHelmet && wornHelmet.lamp)wornHelmet.lamp.enabled=helmetWasLit;
                room.Notify("B: building mode. Left grip: information / tools. Right trigger: interact.");
            }
            wasInside=true;
            FollowExternalEquipment();
        }
        var kb=Keyboard.current; var mouse=Mouse.current;
        if(!externalRig && kb!=null && kb.f8Key.wasPressedThisFrame) room.useDesktopControls=!room.useDesktopControls;
        if(!externalRig && (!rigSelected || VR==room.useDesktopControls)) SelectRig(room.useDesktopControls);
        var head=InputDevices.GetDeviceAtXRNode(XRNode.Head);
        trackingReady=OreVRInput.HeadTracked;
        bool grab=false,drop=false,use=false,alternate=false,build=false,equipment=false,toggleHeadlamp=false,removeHelmet=false;
        bool interact=false,gather=false;
        bool menuChanged=false,confirmTool=false;
        int vrBuild=-1;
        snapCooldown-=Time.deltaTime;
        if(VR) {
            if(!trackingReady || !OreVRInput.HandTracked(true) || !OreVRInput.HandTracked(false)) {
                target=null; hasPlacement=validPlacement=false; ghost.SetActive(false); beam.enabled=false;
                vrControls.Suspend();buildMode=false;if(vrPanel!=null)vrPanel.Hide();
                if(!waitingWarning) { Debug.LogWarning("Ore room is waiting for a tracked headset and both Touch controllers. The room leaves shared-rig locomotion unchanged."); waitingWarning=true; }
                return;
            }
            waitingWarning=false;
            Cursor.lockState=CursorLockMode.None; Cursor.visible=false;
            if(!externalRig && !trackingOriginSet) {
                var subsystems=new System.Collections.Generic.List<XRInputSubsystem>(); SubsystemManager.GetSubsystems(subsystems);
                foreach(var subsystem in subsystems) if(subsystem.running) trackingOriginSet |= subsystem.GetTrackingOriginMode()==TrackingOriginModeFlags.Floor || subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            }
            if(!externalRig)Pose(head,view.transform);
            var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand); var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if(!externalRig) { Pose(right,hand); Pose(left,leftHand); }
            var command=vrControls.Step(OreVRInput.Poll());
            grab=command.grab;drop=command.release && gripOwnsTool;
            use=command.use;alternate=command.alternate;gather=command.collect;toggleHeadlamp=command.lamp;
            vrBuild=command.place;confirmTool=command.confirm;menuChanged=command.menuChanged;
            buildMode=vrControls.Menu==OreVRMenu.Building;
            if(buildMode)buildType=vrControls.BuildType;
            // Update capsule geometry before movement for floor-height and room-scale tracking.
            if(!externalRig) {
            body.height=Mathf.Clamp(view.transform.localPosition.y,1,2.2f);
            body.center=new Vector3(view.transform.localPosition.x,body.height*.5f,view.transform.localPosition.z);
            left.TryGetFeatureValue(CommonUsages.primary2DAxis,out Vector2 move); Move(move,2.2f);
            right.TryGetFeatureValue(CommonUsages.primary2DAxis,out Vector2 turn);
            if(Mathf.Abs(turn.x)>.65f && snapCooldown<=0) { transform.RotateAround(view.transform.position,Vector3.up,Mathf.Sign(turn.x)*30); snapCooldown=.3f; }
            }
        } else {
            view.transform.localPosition=Vector3.up*1.65f; body.height=1.8f; body.center=Vector3.up*.9f;
            if(kb!=null && kb.escapeKey.wasPressedThisFrame) { bool locked=Cursor.lockState==CursorLockMode.Locked; Cursor.lockState=locked?CursorLockMode.None:CursorLockMode.Locked; Cursor.visible=locked; }
            if(mouse!=null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState!=CursorLockMode.Locked) { Cursor.lockState=CursorLockMode.Locked; Cursor.visible=false; return; }
            if(Cursor.lockState!=CursorLockMode.Locked) { target=null; ghost.SetActive(false); return; }
            if(mouse!=null) { var d=mouse.delta.ReadValue()*.09f; transform.Rotate(Vector3.up*d.x); pitch=Mathf.Clamp(pitch-d.y,-75,75); }
            view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            if(kb!=null) {
                Vector2 move=new Vector2((kb.dKey.isPressed?1:0)-(kb.aKey.isPressed?1:0),(kb.wKey.isPressed?1:0)-(kb.sKey.isPressed?1:0)); Move(move,kb.leftShiftKey.isPressed?4:2.7f);
                interact=kb.eKey.wasPressedThisFrame; drop=kb.qKey.wasPressedThisFrame; build=kb.bKey.wasPressedThisFrame; equipment=kb.fKey.wasPressedThisFrame;
                toggleHeadlamp=kb.hKey.wasPressedThisFrame; removeHelmet=kb.xKey.wasPressedThisFrame;
                if(kb.digit1Key.wasPressedThisFrame) { buildType=0; buildMode=true; }
                if(kb.digit2Key.wasPressedThisFrame) { buildType=1; buildMode=true; }
                if(kb.digit3Key.wasPressedThisFrame) { buildType=2; buildMode=true; }
                if(kb.digit4Key.wasPressedThisFrame) { buildType=3; buildMode=true; }
                if(kb.gKey.wasPressedThisFrame) buildMode=!buildMode;
                if(kb.f1Key.wasPressedThisFrame) showHelp=!showHelp;
            }
            // Update the hold position after locomotion so equipment never trails the camera.
            hand.position=view.transform.TransformPoint(new Vector3(.32f,-.28f,.6f)); hand.rotation=view.transform.rotation;
            leftHand.position=view.transform.TransformPoint(new Vector3(-.28f,-.23f,.6f)); leftHand.rotation=view.transform.rotation;
            use=mouse!=null&&mouse.leftButton.wasPressedThisFrame; alternate=mouse!=null&&mouse.rightButton.wasPressedThisFrame;
        }
        if(consumedInputFrame==Time.frameCount || (room.integratedRoom && !InMine)) { target=null; ghost.SetActive(false); beam.enabled=false;if(vrPanel!=null)vrPanel.Hide(); return; }
        Ray aim=AimRay();
        target=FindTarget(aim);
        hasPlacement=buildMode && room.TryGetGridTile(aim,10f,out placement);
        placementReason=hasPlacement?room.BuildBlockedReason(placement,buildType):VR?
            "Point the right controller down at the central cave floor (within 10 meters).":
            "Aim at the central cave floor (within 10 meters).";
        validPlacement=hasPlacement && string.IsNullOrEmpty(placementReason);
        beam.enabled=VR;
        beam.SetPosition(0,aim.origin);
        beam.SetPosition(1,buildMode && hasPlacement?placement:target?target.transform.position:aim.origin+aim.direction*interactDistance);
        ghost.SetActive(buildMode && hasPlacement); ghost.transform.SetPositionAndRotation(placement,room.transform.rotation);
        if(tileRenderer) tileRenderer.sharedMaterial=validPlacement?validTileMaterial:invalidTileMaterial;
        // Building clicks have one meaning and never also mine or use equipment.
        if(buildMode && (use || build || vrBuild>=0)) {
            if(hasPlacement) room.Build(placement,buildType); else room.Notify(placementReason);
            use=false; build=false;
        }
        if(drop) { Drop(); gripOwnsTool=false; }
        if(VR && vrControls.Menu!=OreVRMenu.None) {
            target=null;
            if(vrControls.Menu==OreVRMenu.Information) { beam.enabled=false;if(confirmTool)ConfirmToolOption(); }
            previousPickHead=held?held.transform.TransformPoint(Vector3.up*.35f):Vector3.zero;
            UpdateVRDisplay(menuChanged);return;
        }
        // A menu-toggle press is consumed, even when that press closes the menu.
        if(VR && menuChanged) { UpdateVRDisplay(true);return; }
        if(target) room.Encounter(target);
        if(grab) {
            var equipmentTarget=NearHand();
            if(!equipmentTarget && target && target.grabbable)equipmentTarget=target;
            if(equipmentTarget) { Grab(equipmentTarget); gripOwnsTool=held!=null; }
            else if(held) gripOwnsTool=true;
            else room.Notify("Right grip grabs equipment. Right trigger interacts; Y collects drops.");
        }
        if(interact) {
            if(TryRoomAction(aim)) { use=false; }
            else if(target && target.grabbable) Grab(target);
            else if((!target || IsOre(target) || target.kind==OreKind.Chunk) && CollectNearbyDrops()) { }
            else if(target) target.Use(this);
            else room.Notify(VR?"Point the right controller at an object, or press Y near dropped resources.":"Move within 4 meters of dropped ore, or aim at equipment.");
        }
        if(gather && !CollectNearbyDrops()) room.Notify("No visible dropped resources within 4 meters.");
        if(equipment) UseEquipment(false);
        if(toggleHeadlamp) ToggleHeadlamp();
        if(removeHelmet) RemoveHelmet();
        if(use && !TryRoomAction(aim)) PrimaryUse();
        if(alternate) {
            if(target && (target.kind==OreKind.Smelter || target.kind==OreKind.Drill || target.kind==OreKind.WorkLight)) target.Use(this,true);
            else if(held) held.Use(this,true);
            else if(target) target.Use(this,true);
            else if(wornHelmet) ToggleHeadlamp();
        }

        SwingPickaxe();
        UpdateVRDisplay();
    }
    OreInteractable FindTarget(Ray aim) {
        // Wide aim assistance, with geometry obstruction and player-owned colliders excluded.
        var hits=Physics.SphereCastAll(aim.origin,.16f,aim.direction,interactDistance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits) {
            if(IsPlayerCollider(hit.collider)) continue;
            var obj=hit.collider.GetComponentInParent<OreInteractable>();
            if(obj && obj.kind==OreKind.Chunk && (obj.collected || obj.reservedForDrill)) continue;
            if(obj && IsCarried(obj))continue;
            if(obj && obj.room==room) return obj;
            if(!hit.collider.isTrigger) break;
        }
        return FindDropTarget(aim);
    }
    OreInteractable FindDropTarget(Ray aim) {
        // Drops get a separate, much wider aim cone; tools and mining keep their old range.
        OreInteractable best=null; float bestScore=float.MaxValue;
        foreach(var collider in Physics.OverlapSphere(aim.origin,droppedResourceRange,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide)) {
            var drop=collider.GetComponentInParent<OreInteractable>();
            if(!drop || drop.room!=room || drop.kind!=OreKind.Chunk || drop.collected || drop.reservedForDrill) continue;
            Vector3 delta=drop.transform.position-aim.origin;
            if(delta.sqrMagnitude>droppedResourceRange*droppedResourceRange) continue;
            float along=Vector3.Dot(delta,aim.direction);
            if(along<=0) continue;
            float sideways=(delta-aim.direction*along).magnitude;
            if(sideways>.65f+along*.25f || !CanSeeDrop(drop)) continue;
            float score=delta.sqrMagnitude+sideways*sideways*2;
            if(score<bestScore) { best=drop; bestScore=score; }
        }
        return best;
    }
    bool CanSeeDrop(OreInteractable drop) {
        Vector3 delta=drop.transform.position+Vector3.up*.08f-view.transform.position;
        float distance=delta.magnitude;
        if(distance<.01f) return true;
        foreach(var hit in Physics.RaycastAll(view.transform.position,delta/distance,distance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)) {
            if(IsPlayerCollider(hit.collider) || hit.distance>=distance-.08f) continue;
            var item=hit.collider.GetComponentInParent<OreInteractable>();
            if(item && item.kind==OreKind.Chunk) continue;
            return false;
        }
        return true;
    }
    bool CollectNearbyDrops() {
        int count=0;
        Vector3 center=externalRig?view.transform.position-Vector3.up*.8f:transform.position+Vector3.up*.65f;
        var seen=new System.Collections.Generic.HashSet<OreInteractable>();
        foreach(var collider in Physics.OverlapSphere(center,nearbyCollectRange,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide)) {
            var drop=collider.GetComponentInParent<OreInteractable>();
            if(!drop || drop.room!=room || drop.kind!=OreKind.Chunk || drop.collected || drop.reservedForDrill || !seen.Add(drop)) continue;
            if((drop.transform.position-center).sqrMagnitude>nearbyCollectRange*nearbyCollectRange || !CanSeeDrop(drop)) continue;
            drop.Use(this); count++;
        }
        if(count>0) room.Notify("Collected "+count+" nearby resource"+(count==1?".":"s."));
        return count>0;
    }
    OreInteractable NearHand() {
        OreInteractable best=null; float distance=float.MaxValue;
        foreach(var collider in Physics.OverlapSphere(hand.position,.23f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide)) {
            var obj=collider.GetComponentInParent<OreInteractable>();
            if(!obj || obj.room!=room || !obj.grabbable || IsCarried(obj)) continue;
            float d=(collider.ClosestPoint(hand.position)-hand.position).sqrMagnitude;
            if(d<distance) { distance=d; best=obj; }
        }
        return best;
    }
    void PrimaryUse() {
        if(target) {
            if(target.grabbable) { Grab(target);gripOwnsTool=false; }
            else target.Use(this);
        } else UseEquipment(false);
    }
    void UseEquipment(bool alternate) {
        if(held) {
            if(held.kind==OreKind.Pickaxe && !alternate) {
                if(target && IsOre(target)) target.Use(this);
                else room.Notify("Aim the pickaxe at an ore deposit.");
            } else held.Use(this,alternate);
        } else if(wornHelmet) ToggleHeadlamp();
    }
    static bool IsOre(OreInteractable obj) { return obj.kind==OreKind.Iron || obj.kind==OreKind.Coal || obj.kind==OreKind.Crystal; }
    void SwingPickaxe() {
        swingCooldown-=Time.deltaTime;
        if(!held || held.kind!=OreKind.Pickaxe) return;
        Vector3 head=held.transform.TransformPoint(Vector3.up*.35f);
        if(VR && swingCooldown<=0 && Vector3.Distance(previousPickHead,head)/Mathf.Max(Time.deltaTime,.001f)>1.2f) {
            foreach(var c in Physics.OverlapCapsule(previousPickHead,head,.16f)) {
                var obj=c.GetComponentInParent<OreInteractable>();
                if(obj && IsOre(obj)) { obj.Use(this); swingCooldown=.45f; break; }
            }
        }
        previousPickHead=head;
    }
    void Move(Vector2 input,float speed) {
        Vector3 f=view.transform.forward; f.y=0; f.Normalize(); Vector3 right=Vector3.Cross(Vector3.up,f);
        fall=body.isGrounded?-2:fall-9.81f*Time.deltaTime;
        body.Move((Vector3.ClampMagnitude(f*input.y+right*input.x,1)*speed+Vector3.up*fall)*Time.deltaTime);
        if(transform.position.y < -5) { body.enabled=false; transform.position=room.World(room.spawn); body.enabled=true; fall=0; }
    }
    void MakeCarried(OreInteractable obj) {
        var rb=obj.GetComponent<Rigidbody>();
        if(rb) { if(!rb.isKinematic) { rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero; } rb.isKinematic=true; rb.useGravity=false; }
        foreach(var c in obj.GetComponentsInChildren<Collider>()) c.enabled=false;
    }
    public void Grab(OreInteractable obj) {
        if(!obj || !obj.grabbable) return;
        buildMode=false;
        if(obj.kind==OreKind.Helmet) { EquipHelmet(obj); return; }
        if(held==obj) return;
        if(held) Drop();
        held=obj; MakeCarried(obj); AttachHeld();
        previousPickHead=obj.transform.TransformPoint(Vector3.up*.35f);
        room.Notify(obj.kind==OreKind.Lantern?"Lantern grabbed. Use it with the right trigger while pointing away from stations; left trigger refuels. Left grip opens tool options.":"Pickaxe grabbed. Aim at ore and press the right trigger. Repair through Left grip > Repair / refuel.");
    }
    public void Drop() {
        if(!held) return;
        var obj=held; held=null; obj.transform.SetParent(room.transform,true);
        Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;
        obj.transform.position=externalRig?view.transform.position+forward*.55f-Vector3.up*.45f:transform.position+forward*1.05f+Vector3.up*1.05f;
        foreach(var c in obj.GetComponentsInChildren<Collider>()) {
            c.enabled=true;
            // Ignore the player's capsule; do not launch the tool out of the player's body.
            if(body)Physics.IgnoreCollision(c,body);
        }
        var rb=obj.GetComponent<Rigidbody>();
        if(rb) { rb.isKinematic=false; rb.useGravity=true; rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero; rb.linearDamping=.5f; rb.angularDamping=3; }
        room.Notify("Equipment dropped.");
    }
    public void EquipHelmet(OreInteractable helmet) {
        if(!helmet || helmet.kind!=OreKind.Helmet || wornHelmet==helmet) return;
        if(wornHelmet) RemoveHelmet();
        if(held==helmet) held=null;
        wornHelmet=helmet; MakeCarried(helmet);
        helmet.transform.SetParent(externalRig?room.transform:view.transform,false);
        if(externalRig)FollowExternalEquipment();
        else { helmet.transform.localPosition=Vector3.up*.14f; helmet.transform.localRotation=Quaternion.identity; }
        foreach(var r in helmet.GetComponentsInChildren<Renderer>()) r.enabled=false;
        if(helmet.lamp) helmet.lamp.enabled=true;
        helmetWasLit=true;
        room.Notify(VR?"Helmet equipped. X toggles its headlamp. Left grip > Remove helmet returns it to the bench.":"Helmet equipped. H toggles its headlamp.");
    }
    // Retain this entry point for any scene references to the older script.
    public void ToggleHelmet(OreInteractable helmet) { if(wornHelmet==helmet) RemoveHelmet(); else EquipHelmet(helmet); }
    public void ToggleHeadlamp() {
        if(!wornHelmet || !wornHelmet.lamp) { room.Notify("Pick up the helmet to equip its headlamp."); return; }
        wornHelmet.lamp.enabled=!wornHelmet.lamp.enabled;
        room.Notify(wornHelmet.lamp.enabled?"Headlamp on.":"Headlamp off.");
    }
    public void RemoveHelmet() {
        if(!wornHelmet) return;
        var helmet=wornHelmet; wornHelmet=null;
        helmet.transform.SetParent(room.transform,true); helmet.transform.position=room.World(new Vector3(0,1,-3)); helmet.transform.rotation=room.transform.rotation;
        foreach(var r in helmet.GetComponentsInChildren<Renderer>()) r.enabled=true;
        foreach(var c in helmet.GetComponentsInChildren<Collider>()) c.enabled=true;
        if(helmet.lamp) helmet.lamp.enabled=false;
        room.Notify("Helmet returned to the bench.");
    }
    public void ReturnHeldToBench() {
        if(!held)return;
        var item=held;held=null;gripOwnsTool=false;buildMode=false;
        item.transform.SetParent(room.transform,true);
        item.transform.SetPositionAndRotation(room.World(item.kind==OreKind.Lantern?new Vector3(2,1,-3):new Vector3(-2,1.55f,-3)),room.transform.rotation);
        foreach(var c in item.GetComponentsInChildren<Collider>())c.enabled=true;
        var rb=item.GetComponent<Rigidbody>();if(rb) { rb.isKinematic=true;rb.useGravity=false; }
    }
    bool IsPlayerCollider(Collider c) {
        return c.transform.IsChildOf(transform) || externalRig && externalOrigin && c.transform.IsChildOf(externalOrigin);
    }
    bool TryRoomAction(Ray aim) {
        var hits=Physics.SphereCastAll(aim.origin,.12f,aim.direction,interactDistance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits) {
            if(IsPlayerCollider(hit.collider))continue;
            var item=hit.collider.GetComponentInParent<OreInteractable>();
            if(item && IsCarried(item))continue;
            var action=hit.collider.GetComponentInParent<OreHubAction>();
            if(action && action.room==room) { action.Perform();return true; }
            if(!hit.collider.isTrigger)break;
        }
        return false;
    }
    void FollowExternalEquipment() {
        if(!externalRig || !view)return;
        if(held && hand) {
            Vector3 offset=held.kind==OreKind.Lantern?new Vector3(0,-.32f,.08f):new Vector3(0,-.02f,.1f);
            held.transform.SetPositionAndRotation(hand.TransformPoint(offset),hand.rotation);
        }
        if(wornHelmet)wornHelmet.transform.SetPositionAndRotation(view.transform.position+view.transform.up*.14f,view.transform.rotation);
    }
    void LateUpdate() { FollowExternalEquipment();if(vrPanel!=null)vrPanel.Fit(); }
    void OnDisable() {
        Application.onBeforeRender-=BeforeRender;
        if(beam)beam.enabled=false;if(ghost)ghost.SetActive(false);
        vrControls.Suspend();buildMode=false;if(vrPanel!=null)vrPanel.Hide();
    }
    void OnDestroy() {
        if(!externalRig) { Cursor.lockState=CursorLockMode.None;Cursor.visible=true; }
        if(held && room)ReturnHeldToBench();
        if(wornHelmet && room)RemoveHelmet();
        if(ghost)Destroy(ghost);if(invalidTileMaterial)Destroy(invalidTileMaterial);
        if(vrPanel!=null)vrPanel.Dispose();
    }
    public bool IsPlayerBody(Collider c) { return IsPlayerCollider(c); }
    public bool IsFocused(OreInteractable obj) { return obj && target==obj; }
    public bool IsCarried(OreInteractable obj) { return obj && (held==obj || wornHelmet==obj); }
    public string FocusHint {
        get {
            if(!target) return held?"F: use "+held.kind+" | Q: drop":"E: collect nearby drops | Aim at equipment to grab";
            if(target.kind==OreKind.Chunk) return "E: collect nearby drops | Click: collect this drop";
            if(IsOre(target)) return "Click: mine | E: collect nearby drops";
            if(target.kind==OreKind.Helmet) return "E: equip helmet";
            if(target.grabbable) return "E: "+(held?"swap to ":"grab ")+target.kind;
            return "E: interact | "+target.Hint();
        }
    }
    void OnGUI() {
        if(VR || !room || room.integratedRoom && !InMine) return;
        if(hud==null) { hud=new GUIStyle(GUI.skin.box); hud.fontSize=15; hud.alignment=TextAnchor.MiddleLeft; hud.padding=new RectOffset(12,12,8,8); hud.wordWrap=true; }
        float width=room.integratedRoom?Mathf.Min(480,Screen.width*.5f-24):Mathf.Min(520,Screen.width-24);
        GUI.Box(new Rect(12,12,width,60),room.ResourcesText(),hud);
        GUI.Box(new Rect(12,Screen.height-70,width,58),FocusHint+"\n"+(buildMode?"Build: "+room.BuildName(buildType)+" | Click / B place | G close":"F equipment | H headlamp | F1 help"),hud);
        if(buildMode) GUI.Box(new Rect(12,140,width,48),hasPlacement?
            "Tile "+OreRoom.GridCell(room.Local(placement))+" | "+(validPlacement?"Ready to place":placementReason):placementReason,hud);
        if(!string.IsNullOrEmpty(room.message)) GUI.Box(new Rect(12,82,width,48),room.message,hud);
        if(showHelp) GUI.Box(new Rect(12,buildMode?198:140,width,155),"WASD move | Mouse look\nE grab / swap / interact | Q drop\nLeft click use / mine | Right click refuel / repair\nF always uses held equipment | H headlamp | X remove helmet\n1 / 2 / 3 / 4 select building | Click / B place | G exit building\nEsc release cursor | F8 desktop override",hud);
        GUI.color=target?new Color(.5f,1,.65f):Color.white;
        GUI.Label(new Rect(Screen.width/2-6,Screen.height/2-12,20,24),"+"); GUI.color=Color.white;
    }
    void OnEnable() { Application.onBeforeRender+=BeforeRender; }
    void BeforeRender() {
        if(externalRig || !VR || !view || !trackingReady) return;
        Pose(InputDevices.GetDeviceAtXRNode(XRNode.Head),view.transform);
        Pose(InputDevices.GetDeviceAtXRNode(XRNode.RightHand),hand);
        Pose(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand),leftHand);
    }
}
}
