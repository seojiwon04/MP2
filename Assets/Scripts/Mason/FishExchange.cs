using UnityEngine;
using TMPro;

public class FishExchange : MonoBehaviour
{
    [Header("Temporary Fish Resource")]
    public int fish = 5;

    [Header("Exchange")]
    public int fishCost = 1;
    public int woodReward = 1;

    [Header("UI")]
    public TMP_Text fishText;

    private PlayerResources playerResources;
    private ErrorPopupManager errorPopup;

    void Start()
    {
        playerResources =
            FindFirstObjectByType<PlayerResources>();

        errorPopup =
            FindFirstObjectByType<ErrorPopupManager>();

        UpdateUI();
    }

    public void TradeFishForWood()
    {
        if (fish < fishCost)
        {
            if (errorPopup != null)
            {
                errorPopup.ShowError(
                    "You need more Fish to trade for Wood."
                );
            }

            return;
        }

        fish -= fishCost;

        if (playerResources != null)
        {
            playerResources.AddWood(woodReward);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (fishText != null)
        {
            fishText.text = "Fish: " + fish;
        }
    }
}