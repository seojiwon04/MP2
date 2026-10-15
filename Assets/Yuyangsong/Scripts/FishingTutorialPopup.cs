using UnityEngine;

public class FishingTutorialPopup : MonoBehaviour
{
    public GameObject tutorialPanel;

    public void Toggle()
    {
        tutorialPanel.SetActive(!tutorialPanel.activeSelf);
    }
}
