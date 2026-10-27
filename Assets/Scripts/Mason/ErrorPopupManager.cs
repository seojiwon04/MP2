using UnityEngine;
using TMPro;
using System.Collections;

public class ErrorPopupManager : MonoBehaviour
{
    public GameObject popupObject;
    public TMP_Text popupText;
    public float displayTime = 3f;

    void Start()
    {
        if (popupObject != null)
            popupObject.SetActive(false);
    }

    public void ShowError(string message)
    {
        if (popupObject == null || popupText == null)
            return;

        popupText.text = message;
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