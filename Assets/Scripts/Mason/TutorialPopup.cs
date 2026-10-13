using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialPopup : MonoBehaviour
{
    public GameObject popupObject;
    public TMP_Text popupText;
    public float displayTime = 5f;

    public void ShowTutorial(string message, Transform popupPoint)
    {
        popupText.text = message;

        popupObject.transform.position = popupPoint.position;
        popupObject.transform.rotation = popupPoint.rotation;

        popupObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        popupObject.SetActive(false);
    }
}