using UnityEngine;
namespace Rishi.OreRoom {
// Scene-instance adapter: the supplied forest grid generator uses world-origin positions.
[DefaultExecutionOrder(1000)]
public class OreForestPlacement : MonoBehaviour {
    public GridManager forestGrid;
    public GameObject forestRoot;
    void LateUpdate() {
        if(!forestGrid)forestGrid=forestRoot?forestRoot.GetComponentInChildren<GridManager>():GetComponentInChildren<GridManager>();
        if(!forestGrid)return;
        float x=(forestGrid.width-1)*forestGrid.spacing*.5f,z=(forestGrid.height-1)*forestGrid.spacing*.5f;
        foreach(var cell in forestGrid.GetComponentsInChildren<global::GridCell>()) {
            Vector3 wanted=forestGrid.transform.TransformPoint(new Vector3(cell.x*forestGrid.spacing-x,0,cell.z*forestGrid.spacing-z));
            Vector3 delta=wanted-cell.transform.position;
            if(delta.sqrMagnitude<.001f)continue;
            cell.transform.position=wanted;if(cell.currentObject)cell.currentObject.transform.position+=delta;
        }
    }
}
}
