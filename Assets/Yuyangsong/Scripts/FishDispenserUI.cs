using UnityEngine;
using TMPro;

public class FishDispenserUI : MonoBehaviour
{
    public FishDispenser dispenser;
    public TMP_Text typeText;
    public TMP_Text weightText;

    public FishStats.FishType fishType = FishStats.FishType.Fish;
    public int weight = 10;

    void Start()
    {
        UpdateUI();
    }

    public void SwitchType()
    {
        fishType = fishType == FishStats.FishType.Fish
            ? FishStats.FishType.Shark
            : FishStats.FishType.Fish;
        UpdateUI();
    }

    public void ChangeWeight(int amount)
    {
        weight = Mathf.Clamp(weight + amount, 0, 100);
        UpdateUI();
    }

    public void TakeFish()
    {
        dispenser.TakeFish(fishType, weight);
    }

    void UpdateUI()
    {
        typeText.text = fishType.ToString();
        weightText.text = $"{weight} / 100 kg";
    }
}
