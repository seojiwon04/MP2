using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // A button on the Trading Post: select it to trade Wool for a new Sheep.
    public class BuySheepButton : MonoBehaviour
    {
        public GameObject sheepPrefab;
        public Transform spawnPoint;
        public Transform flock;
        public float woolCost = 10;

        void Start()
        {
            GetComponent<XRSimpleInteractable>().selectEntered.AddListener(args => Buy());
        }

        void Buy()
        {
            if (!NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) return;
            if (!PlayerResources.Instance.Spend("Wool", woolCost)) return;

            // The sheep counts itself in PlayerResources when it starts.
            Instantiate(sheepPrefab, hit.position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), flock);
        }
    }
}
