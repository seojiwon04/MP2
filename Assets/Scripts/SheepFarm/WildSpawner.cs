using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SheepFarm
{
    // Scatters grass at the start, then keeps adding more every 10-30 seconds.
    public class WildSpawner : MonoBehaviour
    {
        public GameObject grassPrefab;
        public int startCount = 12;
        public Vector3 areaCenter = new Vector3(0, 0, 4);
        public Vector3 areaSize = new Vector3(20, 0, 14);
        public float minDelay = 10;
        public float maxDelay = 30;

        void Start()
        {
            for (int i = 0; i < startCount; i++) SpawnOne();
            StartCoroutine(SpawnLoop());
        }

        IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
                SpawnOne();
            }
        }

        void SpawnOne()
        {
            for (int tries = 0; tries < 10; tries++)
            {
                // Area is relative to this spawner, so the farm works wherever it's placed.
                Vector3 p = transform.TransformPoint(areaCenter + new Vector3(Random.Range(-0.5f, 0.5f) * areaSize.x, 0, Random.Range(-0.5f, 0.5f) * areaSize.z));
                if (!NavMesh.SamplePosition(p, out NavMeshHit hit, 0.5f, NavMesh.AllAreas)) continue;
                p = new Vector3(hit.position.x, transform.position.y, hit.position.z);
                if (!GridSnap.IsFree(p)) continue;

                GameObject g = Instantiate(grassPrefab, p, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                g.GetComponent<Grass>().growth.value = Random.Range(30f, 100f);
                return;
            }
        }
    }
}
