using TMPro;
using UnityEngine;

public class FishingInteractionFeedback : MonoBehaviour
{
    public GameObject feedbackCanvas;
    public TMP_Text message;

    void Start() => Hide();

    public void Show(string text, float seconds = 3f)
    {
        CancelInvoke(nameof(Hide));
        message.text = text;
        feedbackCanvas.SetActive(true);
        Invoke(nameof(Hide), seconds);
    }

    void Hide() => feedbackCanvas.SetActive(false);
}