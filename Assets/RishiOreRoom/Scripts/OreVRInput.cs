using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using XRUsages=UnityEngine.XR.CommonUsages;
namespace Rishi.OreRoom {
// These actions belong to the mine. Existing hub action assets and controllers are never edited.
public static class OreVRInput {
    static InputActionAsset actions;
    static bool warned;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { if(actions)actions.Disable();actions=null;warned=false; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register() {
        if(actions)return;
        actions=Resources.Load<InputActionAsset>("RishiOre/OreQuestControls");
        if(actions)actions.Enable();
        else if(!warned) {
            warned=true;Debug.LogError("Missing RishiOre/OreQuestControls.inputactions. Copy the Resources folder included in the ore controller patch, then rebuild the APK.");
        }
    }
    static bool Read(string action, XRController device, UnityEngine.XR.InputDevice legacy,
        string button, InputFeatureUsage<bool> feature, string axis=null, InputFeatureUsage<float> analog=default(InputFeatureUsage<float>)) {
        var binding=actions?actions.FindAction("OreRoom/"+action,false):null;
        if(binding!=null && binding.IsPressed())return true;
        if(device!=null && device.added) {
            var control=device.TryGetChildControl<ButtonControl>(button);
            if(control!=null && control.isPressed)return true;
            var value=axis==null?null:device.TryGetChildControl<AxisControl>(axis);
            if(value!=null && value.ReadValue()>=.65f)return true;
        }
        if(legacy.TryGetFeatureValue(feature,out bool pressed) && pressed)return true;
        return axis!=null && legacy.TryGetFeatureValue(analog,out float amount) && amount>=.65f;
    }
    public static OreVRButtons Poll() {
        Register();
        var right=InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        var left=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        return new OreVRButtons {
            grip=Read("Grab",XRController.rightHand,right,"gripPressed",XRUsages.gripButton,"grip",XRUsages.grip),
            trigger=Read("Use",XRController.rightHand,right,"triggerPressed",XRUsages.triggerButton,"trigger",XRUsages.trigger),
            alternate=Read("Alternate",XRController.leftHand,left,"triggerPressed",XRUsages.triggerButton,"trigger",XRUsages.trigger),
            build=Read("Build",XRController.rightHand,right,"secondaryButton",XRUsages.secondaryButton),
            lamp=Read("Headlamp",XRController.leftHand,left,"primaryButton",XRUsages.primaryButton),
            collect=Read("Collect",XRController.leftHand,left,"secondaryButton",XRUsages.secondaryButton),
            info=Read("Information",XRController.leftHand,left,"gripPressed",XRUsages.gripButton,"grip",XRUsages.grip)
        };
    }
    public static bool HeadTracked {
        get {
            var hmd=InputSystem.GetDevice<XRHMD>();
            if(hmd!=null && hmd.added && hmd.isTracked!=null && hmd.isTracked.isPressed)return true;
            var head=InputDevices.GetDeviceAtXRNode(XRNode.Head);
            return head.isValid && head.TryGetFeatureValue(XRUsages.isTracked,out bool tracked) && tracked;
        }
    }
    public static bool HandTracked(bool right) {
        var controller=right?XRController.rightHand:XRController.leftHand;
        if(controller!=null && controller.added && controller.isTracked!=null && controller.isTracked.isPressed)return true;
        var legacy=InputDevices.GetDeviceAtXRNode(right?XRNode.RightHand:XRNode.LeftHand);
        return legacy.isValid && legacy.TryGetFeatureValue(XRUsages.isTracked,out bool tracked) && tracked;
    }
    public static bool TryAim(Transform trackingSpace,out Ray ray) {
        ray=default(Ray);
        var controller=XRController.rightHand;
        if(!trackingSpace || controller==null || !controller.added || !HandTracked(true))return false;
        var position=controller.TryGetChildControl<Vector3Control>("pointerPosition")
            ?? controller.TryGetChildControl<Vector3Control>("pointerPose/position");
        var rotation=controller.TryGetChildControl<QuaternionControl>("pointerRotation")
            ?? controller.TryGetChildControl<QuaternionControl>("pointerPose/rotation");
        if(position==null || rotation==null)return false;
        Quaternion q=rotation.ReadValue();
        if(q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w<.5f)return false;
        ray=new Ray(trackingSpace.TransformPoint(position.ReadValue()),trackingSpace.rotation*q*Vector3.forward);
        return true;
    }
}
}
