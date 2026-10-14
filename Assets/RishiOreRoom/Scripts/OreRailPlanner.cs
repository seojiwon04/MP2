using System.Collections.Generic;
using UnityEngine;
namespace Rishi.OreRoom {
// Manhattan-distance A*: searches connected rail tiles, excluding blocked cells.
public class OreRailPlanner {
    public readonly HashSet<Vector2Int> rails = new HashSet<Vector2Int>();
    public readonly HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();
    static readonly Vector2Int[] directions = { Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left };
    public List<Vector2Int> Find(Vector2Int start, Vector2Int goal) {
        var open = new List<Vector2Int>(); var closed = new HashSet<Vector2Int>();
        var came = new Dictionary<Vector2Int,Vector2Int>(); var costs = new Dictionary<Vector2Int,int>();
        if (!rails.Contains(start) || !rails.Contains(goal) || blocked.Contains(goal)) return null;
        open.Add(start); costs[start] = 0;
        while (open.Count > 0) {
            var current = open[0]; int best = costs[current] + H(current,goal);
            foreach (var c in open) { int f = costs[c] + H(c,goal); if (f < best) { current = c; best = f; } }
            if (current == goal) { var path = new List<Vector2Int> { current }; while (came.ContainsKey(current)) { current = came[current]; path.Add(current); } path.Reverse(); return path; }
            open.Remove(current); closed.Add(current);
            foreach (var d in directions) {
                var next = current+d;
                if (!rails.Contains(next) || blocked.Contains(next) || closed.Contains(next)) continue;
                int cost = costs[current]+1;
                if (!costs.ContainsKey(next) || cost < costs[next]) { costs[next] = cost; came[next] = current; if (!open.Contains(next)) open.Add(next); }
            }
        }
        return null;
    }
    static int H(Vector2Int a, Vector2Int b) { return Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y); }
}
}
