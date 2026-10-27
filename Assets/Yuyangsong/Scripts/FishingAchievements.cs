using UnityEngine;
using TMPro;

public class FishingAchievements : MonoBehaviour
{
    public TradingStation shop;

    public GameObject firstCatch;
    public GameObject fishingMaster;
    public GameObject bladeMaster;
    public GameObject heavyPurse;
    public GameObject soldOut;

    public TMP_Text firstCatchProgress;
    public TMP_Text fishingProgress;
    public TMP_Text bladeProgress;
    public TMP_Text coinsProgress;
    public TMP_Text shopProgress;

    private int initialStock;

    void Start()
    {
        initialStock = shop.woodStock + shop.ironStock;
    }

    void Update()
    {
        FishingResources r = FishingResources.Instance;
        if (r == null) return;

        firstCatchProgress.text = $"{(r.totalFish > 0 ? 1 : 0)} / 1";

        fishingProgress.text =
            $"{Mathf.Min(r.totalFish, 100)} / 100";

        bladeProgress.text =
            $"{Mathf.Min(r.totalFishMeat, 100)} / 100";

        coinsProgress.text =
            $"{Mathf.Min(r.totalCoins, 100)} / 100";

        int purchased = initialStock - shop.woodStock - shop.ironStock;
        shopProgress.text = $"{purchased} / {initialStock}";

        if (r.totalFish > 0)
            firstCatch.SetActive(true);

        if (r.totalFish >= 100)
            fishingMaster.SetActive(true);

        if (r.totalFishMeat >= 100)
            bladeMaster.SetActive(true);

        if (r.totalCoins >= 100)
            heavyPurse.SetActive(true);

        if (shop.woodStock == 0 && shop.ironStock == 0)
            soldOut.SetActive(true);
    }
}