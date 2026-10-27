using UnityEngine;

namespace SheepFarm
{
    // Turns this object to face the player's camera.
    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}
