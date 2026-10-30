using TMPro;
using UnityEngine;

public class FirstCivFeeder : MonoBehaviour
{
    public CivBuilder builder;
    public CivFeeder feeder;
    public HubInventory inventory;
    public TMP_Text hungerText;
    public Renderer firstCiv;
    public TMP_Text instructionsText;
    public GameObject floorToWool;
    public GameObject SheepFarm;
    public GameObject floorToFish;
    public GameObject FishRoom;

    public string foodResource = "Wood";
    public string foodName = "Wood";
    public int foodAmount = 2;

    public bool pauseHungerUntilFed = true;

    bool hungry = false;

    private void Start()
    {
        if (builder == null)
        {
            builder = GetComponent<CivBuilder>();
        }
        if (feeder == null)
        {
            feeder = GetComponent<CivFeeder>();
        }
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<HubInventory>();
        }
        if (firstCiv == null)
        {
            firstCiv = feeder.firstCiv;
        }

        if (pauseHungerUntilFed)
        {
            feeder.enabled = false;
        }

        MakeHungry();
    }

    private void MakeHungry()
    {
        hungry = true;

        Bounds top = firstCiv.bounds;
        hungerText.transform.position = new Vector3(top.center.x, top.max.y + 2.5f, top.center.z);
        hungerText.text = foodAmount + " " + foodName;
        hungerText.gameObject.SetActive(true);
    }

    public bool TryFeed(Renderer platform)
    {
        if (!hungry || platform != firstCiv)
        {
            return false;
        }
        if (!inventory.UseResource(foodResource, foodAmount))
        {
            builder.ShowError(platform, "Not enough resources!");
            return true;
        }

        hungry = false;
        hungerText.gameObject.SetActive(false);

        if (pauseHungerUntilFed)
        {
            feeder.enabled = true;
        }
        
        instructionsText.text = "Great! Keep in mind, civilizations will die if they are not fed in time.\nNow, try to build a road! First, obtain some wool and fish meat. With enough resources, click a road that stems from a civ.";
        floorToFish.SetActive(true);
        floorToWool.SetActive(true);
        SheepFarm.SetActive(true);
        FishRoom.SetActive(true);
        return true;
    }
}