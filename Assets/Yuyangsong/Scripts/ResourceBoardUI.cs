using UnityEngine;
using TMPro;

public class ResourceBoardUI : MonoBehaviour
{
    public TMP_Text fishText;
    public TMP_Text sharkText;
    public TMP_Text fishMeatText;
    public TMP_Text sharkMeatText;
    public TMP_Text woodText;
    public TMP_Text ironText;
    public TMP_Text coinsText;

    void Update()
    {
        FishingResources resources = FishingResources.Instance;
        if (resources == null)
            return;

        fishText.text = $"Fish: {resources.fishWeight:F1} kg";
        sharkText.text = $"Shark: {resources.sharkWeight:F1} kg";    
        fishMeatText.text = $"Fish Meat: {resources.fishMeatWeight:F1} kg";
        sharkMeatText.text = $"Shark Meat: {resources.sharkMeatWeight:F1} kg";
        woodText.text = $"Wood: {resources.wood}";
        ironText.text = $"Iron: {resources.iron}";
        coinsText.text = $"Coins: {resources.coins}";
    }
}
