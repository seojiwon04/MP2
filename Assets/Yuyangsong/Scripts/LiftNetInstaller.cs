using UnityEngine;

public class LiftNetInstaller : MonoBehaviour
{
    public GameObject liftNetPrefab, sandboxCanvas;
    public Transform spawnPoint;
    public int woodCost = 5;
    public int ironCost = 5;
    
    public FishingInteractionFeedback feedback;

    public FishingInteractionFeedback sandboxFeedback;

    public GameObject goalArrow;

    private bool isSandbox;

    private GameObject currentNet;

    public void StartSandbox()
    {
        isSandbox = true;
        sandboxCanvas.SetActive(true);
        goalArrow.SetActive(true);
    }

    public void InstallNet()
    {
        if (currentNet != null) return;

        if (!isSandbox)
        {
            if (!FishingProgression.Instance.LiftNetBuildingUnlocked)
                return;

            FishingResources r = FishingResources.Instance;

            if (!r.SpendMaterials(woodCost, ironCost))
            {
                feedback.Show(
                    $"Not enough resources\n" +
                    $"Need: Wood {woodCost} / Have: Wood {r.GetWood()}\n" +
                    $"Need: Iron {ironCost} / Have: Iron {r.GetIron()}"
                );
                return;
            }
        }

        currentNet = Instantiate(
            liftNetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (isSandbox)
        {
            goalArrow.SetActive(false);

            FishingLiftNet net = currentNet.GetComponent<FishingLiftNet>();
            net.sandboxOwner = this;
            net.catchRate = 2.5f;
            net.durabilityLossRate = 5f;
        }
    }

    public void CompleteSandbox()
    {
        isSandbox = false;
        sandboxCanvas.SetActive(false);
        Destroy(currentNet);
        FishingProgression.Instance.OnLiftNetSandboxCompleted();
        sandboxFeedback.Show(
            "Practice complete!\nLift Net building unlocked!",
            4f
        );
    }
}
