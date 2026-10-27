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
    public bool InMine { get { if(!room || !view)return false; var p=room.Local(view.transform.position); return Mathf.Abs(p.x)<8 && p.z> -6 && p.z<6; } }
    public OreInteractable held;
    public Camera view;

    public bool VR { get; private set; }
    public int buildType=3;
    public float interactDistance=3f;
    public float droppedResourceRange=6f;
    public float nearbyCollectRange=4f;
    CharacterController body;
    Transform hand,leftHand;
    LineRenderer beam;
    GameObject desktopRig,xrRig;
    Camera desktopCamera,xrCamera;
    Transform desktopHand,desktopLeft,xrHand,xrLeft;
    TextMesh wristReadout;
    bool rigSelected,trackingReady,gripOwnsTool,previousGather,previousLeftGrip;
    bool cycleHoldHandled,helmetHoldHandled,waitingWarning;
    float cycleHoldTime,helmetHoldTime;
    bool previousGrip,previousTrigger,previousAlt,previousBuild,previousCycle,previousHeadlamp;
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
        wristReadout=OreVisuals.Label(xrLeft,"",new Vector3(0,.16f,.03f),.014f);
        var wristLabel=wristReadout.GetComponent<OreBillboard>(); wristLabel.requireFocus=false; wristLabel.showDistance=1.5f;
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
        previousGrip=previousTrigger=previousAlt=previousBuild=previousCycle=previousHeadlamp=previousGather=previousLeftGrip=false;
        gripOwnsTool=false; trackingReady=false; trackingOriginSet=false; waitingWarning=false;
        buildMode=false; target=null; fall=0; rigSelected=true;
        if(held) previousPickHead=held.transform.TransformPoint(Vector3.up*.35f);
        Cursor.lockState=desktop?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=false;
    }
    void AttachHeld() {
        held.transform.SetParent(hand,false);
        held.transform.localPosition=held.kind==OreKind.Lantern?new Vector3(0,-.32f,.08f):new Vector3(0,-.02f,.1f);
        held.transform.localRotation=Quaternion.identity;
    }
    void UpdateWrist(string overrideText=null) {
        if(!wristReadout || !VR) return;
        string equipment=held?held.kind.ToString()+(held.kind==OreKind.Lantern?" fuel "+held.charge.ToString("0"):
            held.kind==OreKind.Pickaxe?" durability "+held.durability.ToString("0"):""):"Hand empty";
        string instruction=buildMode?"Build: "+room.BuildName(buildType)+"\n"+(hasPlacement?(validPlacement?"A / trigger: place":placementReason):"Aim at floor"):
            "A interact | Y collect\nB build | X headlamp";
        wristReadout.text=overrideText??room.ResourcesText()+"\n"+equipment+"\n"+instruction+
            (string.IsNullOrEmpty(room.message)?"":"\n"+WrapWrist(room.message,38));
    }
    static string WrapWrist(string text,int width) {
        var builder=new System.Text.StringBuilder(); int column=0;
        foreach(var word in text.Split(' ')) {
            if(column>0 && column+word.Length+1>width) { builder.Append('\n'); column=0; }
            if(column>0) { builder.Append(' '); column++; }
            builder.Append(word); column+=word.Length;
        }
        return builder.ToString();
    }
    void Pose(UnityEngine.XR.InputDevice device,Transform t) {
        if(device.TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 p)) t.localPosition=p+(trackingOriginSet?Vector3.zero:Vector3.up*1.65f);
        if(device.TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion q)) t.localRotation=q;
    }
    static bool Button(UnityEngine.XR.InputDevice d,InputFeatureUsage<bool> usage) { return d.TryGetFeatureValue(usage,out bool value)&&value; }
    void Update() {
        if(!room || !view) return;
        var kb=Keyboard.current; var mouse=Mouse.current;
        if(!externalRig && kb!=null && kb.f8Key.wasPressedThisFrame) room.useDesktopControls=!room.useDesktopControls;
        if(!externalRig && (!rigSelected || VR==room.useDesktopControls)) SelectRig(room.useDesktopControls);
        var head=InputDevices.GetDeviceAtXRNode(XRNode.Head);
        bool tracked=head.TryGetFeatureValue(CommonUsages.isTracked,out bool tracking)&&tracking;
        trackingReady=head.isValid && tracked && XRSettings.isDeviceActive;
        bool grab=false,drop=false,use=false,alternate=false,build=false,equipment=false,toggleHeadlamp=false,removeHelmet=false;
        bool interact=false,gather=false;
        snapCooldown-=Time.deltaTime;
        if(VR) {
            if(!trackingReady) {
                target=null; hasPlacement=validPlacement=false; ghost.SetActive(false); beam.enabled=false;
                UpdateWrist("VR selected\nWaiting for tracked headset\nCheck OpenXR setup");
                if(!waitingWarning) { Debug.LogWarning("Ore room VR selected: waiting for a tracked headset. Enable OpenXR, select the Touch interaction profile, and connect the headset. For keyboard testing check Use Desktop Controls on the room."); waitingWarning=true; }
                return;
            }
            waitingWarning=false;
            Cursor.lockState=CursorLockMode.None; Cursor.visible=false;
            if(!trackingOriginSet) {
                var subsystems=new System.Collections.Generic.List<XRInputSubsystem>(); SubsystemManager.GetSubsystems(subsystems);
                foreach(var subsystem in subsystems) if(subsystem.running) trackingOriginSet |= subsystem.GetTrackingOriginMode()==TrackingOriginModeFlags.Floor || subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            }
            if(!externalRig)Pose(head,view.transform);
            var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand); var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if(!externalRig) { Pose(right,hand); Pose(left,leftHand); }
            bool grip=Button(right,CommonUsages.gripButton),trigger=Button(right,CommonUsages.triggerButton),alt=Button(left,CommonUsages.triggerButton);
            bool place=Button(right,CommonUsages.primaryButton),cycle=Button(right,CommonUsages.secondaryButton),helmet=Button(left,CommonUsages.primaryButton);
            bool collect=Button(left,CommonUsages.secondaryButton),leftGrip=Button(left,CommonUsages.gripButton);
            grab=grip&&!previousGrip; drop=!grip&&previousGrip&&gripOwnsTool;
            use=trigger&&!previousTrigger; alternate=alt&&!previousAlt;
            if(place&&!previousBuild) { if(buildMode) build=true; else interact=true; }
            gather=collect&&!previousGather;
            // Left grip operates held equipment independently of what the pointer is targeting.
            equipment=leftGrip&&!previousLeftGrip;
            if(cycle&&!previousCycle) { cycleHoldTime=0; cycleHoldHandled=false; }
            if(cycle) {
                cycleHoldTime+=Time.deltaTime;
                if(cycleHoldTime>=.6f && !cycleHoldHandled) { buildMode=false; cycleHoldHandled=true; room.Notify("Building mode closed."); }
            }
            if(!cycle&&previousCycle&&!cycleHoldHandled) {
                if(buildMode) buildType=(buildType+1)%OreRoom.names.Length; else buildMode=true;
            }
            if(helmet&&!previousHeadlamp) { helmetHoldTime=0; helmetHoldHandled=false; }
            if(helmet) {
                helmetHoldTime+=Time.deltaTime;
                if(helmetHoldTime>=.8f && !helmetHoldHandled) { removeHelmet=true; helmetHoldHandled=true; }
            }
            if(!helmet&&previousHeadlamp&&!helmetHoldHandled) toggleHeadlamp=true;
            previousGrip=grip; previousTrigger=trigger; previousAlt=alt; previousBuild=place; previousCycle=cycle;
            previousHeadlamp=helmet; previousGather=collect; previousLeftGrip=leftGrip;
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
        if(consumedInputFrame==Time.frameCount || (room.integratedRoom && !InMine)) { target=null; ghost.SetActive(false); beam.enabled=false; return; }
        Transform aim=VR?hand:view.transform;
        target=FindTarget(aim);
        if(VR) { var near=NearHand(); if(near) target=near; }
        hasPlacement=buildMode && room.TryGetGridTile(new Ray(aim.position,aim.forward),6f,out placement);
        placementReason=hasPlacement?room.BuildBlockedReason(placement,buildType):"Aim at a floor tile within 6 meters.";
        validPlacement=hasPlacement && string.IsNullOrEmpty(placementReason);
        beam.enabled=VR;
        beam.SetPosition(0,aim.position);
        beam.SetPosition(1,buildMode && hasPlacement?placement:target?target.transform.position:aim.position+aim.forward*interactDistance);
        ghost.SetActive(buildMode && hasPlacement); ghost.transform.position=placement;
        if(tileRenderer) tileRenderer.sharedMaterial=validPlacement?validTileMaterial:invalidTileMaterial;
        // Building clicks have one meaning and never also mine or use equipment.
        if(buildMode && (use || build)) {
            if(hasPlacement) room.Build(placement,buildType); else room.Notify(placementReason);
            use=false; build=false;
        }
        if(target) room.Encounter(target);
        if(drop) { Drop(); gripOwnsTool=false; }
        if(grab) {
            if(target && target.grabbable) { Grab(target); gripOwnsTool=held!=null; }
            else if(held) gripOwnsTool=true;
            else room.Notify("Grip grabs equipment. A interacts; Y collects drops.");
        }
        if(interact) {
            if(target && target.grabbable) Grab(target);
            else if((!target || IsOre(target) || target.kind==OreKind.Chunk) && CollectNearbyDrops()) { }
            else if(target) target.Use(this);
            else room.Notify(VR?"Aim at an object, or press Y near dropped resources.":"Move within 4 meters of dropped ore, or aim at equipment.");
        }
        if(gather && !CollectNearbyDrops()) room.Notify("No visible dropped resources within 4 meters.");
        if(equipment) UseEquipment(false);
        if(toggleHeadlamp) ToggleHeadlamp();
        if(removeHelmet) RemoveHelmet();
        if(use) PrimaryUse();
        if(alternate) {
            if(target && (target.kind==OreKind.Smelter || target.kind==OreKind.Drill || target.kind==OreKind.WorkLight)) target.Use(this,true);
            else if(held) held.Use(this,true);
            else if(target) target.Use(this,true);
            else if(wornHelmet) ToggleHeadlamp();
        }

        SwingPickaxe();
        UpdateWrist();
    }
    OreInteractable FindTarget(Transform aim) {
        // Wide aim assistance, with geometry obstruction and player-owned colliders excluded.
        var hits=Physics.SphereCastAll(aim.position,.16f,aim.forward,interactDistance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits) {
            if(hit.collider.transform.IsChildOf(transform)) continue;
            var obj=hit.collider.GetComponentInParent<OreInteractable>();
            if(obj && obj.kind==OreKind.Chunk && (obj.collected || obj.reservedForDrill)) continue;
            if(obj && !IsCarried(obj)) return obj;
            if(!hit.collider.isTrigger) break;
        }
        return FindDropTarget(aim);
    }
    OreInteractable FindDropTarget(Transform aim) {
        // Drops get a separate, much wider aim cone; tools and mining keep their old range.
        OreInteractable best=null; float bestScore=float.MaxValue;
        foreach(var collider in Physics.OverlapSphere(aim.position,droppedResourceRange,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide)) {
            var drop=collider.GetComponentInParent<OreInteractable>();
            if(!drop || drop.kind!=OreKind.Chunk || drop.collected || drop.reservedForDrill) continue;
            Vector3 delta=drop.transform.position-aim.position;
            if(delta.sqrMagnitude>droppedResourceRange*droppedResourceRange) continue;
            float along=Vector3.Dot(delta,aim.forward);
            if(along<=0) continue;
            float sideways=(delta-aim.forward*along).magnitude;
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
            if(hit.collider.transform.IsChildOf(transform) || hit.distance>=distance-.08f) continue;
            var item=hit.collider.GetComponentInParent<OreInteractable>();
            if(item && item.kind==OreKind.Chunk) continue;
            return false;
        }
        return true;
    }
    bool CollectNearbyDrops() {
        int count=0;
        Vector3 center=transform.position+Vector3.up*.65f;
        var seen=new System.Collections.Generic.HashSet<OreInteractable>();
        foreach(var collider in Physics.OverlapSphere(center,nearbyCollectRange,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Collide)) {
            var drop=collider.GetComponentInParent<OreInteractable>();
            if(!drop || drop.kind!=OreKind.Chunk || drop.collected || drop.reservedForDrill || !seen.Add(drop)) continue;
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
            if(!obj || !obj.grabbable || IsCarried(obj)) continue;
            float d=(collider.ClosestPoint(hand.position)-hand.position).sqrMagnitude;
            if(d<distance) { distance=d; best=obj; }
        }
        return best;
    }
    void PrimaryUse() {
        // Lantern operation always wins while it is held; E / grip can still use stations.
        if(held && held.kind==OreKind.Lantern) { held.Use(this); return; }
        if(target) {
            if(target.grabbable) { if(!held) Grab(target); else UseEquipment(false); }
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
        room.Notify(obj.kind==OreKind.Lantern?"Lantern grabbed. F / trigger toggles light; alternate refuels.":"Pickaxe grabbed. Aim at ore and click / trigger.");
    }
    public void Drop() {
        if(!held) return;
        var obj=held; held=null; obj.transform.SetParent(room.transform,true);
        Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;
        obj.transform.position=transform.position+forward*1.05f+Vector3.up*1.05f;
        foreach(var c in obj.GetComponentsInChildren<Collider>()) {
            c.enabled=true;
            // Ignore the player's capsule; do not launch the tool out of the player's body.
            Physics.IgnoreCollision(c,body);
        }
        var rb=obj.GetComponent<Rigidbody>();
        if(rb) { rb.isKinematic=false; rb.useGravity=true; rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero; rb.linearDamping=.5f; rb.angularDamping=3; }
        room.Notify("Equipment dropped.");
    }
    public void EquipHelmet(OreInteractable helmet) {
        if(!helmet || helmet.kind!=OreKind.Helmet || wornHelmet==helmet) return;
        if(wornHelmet) RemoveHelmet();
        if(held==helmet) held=null;
        wornHelmet=helmet; MakeCarried(helmet); helmet.transform.SetParent(view.transform,false);
        helmet.transform.localPosition=Vector3.up*.14f; helmet.transform.localRotation=Quaternion.identity;
        foreach(var r in helmet.GetComponentsInChildren<Renderer>()) r.enabled=false;
        if(helmet.lamp) helmet.lamp.enabled=true;
        room.Notify("Helmet equipped. H / left primary button toggles headlamp.");
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
        helmet.transform.SetParent(room.transform,true); helmet.transform.position=room.World(new Vector3(0,1,-3)); helmet.transform.rotation=Quaternion.identity;
        foreach(var r in helmet.GetComponentsInChildren<Renderer>()) r.enabled=true;
        foreach(var c in helmet.GetComponentsInChildren<Collider>()) c.enabled=true;
        if(helmet.lamp) helmet.lamp.enabled=false;
        room.Notify("Helmet returned to the bench.");
    }
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
    void OnDisable() { Application.onBeforeRender-=BeforeRender; }
    void BeforeRender() {
        if(externalRig || !VR || !view || !trackingReady) return;
        Pose(InputDevices.GetDeviceAtXRNode(XRNode.Head),view.transform);
        Pose(InputDevices.GetDeviceAtXRNode(XRNode.RightHand),hand);
        Pose(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand),leftHand);
    }
    void OnDestroy() { Cursor.lockState=CursorLockMode.None; Cursor.visible=true; if(ghost) Destroy(ghost); if(invalidTileMaterial) Destroy(invalidTileMaterial); }
}
}
