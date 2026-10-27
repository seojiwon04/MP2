using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // Pick it up from the Trading Post. Trigger fills from the pond or sprays droplets.
    public class WateringCan : MonoBehaviour
    {
        public GameObject dropletPrefab;
        public float fillRate = 30;
        public float sprayCost = 0.5f;
        public float sprayInterval = 0.05f;
        public float sprayAngle = 35;
        public float rayLength = 4;
        public ErrorPopup errorPopup;

        XRGrabInteractable grab;
        Collider canCollider;
        Transform tilt;
        Transform spout;
        Transform gaugePivot;
        LineRenderer ray;
        float sprayTimer;
        bool triggerHeld;
        string lastError;

        void Start()
        {
            grab = GetComponent<XRGrabInteractable>();
            grab.activated.AddListener(args => triggerHeld = true);
            grab.deactivated.AddListener(args => triggerHeld = false);
            grab.selectExited.AddListener(args => triggerHeld = false);
            canCollider = GetComponent<Collider>();
            tilt = transform.Find("Tilt");
            spout = transform.Find("Tilt/Model/Spout");
            gaugePivot = transform.Find("GaugePivot");
            ray = GetComponent<LineRenderer>();
        }

        void Update()
        {
            PlayerResources res = PlayerResources.Instance;
            bool held = grab.isSelected;
            bool pressed = held && triggerHeld;

            // Aim ray (only while held): turns blue when pointing at the pond.
            Pond pond = null;
            Vector3 end = spout.position + spout.forward * rayLength;
            if (Physics.Raycast(spout.position, spout.forward, out RaycastHit hit, rayLength))
            {
                end = hit.point;
                pond = hit.collider.GetComponentInParent<Pond>();
            }
            ray.enabled = held;
            ray.SetPosition(0, spout.position);
            ray.SetPosition(1, end);
            ray.startColor = ray.endColor = pond != null ? Color.blue : Color.white;

            // Say why nothing happens, once each time it starts happening.
            string error = null;
            ErrorPopup errorAt = errorPopup;
            if (pressed && pond != null && pond.waterLevel.value < 1)
            {
                error = "No more water left";
                errorAt = pond.errorPopup;
            }
            else if (pressed && pond == null && res.Get("Water") < sprayCost) error = "Not enough water!";
            if (error != null && error != lastError) errorAt.Show(error);
            lastError = error;

            bool spraying = false;
            if (pressed && pond != null)
            {
                float room = res.maxWater - res.Get("Water");
                res.Add("Water", pond.Take(Mathf.Min(fillRate * Time.deltaTime, room)));
            }
            else if (pressed && res.Get("Water") >= sprayCost)
            {
                spraying = true;
                sprayTimer += Time.deltaTime;
                if (sprayTimer >= sprayInterval)
                {
                    sprayTimer = 0;
                    res.Spend("Water", sprayCost);
                    Spray();
                }
            }

            // Tilt while spraying, gauge shows how much water is in the can.
            tilt.localRotation = spraying ? Quaternion.Euler(sprayAngle, 0, 0) : Quaternion.identity;
            gaugePivot.localScale = new Vector3(1, Mathf.Max(0.02f, res.Get("Water") / res.maxWater), 1);
        }

        void Spray()
        {
            GameObject drop = Instantiate(dropletPrefab, spout.position, Quaternion.identity);
            Physics.IgnoreCollision(drop.GetComponent<Collider>(), canCollider); // don't splash on the can itself
            drop.GetComponent<Rigidbody>().linearVelocity = spout.forward * 3f + Random.insideUnitSphere * 0.3f;
        }
    }
}
