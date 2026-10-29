using UnityEngine;

public class TridentSharpener : MonoBehaviour
{
    public float sharpenAmount = 0.2f;

    public GameObject goalArrow;

    void OnTriggerEnter(Collider other)
    {
        TridentStats trident = other.GetComponent<TridentStats>();

        if (trident != null)
        {
            trident.Sharpen(sharpenAmount);
            goalArrow.SetActive(false);
        }
    }
}
