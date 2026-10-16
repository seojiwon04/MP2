using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // Touch a sheep with the shears to take its wool.
    public class Shears : MonoBehaviour
    {
        public float reach = 0.3f;

        XRGrabInteractable grab;

        void Start()
        {
            grab = GetComponent<XRGrabInteractable>();
        }

        void Update()
        {
            if (!grab.isSelected) return;

            foreach (Collider c in Physics.OverlapSphere(transform.position, reach))
            {
                Sheep sheep = c.GetComponentInParent<Sheep>();
                if (sheep != null) sheep.Shear();
            }
        }
    }
}
