using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [Header("Unlockable Content")]
    public GameObject seedShop;
    public GameObject sprinklerShop;
    public GameObject integrationObject;

    private bool seedUnlocked = false;
    private bool sprinklerUnlocked = false;
    private bool integrationUnlocked = false;

    void Start()
    {
        if (seedShop != null)
            seedShop.SetActive(false);

        if (sprinklerShop != null)
            sprinklerShop.SetActive(false);

        if (integrationObject != null)
            integrationObject.SetActive(false);
    }

    public void NotifyWoodCollected(int totalWoodCollected)
    {
        if (!seedUnlocked && totalWoodCollected >= 1)
        {
            seedUnlocked = true;
            seedShop.SetActive(true);

            Debug.Log("Seed shop unlocked!");
        }
    }

    public void NotifyTreePlanted()
    {
        if (!sprinklerUnlocked)
        {
            sprinklerUnlocked = true;
            sprinklerShop.SetActive(true);

            Debug.Log("Sprinkler shop unlocked!");
        }
    }

    public void NotifySprinklerPlaced()
    {
        if (!integrationUnlocked)
        {
            integrationUnlocked = true;

            if (integrationObject != null)
                integrationObject.SetActive(true);

            Debug.Log("Integration unlocked!");
        }
    }
}
