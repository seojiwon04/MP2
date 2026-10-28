using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // Touch a sheep with the shears to take its wool.
    public class Shears : MonoBehaviour
    {
        public float reach = 0.3f;

        XRGrabInteractable grab;
        HashSet<Sheep> touching = new HashSet<Sheep>();
        HashSet<Sheep> wasTouching = new HashSet<Sheep>();

        void Start()
        {
            grab = GetComponent<XRGrabInteractable>();
        }

        void Update()
        {
            (wasTouching, touching) = (touching, wasTouching);
            touching.Clear();
            if (!grab.isSelected) return;

            foreach (Collider c in Physics.OverlapSphere(transform.position, reach))
            {
                Sheep sheep = c.GetComponentInParent<Sheep>();
                if (sheep == null || !touching.Add(sheep)) continue;

                // Only complain when the shears first touch the sheep, not every frame after.
                if (!sheep.Shear() && !wasTouching.Contains(sheep)) sheep.errorPopup.Show("No wool available for collection");
            }
        }
    }
}
