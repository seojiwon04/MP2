using UnityEngine;
using TMPro;

public class FishDispenserUI : MonoBehaviour
{
    public FishDispenser dispenser;
    public TMP_Text weightText;

    public int weight = 5;

    void Start()
    {
        UpdateUI();
    }

    public void ChangeWeight(int amount)
    {
        weight = Mathf.Clamp(weight + amount, 1, 20);
        UpdateUI();
    }

    public void TakeFish()
    {
        dispenser.TakeFish(weight);
    }

    void UpdateUI()
    {
        weightText.text = $"{weight} / 20 kg";
    }
}
