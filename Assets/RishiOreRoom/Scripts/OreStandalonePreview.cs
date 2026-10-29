using UnityEngine;
using Unity.XR.CoreUtils;
namespace Rishi.OreRoom {
// Used only by the separate desktop test scene, never saved inside the room prefab.
public class OreStandalonePreview : MonoBehaviour {
    public OreRoom room;
    void Start() {
        if(!room || room.player)return;
        if(FindAnyObjectByType<XROrigin>()) {
            Debug.LogWarning("Desktop preview did not create a player because an XR Origin is already present.",this);return;
        }
        var actor=new GameObject("Desktop preview player - outside room prefab");
        actor.transform.SetPositionAndRotation(room.World(room.spawn),room.transform.rotation);
        room.useDesktopControls=true;
        var player=actor.AddComponent<OrePlayer>();player.Init(room);room.player=player;
    }
}
}
