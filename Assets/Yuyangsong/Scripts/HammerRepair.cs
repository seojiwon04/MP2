using UnityEngine;

public class HammerRepair : MonoBehaviour
{
    public float repairAmount = 10f;

    void OnTriggerEnter(Collider other)
    {
        FishingLiftNet net = other.GetComponent<FishingLiftNet>();

        if (net != null)
            net.Repair(repairAmount);
    }
}
