using UnityEngine;

public class FishingGoalArrow : MonoBehaviour
{
    private Vector3 originalScale;

    void Awake() => originalScale = transform.localScale;

    void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);

        transform.localScale = originalScale *
            (1f + Mathf.Sin(Time.time * Mathf.PI) * 0.1f);
    }
}