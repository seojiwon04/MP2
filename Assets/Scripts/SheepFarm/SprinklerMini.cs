using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // The small sprinkler on the Trading Post. Let go of it over the ground to buy a real one.
    public class SprinklerMini : MonoBehaviour
    {
        public GameObject sprinklerPrefab;
        public float woolCost = 6;

        XRGrabInteractable grab;
        Rigidbody rb;
        Vector3 homePosition;
        Quaternion homeRotation;
        bool wasHeld;
        float returnTimer;

        void Start()
        {
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
        }

        void Update()
        {
            // Only grabbable when you can afford it (stays grabbable while already held).
            grab.enabled = grab.isSelected || PlayerResources.Instance.Get("Wool") >= woolCost;

            if (wasHeld && !grab.isSelected)
            {
                TryPlace();
                returnTimer = 0.2f;
            }
            wasHeld = grab.isSelected;

            // Go back to the shelf shortly after being let go.
            if (returnTimer > 0)
            {
                returnTimer -= Time.deltaTime;
                if (returnTimer <= 0)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    transform.SetPositionAndRotation(homePosition, homeRotation);
                }
            }
        }

        void TryPlace()
        {
            if (Vector3.Distance(transform.position, homePosition) < 1f) return;
            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f)) return;
            if (hit.collider.name != "Ground") return;

            Vector3 cell = GridSnap.Snap(hit.point);
            if (!GridSnap.IsFree(cell)) return;
            if (!PlayerResources.Instance.Spend("Wool", woolCost)) return;

            Instantiate(sprinklerPrefab, cell, Quaternion.identity);
        }
    }
}
