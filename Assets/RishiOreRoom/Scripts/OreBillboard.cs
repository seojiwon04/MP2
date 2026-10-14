using UnityEngine;
namespace Rishi.OreRoom {
public class OreBillboard : MonoBehaviour {
    public float showDistance = 2.5f;
    public bool requireFocus = true;
    [System.NonSerialized] public float suppressedUntil;
    Renderer textRenderer;
    OreInteractable owner;
    Camera viewer;
    bool visible;
    void Awake() { textRenderer=GetComponent<Renderer>(); owner=GetComponentInParent<OreInteractable>(); }
    void LateUpdate() {
        if(!textRenderer) return;
        if(!viewer || !viewer.isActiveAndEnabled) viewer=Camera.main;
        if(!viewer) { textRenderer.enabled=false; return; }
        Vector3 away=transform.position-viewer.transform.position;
        float range=visible?showDistance+.3f:showDistance;
        visible=Time.time>=suppressedUntil && away.sqrMagnitude <= range*range && Vector3.Dot(viewer.transform.forward,away.normalized)>.25f;
        if(owner) {
            var player=viewer.GetComponentInParent<OrePlayer>();
            if(player) visible &= !player.IsCarried(owner) && (!requireFocus || player.IsFocused(owner));
        }
        textRenderer.enabled=visible;
        if(visible && away.sqrMagnitude>.0001f) transform.rotation=Quaternion.LookRotation(away);
    }
}
}
