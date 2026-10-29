using UnityEngine;

namespace SheepFarm
{
    // Hides an object behind this lock until a resource reaches the threshold,
    // then removes the lock (and its text) and shows the object.
    public class MasteryLock : MonoBehaviour
    {
        public GameObject lockedObject;
        public string resource = "Tilled";
        public float threshold = 5;

        void Awake()
        {
            lockedObject.SetActive(false);
        }

        void Update()
        {
            if (PlayerResources.Instance.Get(resource) < threshold) return;

            lockedObject.SetActive(true);
            Destroy(gameObject);
        }
    }
}
