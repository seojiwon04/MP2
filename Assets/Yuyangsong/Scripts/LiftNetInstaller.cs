using UnityEngine;

public class LiftNetInstaller : MonoBehaviour
{
    public GameObject liftNetPrefab, sandboxCanvas;
    public Transform spawnPoint;
    public int woodCost = 5;
    public int ironCost = 5;
    
    public FishingInteractionFeedback feedback;

    private bool isSandbox;

    private GameObject currentNet;

    public void StartSandbox()
    {
        isSandbox = true;
        sandboxCanvas.SetActive(true);
    }

    public void InstallNet()
    {
        if (currentNet != null) return;

        if (!isSandbox)
        {
            if (!FishingProgression.Instance.LiftNetBuildingUnlocked)
                return;

            FishingResources r = FishingResources.Instance;

            if (r.wood < woodCost || r.iron < ironCost)
            {
                feedback.Show(
                    $"Not enough resources\nNeed: Wood {woodCost} / Have: Wood {r.wood}\nNeed: Iron {ironCost} / Have: Iron {r.iron}"
                );
                return;
            }

            r.wood -= woodCost;
            r.iron -= ironCost;
        }

        currentNet = Instantiate(
            liftNetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (isSandbox)
        {
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
    }
}
