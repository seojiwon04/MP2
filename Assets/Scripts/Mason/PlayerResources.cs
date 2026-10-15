using UnityEngine;
using TMPro;

public class PlayerResources : MonoBehaviour
{
    public int wood = 0;

    public TMP_Text woodText;

    void Start()
    {
        UpdateUI();
    }

    public void AddWood(int amount)
    {
        wood += amount;
        UpdateUI();
    }

    public bool SpendWood(int amount)
    {
        if (wood < amount)
            return false;

        wood -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (woodText != null)
        {
            woodText.text = "WOOD: " + wood;
        }
    }
}