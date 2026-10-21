using UnityEngine;
namespace Rishi.OreRoom {
public static class OreVisuals {
    public static Material Material(string name, Color color) {
        var m = Resources.Load<Material>("RishiOre/Materials/" + name);
        if (m) return m;
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) shader = Shader.Find("Standard");
        m = new Material(shader) { name = name, color = color }; return m;
    }
    public static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 p, Vector3 size, Material mat, bool solid = true) {
        var g = GameObject.CreatePrimitive(type); g.name = name;
        g.transform.SetParent(parent, false); g.transform.localPosition = p; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = mat;
        if (!solid) { var c = g.GetComponent<Collider>(); if (Application.isPlaying) Object.Destroy(c); else Object.DestroyImmediate(c); }
        return g;
    }
    public static TextMesh Label(Transform parent, string text, Vector3 p, float size = .1f) {
        var g = new GameObject("Label"); g.transform.SetParent(parent, false); g.transform.localPosition = p;
        var t = g.AddComponent<TextMesh>(); t.text = text; t.characterSize = size; t.fontSize = 48;
        g.AddComponent<OreBillboard>(); t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.color = new Color(.95f,.88f,.64f); return t;
    }
    public static void Burst(Vector3 p, Color color) {
        var g = new GameObject("Ore sparks"); g.transform.position = p;
        var ps = g.AddComponent<ParticleSystem>(); var main = ps.main;
        main.loop = false; main.duration = .35f; main.startLifetime = .45f; main.startSpeed = 1.8f; main.startSize = .05f; main.startColor = color;
        var emission = ps.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, 18) });
        ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = Material("Spark", color); ps.Play(); Object.Destroy(g, 1.5f);
    }
}
}
