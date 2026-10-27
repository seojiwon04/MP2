using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Rishi.OreRoom {
// This follows the shared player's location, so the outdoor rooms recover their lighting on exit.
[DefaultExecutionOrder(10000)]
public class OreCaveLighting : MonoBehaviour {
    OreRoom room;
    bool applied;
    AmbientMode ambientMode;
    Color skyColor,equatorColor,groundColor;
    float ambientIntensity,reflectionIntensity;
    SphericalHarmonicsL2 ambientProbe;
    Camera viewer;
    CameraClearFlags clearFlags;
    Color background;
    readonly List<Light> directionalLights=new List<Light>();
    readonly List<bool> directionalEnabled=new List<bool>();
    void Awake() { room=GetComponent<OreRoom>(); }
    void Start() {
        // Existing saved prefabs get these values too; no scene regeneration is necessary.
        foreach(var light in GetComponentsInChildren<Light>(true)) {
            if(light.name=="Warm cave lamp") { light.intensity=.04f;light.range=3; }
            else if(light.name=="Equipment bench glow") { light.intensity=.18f;light.range=2.4f; }
        }
    }
    void LateUpdate() {
        var camera=room && room.player?room.player.view:null;
        bool inside=camera && room.player.InMine;
        if(applied && (!inside || viewer!=camera))Restore();
        if(inside && !applied)Enter(camera);
    }
    void Enter(Camera camera) {
        ambientMode=RenderSettings.ambientMode;
        skyColor=RenderSettings.ambientSkyColor;equatorColor=RenderSettings.ambientEquatorColor;groundColor=RenderSettings.ambientGroundColor;
        ambientIntensity=RenderSettings.ambientIntensity;reflectionIntensity=RenderSettings.reflectionIntensity;
        ambientProbe=RenderSettings.ambientProbe;
        viewer=camera;clearFlags=camera.clearFlags;background=camera.backgroundColor;
        directionalLights.Clear();directionalEnabled.Clear();
        foreach(var light in FindObjectsByType<Light>())if(light.type==LightType.Directional) {
            directionalLights.Add(light);directionalEnabled.Add(light.enabled);light.enabled=false;
        }
        var dark=new Color(.006f,.007f,.009f);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=dark;
        RenderSettings.ambientIntensity=.02f;RenderSettings.reflectionIntensity=.02f;
        var probe=new SphericalHarmonicsL2();probe.AddAmbientLight(dark);RenderSettings.ambientProbe=probe;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.002f,.003f,.004f);
        applied=true;
    }
    void Restore() {
        if(!applied)return;applied=false;
        RenderSettings.ambientMode=ambientMode;
        RenderSettings.ambientSkyColor=skyColor;RenderSettings.ambientEquatorColor=equatorColor;RenderSettings.ambientGroundColor=groundColor;
        RenderSettings.ambientIntensity=ambientIntensity;RenderSettings.reflectionIntensity=reflectionIntensity;
        RenderSettings.ambientProbe=ambientProbe;
        for(int i=0;i<directionalLights.Count;i++)if(directionalLights[i])directionalLights[i].enabled=directionalEnabled[i];
        directionalLights.Clear();directionalEnabled.Clear();
        if(viewer) { viewer.clearFlags=clearFlags;viewer.backgroundColor=background; }
        viewer=null;
    }
    void OnDisable() { Restore(); }
    void OnDestroy() { Restore(); }
}
}
