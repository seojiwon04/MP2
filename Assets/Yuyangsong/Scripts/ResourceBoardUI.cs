using UnityEngine;
using TMPro;

public class ResourceBoardUI : MonoBehaviour
{
    public TMP_Text fishText;
    public TMP_Text fishMeatText;
    public TMP_Text woodText;
    public TMP_Text ironText;
    public TMP_Text coinsText;

    void Update()
    {
        FishingResources resources = FishingResources.Instance;
        if (resources == null)
            return;

        fishText.text = $"Fish: {resources.fishWeight} kg";  
        fishMeatText.text = $"Fish Meat: {resources.fishMeatWeight} kg";
        woodText.text = $"Wood: {resources.wood}";
        ironText.text = $"Iron: {resources.iron}";
        coinsText.text = $"Coins: {resources.coins}";
    }
}
