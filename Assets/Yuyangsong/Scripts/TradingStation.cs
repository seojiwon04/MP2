using UnityEngine;
using TMPro;

public class TradingStation : MonoBehaviour
{
    public int woodStock = 20;
    public int ironStock = 20;

    public float sellWeight = 5f;
    public int buyAmount = 5;

    public int fishPrice = 2;
    public int sharkPrice = 5;
    public int fishMeatPrice = 4;
    public int sharkMeatPrice = 10;
    public int woodPrice = 5;
    public int ironPrice = 10;

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
        r.coins += fishPrice;
    }

    public void SellShark()
    {
        var r = FishingResources.Instance;
        if (r == null || r.sharkWeight < sellWeight) return;
        r.sharkWeight -= sellWeight;
        r.coins += sharkPrice;
    }

    public void SellFishMeat()
    {
        var r = FishingResources.Instance;
        if (r == null || r.fishMeatWeight < sellWeight) return;
        r.fishMeatWeight -= sellWeight;
        r.coins += fishMeatPrice;
    }

    public void SellSharkMeat()
    {
        var r = FishingResources.Instance;
        if (r == null || r.sharkMeatWeight < sellWeight) return;
        r.sharkMeatWeight -= sellWeight;
        r.coins += sharkMeatPrice;
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
