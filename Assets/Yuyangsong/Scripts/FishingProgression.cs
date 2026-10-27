using UnityEngine;

public class FishingProgression : MonoBehaviour
{
    public static FishingProgression Instance;

    public GameObject grindstoneRoot, tradingStationRoot;
    public int meatRequired = 10;
    public bool GrindstoneUnlocked { get; private set; }
    public bool TradingStationUnlocked { get; private set; }
    public bool LiftNetSandboxUnlocked { get; private set; }
    public bool LiftNetBuildingUnlocked { get; private set; }

    private int totalMeat;

    void Awake()
    {
        Instance = this;
        grindstoneRoot.SetActive(false);
        tradingStationRoot.SetActive(false);
    }

    public void OnTridentDepleted() 
    {
        if (GrindstoneUnlocked) return;
        GrindstoneUnlocked = true;
        grindstoneRoot.SetActive(true); 
    } 

    public void OnMeatCollected(int amount)
    {
        totalMeat += amount;
        if (TradingStationUnlocked) return;
        if (totalMeat >= meatRequired)
        {
            TradingStationUnlocked = true;
            tradingStationRoot.SetActive(true);
        }
    }

    public void OnDockBuilt(LiftNetInstaller installer)
    {
        if (LiftNetSandboxUnlocked) return;

        LiftNetSandboxUnlocked = true;
        installer.StartSandbox();
    }

    public void OnLiftNetSandboxCompleted()
    {
        LiftNetBuildingUnlocked = true;
    }
}