using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
namespace Rishi.OreRoom {
// The western cave entrance is ordinary geometry, with no extra rig, canvas or travel menu.
[DisallowMultipleComponent]
public class OreHubConnection : MonoBehaviour {
    public const string EntranceName="Ore hub entrance";
    public static void Prepare(OreRoom room) {
        if(!room)return;
        room.spawnTestPlayer=false;room.integratedRoom=true;
        var connection=room.GetComponent<OreHubConnection>();
        if(!connection)connection=room.gameObject.AddComponent<OreHubConnection>();
        var adapter=room.GetComponent<OreSharedRigAdapter>();
        if(!adapter)adapter=room.gameObject.AddComponent<OreSharedRigAdapter>();
        XROrigin origin=null;
        foreach(var candidate in Object.FindObjectsByType<XROrigin>())
            if(candidate.gameObject.scene==room.gameObject.scene && candidate.isActiveAndEnabled) { origin=candidate;break; }
        if(origin) {
            adapter.sharedOrigin=origin;adapter.sharedCamera=origin.Camera;
            adapter.rightController=OreSharedRigAdapter.FindHand(origin.transform,true);
            adapter.leftController=OreSharedRigAdapter.FindHand(origin.transform,false);
        }
        var entrance=room.transform.Find(EntranceName);
        if(!entrance) {
            entrance=new GameObject(EntranceName).transform;entrance.SetParent(room.transform,false);
            var back=room.transform.Find("Back wall");
            if(back) {
                var material=back.GetComponent<Renderer>().sharedMaterial;
                // Keep the old wall as the left section, opening a four-meter doorway in its center.
                back.localPosition=new Vector3(-5.25f,2,-6.5f);back.localScale=new Vector3(6.5f,5,1);
                OreVisuals.Part(entrance,"Entrance right wall",PrimitiveType.Cube,new Vector3(5.25f,2,-6.5f),new Vector3(6.5f,5,1),material);
                OreVisuals.Part(entrance,"Entrance lintel",PrimitiveType.Cube,new Vector3(0,4.1f,-6.5f),new Vector3(4,.8f,1),material);
            }
            var floor=room.transform.Find("Cave floor");
            var ground=floor?floor.GetComponent<Renderer>().sharedMaterial:OreVisuals.Material("Ground",Color.gray);
            OreVisuals.Part(entrance,"Floor to Ore",PrimitiveType.Cube,new Vector3(0,-.15f,-16.5f),new Vector3(4,.3f,20),ground);
        }
        var caveFloor=room.transform.Find("Cave floor");
        if(caveFloor)MakeTeleportable(caveFloor.gameObject,origin);
        var path=entrance.Find("Floor to Ore");
        if(path)MakeTeleportable(path.gameObject,origin);
    }
    static void MakeTeleportable(GameObject floor,XROrigin origin) {
        var area=floor.GetComponent<TeleportationArea>();
        if(!area)area=floor.AddComponent<TeleportationArea>();
        area.colliders.Clear();area.colliders.Add(floor.GetComponent<Collider>());
        area.interactionLayers=UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.GetMask("Teleport");
        if(origin) {
            var provider=origin.GetComponentInChildren<TeleportationProvider>(true);
            if(provider)area.teleportationProvider=provider;
        }
    }
}
}
