using UnityEngine;

public class Trigger : MonoBehaviour
{
    
    public TutorialPopup tutorialPopup;
    public Transform popupPoint;

    [TextArea]
    public string message;

    public float triggerDistance = 2f;

    private Transform playerCamera;
    private bool hasShown = false;

    void Start()
    {
        FindPlayerCamera();
    }

    void Update()
    {
        if (hasShown)
            return;

        if (playerCamera == null)
        {
            FindPlayerCamera();
            return;
        }

        float distance = Vector3.Distance(
            playerCamera.position,
            transform.position
        );

        if (distance <= triggerDistance)
        {
            hasShown = true;

            tutorialPopup.ShowTutorial(
                message,
                popupPoint
            );
        }
    }

    private void FindPlayerCamera()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }
}