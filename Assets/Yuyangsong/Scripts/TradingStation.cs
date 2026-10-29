using UnityEngine;
using TMPro;

public class TradingStation : MonoBehaviour
{
    public int woodStock = 20;
    public int ironStock = 20;

    public int sellWeight = 1;
    public int buyAmount = 1;

    public int fishPrice = 1;
    public int fishMeatPrice = 2;
    public int woodPrice = 2;
    public int ironPrice = 4;

    public TMP_Text woodStockText;
    public TMP_Text ironStockText;

    public FishingInteractionFeedback feedback;

    public GameObject goalArrow;

    void Update()
    {
        woodStockText.text = $"Wood Stock: {woodStock}";
        ironStockText.text = $"Iron Stock: {ironStock}";
    }

    public void SellFish()
    {
        goalArrow.SetActive(false);

        var r = FishingResources.Instance;
        if (r == null || r.fishWeight < sellWeight) return;
        r.fishWeight -= sellWeight;
        r.coins += fishPrice * sellWeight;
        r.totalCoins += fishPrice * sellWeight;
    }

    public void SellFishMeat()
    {
        goalArrow.SetActive(false);

        var r = FishingResources.Instance;
        if (r == null || r.fishMeatWeight < sellWeight) return;
        r.fishMeatWeight -= sellWeight;
        r.coins += fishMeatPrice * sellWeight;
        r.totalCoins += fishMeatPrice * sellWeight;
    }

    public void BuyWood()
    {
        goalArrow.SetActive(false);

        var r = FishingResources.Instance;
        int cost = woodPrice * buyAmount;

        if (r == null) return;

        if (woodStock < buyAmount)
        {
            feedback.Show("Wood sold out");
            return;
        }

        if (r.coins < cost)
        {
            feedback.Show(
                $"Not enough Coins\nNeed: {cost} / Have: {r.coins}"
            );
            return;
        }

        if (!r.AddWood(buyAmount))
            return;

        r.coins -= cost;
        woodStock -= buyAmount;
    }

    public void BuyIron()
    {
        goalArrow.SetActive(false);
        
        var r = FishingResources.Instance;
        int cost = ironPrice * buyAmount;

        if (r == null) return;

        if (ironStock < buyAmount)
        {
            feedback.Show("Iron sold out");
            return;
        }

        if (r.coins < cost)
        {
            feedback.Show(
                $"Not enough Coins\nNeed: {cost} / Have: {r.coins}"
            );
            return;
        }

        if (!r.AddIron(buyAmount))
            return;

        r.coins -= cost;
        ironStock -= buyAmount;
    }
}
