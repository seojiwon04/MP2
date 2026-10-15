using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
    public GameObject tutorialPanel;

    public void Toggle()
    {
        tutorialPanel.SetActive(!tutorialPanel.activeSelf);
    }
}
