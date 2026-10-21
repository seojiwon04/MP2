using UnityEngine;
namespace Rishi.OreRoom {
// A physical coal instance disappears on contact and increases the drill's fuel stat.
public class OreDrillFuelReceiver : MonoBehaviour {
    public OreInteractable drill;
    void OnTriggerEnter(Collider other) {
        if(!drill) return;
        var chunk=other.GetComponentInParent<OreInteractable>();
        if(chunk) drill.AcceptCoalFragment(chunk);
    }
}
}
