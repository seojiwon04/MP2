using System.Collections.Generic;
using UnityEngine;
namespace Rishi.OreRoom {
public class OreMinecart : MonoBehaviour {
    public float speed = 1.8f, delivered;
    public TextMesh status;
    public Transform[] wheels;
    OreRoom room;
    List<Vector2Int> path;
    int next;
    Vector2Int cell;
    bool returning;
    float wait = 4;
    public void Bind(OreRoom r) {
        room=r; cell=r.entrance; transform.position=r.RailPosition(cell);
        foreach(var fence in r.GetComponentsInChildren<Collider>()) if(fence.gameObject.name=="Player safety fence collider")
            foreach(var collider in GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(collider,fence);
        Replan();
    }
    public void Replan() {
        if (!room) return;
        // Finish the current edge first, then plan from that cell: never cut across a bend.
        if (path != null && next < path.Count && Vector3.Distance(transform.position,room.RailPosition(cell))>.05f) return;
        path = room.planner.Find(cell,returning ? room.entrance : room.exit); next = 1;
    }
    void Update() {
        if (!room) return;
        if (wait > 0) { wait-=Time.deltaTime; if (status) status.text="Minecart\nNext trip: "+Mathf.CeilToInt(wait); return; }
        if (path == null || path.Count < 2) { Replan(); if (status) status.text="Minecart\nWaiting for connected rails"; return; }
        if (next >= path.Count) {
            returning=!returning; wait=Random.Range(4f,8f); delivered += 1; Replan(); return;
        }
        Vector3 target = room.RailPosition(path[next]); var delta = target-transform.position;
        if (delta.sqrMagnitude > .001f) transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),180*Time.deltaTime);
        float movement=speed*Time.deltaTime; transform.position=Vector3.MoveTowards(transform.position,target,movement);
        if (wheels != null) foreach (var w in wheels) if(w) w.Rotate(Vector3.up,movement*230,Space.Self);
        if (status) status.text="Minecart\n"+(returning ? "Returning" : "To upper mine")+" | Trips: "+delivered.ToString("0");
        if (Vector3.Distance(transform.position,target)<.01f) {
            cell=path[next];
            if(cell==(returning?room.entrance:room.exit)) { returning=!returning; wait=Random.Range(4f,8f); delivered+=1; }
            Replan();
        }
    }
}
}
