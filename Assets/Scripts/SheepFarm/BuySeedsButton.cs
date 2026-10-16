using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SheepFarm
{
    // A button on the Trading Post: select it to trade Wool for Seeds.
    public class BuySeedsButton : MonoBehaviour
    {
        public float woolCost = 1;
        public float seedsGiven = 3;

        void Start()
        {
            GetComponent<XRSimpleInteractable>().selectEntered.AddListener(args => Buy());
        }

        void Buy()
        {
            if (PlayerResources.Instance.Spend("Wool", woolCost))
                PlayerResources.Instance.Add("Seeds", seedsGiven);
        }
    }
}
