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

    void Update()
    {
        woodStockText.text = $"Wood Stock: {woodStock}";
        ironStockText.text = $"Iron Stock: {ironStock}";
    }

    public void SellFish()
    {
        var r = FishingResources.Instance;
        if (r == null || r.fishWeight < sellWeight) return;
        r.fishWeight -= sellWeight;
        r.coins += fishPrice * sellWeight;
    }

    public void SellFishMeat()
    {
        var r = FishingResources.Instance;
        if (r == null || r.fishMeatWeight < sellWeight) return;
        r.fishMeatWeight -= sellWeight;
        r.coins += fishMeatPrice * sellWeight;
    }

    public void BuyWood()
    {
        var r = FishingResources.Instance;
        int cost = woodPrice * buyAmount;

        if (r == null || woodStock < buyAmount || r.coins < cost)
            return;

        r.coins -= cost;
        r.wood += buyAmount;
        woodStock -= buyAmount;
    }

    public void BuyIron()
    {
        var r = FishingResources.Instance;
        int cost = ironPrice * buyAmount;

        if (r == null || ironStock < buyAmount || r.coins < cost)
            return;

        r.coins -= cost;
        r.iron += buyAmount;
        ironStock -= buyAmount;
    }
}
