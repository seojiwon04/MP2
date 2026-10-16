using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // Strike the ground with the blade to make a tilled plot.
    public class Hoe : MonoBehaviour
    {
        public GameObject plotPrefab;
        public Vector3 bladeTip = new Vector3(-0.38f, 0.5f, 0); // in the hoe's local space
        public float strikeSpeed = 1f;

        XRGrabInteractable grab;
        float lastY;
        float cooldown;

        void Start()
        {
            grab = GetComponent<XRGrabInteractable>();
            lastY = transform.TransformPoint(bladeTip).y;
        }

        void Update()
        {
            Vector3 tip = transform.TransformPoint(bladeTip);
            float downSpeed = (lastY - tip.y) / Time.deltaTime;
            lastY = tip.y;
            cooldown -= Time.deltaTime;

            if (grab.isSelected && tip.y < 0.1f && downSpeed > strikeSpeed && cooldown <= 0)
            {
                cooldown = 0.5f;
                Vector3 cell = GridSnap.Snap(tip);
                if (GridSnap.IsFree(cell)) Instantiate(plotPrefab, cell, Quaternion.identity);
            }
        }
    }
}
