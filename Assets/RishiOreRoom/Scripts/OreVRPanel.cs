using UnityEngine;
namespace Rishi.OreRoom {
// A room-owned world panel: it stays where opened, has no colliders, and follows neither hand nor head.
public sealed class OreVRPanel {
    readonly GameObject root;
    readonly TextMesh text;
    readonly Renderer textRenderer;
    readonly Material backdrop;
    public OreVRPanel(Transform room) {
        root=new GameObject("Ore information panel - world anchored");root.transform.SetParent(room,false);
        var background=GameObject.CreatePrimitive(PrimitiveType.Cube);
        background.name="Panel background";background.transform.SetParent(root.transform,false);
        background.transform.localPosition=new Vector3(0,0,.015f);
        background.transform.localScale=new Vector3(.86f,.68f,.008f);
        var collider=background.GetComponent<Collider>();collider.enabled=false;Object.Destroy(collider);
        var shader=Shader.Find("Universal Render Pipeline/Unlit");
        if(!shader)shader=Shader.Find("Unlit/Color");
        backdrop=shader?new Material(shader):new Material(OreVisuals.Material("Metal",Color.gray));
        backdrop.color=new Color(.025f,.03f,.04f);
        background.GetComponent<Renderer>().sharedMaterial=backdrop;
        var label=new GameObject("Panel text");label.transform.SetParent(root.transform,false);
        label.transform.localPosition=new Vector3(-.39f,.30f,0);
        text=label.AddComponent<TextMesh>();text.anchor=TextAnchor.UpperLeft;text.alignment=TextAlignment.Left;
        text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize=64;text.characterSize=.038f;text.color=Color.white;
        textRenderer=label.GetComponent<Renderer>();textRenderer.sharedMaterial=text.font.material;
        root.SetActive(false);
    }
    public void Anchor(Camera view) {
        root.transform.SetPositionAndRotation(view.transform.position+view.transform.forward*1.35f,view.transform.rotation);
        // Keep a consistent physical size even when the room is placed under a scaled parent.
        root.transform.localScale=Vector3.one;
        var scale=root.transform.lossyScale;
        root.transform.localScale=new Vector3(1/Mathf.Max(.001f,Mathf.Abs(scale.x)),1/Mathf.Max(.001f,Mathf.Abs(scale.y)),1/Mathf.Max(.001f,Mathf.Abs(scale.z)));
    }
    public void Show(string content) {
        if(text.text!=content)text.text=content;
        root.SetActive(true);Fit();
    }
    public void Fit() {
        if(!root.activeSelf)return;
        var size=textRenderer.localBounds.size;
        if(size.x>.0001f && size.y>.0001f)
            text.transform.localScale=Vector3.one*Mathf.Min(1,Mathf.Min(.78f/size.x,.60f/size.y));
    }
    public void Hide() { if(root)root.SetActive(false); }
    public void Dispose() { if(root)Object.Destroy(root);if(backdrop)Object.Destroy(backdrop); }
}
}
