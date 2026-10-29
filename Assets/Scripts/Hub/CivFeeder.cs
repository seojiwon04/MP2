using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CivFeeder : MonoBehaviour
{
    public CivBuilder builder;
    public HubInventory inventory;
    public Material deadMaterial;
    public TMP_Text hungerText;
    public Renderer firstCiv;

    public float timeBetweenHunger = 180;
    public float timeToFeed = 60;
    public int foodAmount = 2;

    public bool debugHunger = false; // turn off before submitting
    public float debugTimeBetweenHunger = 20;
    public float debugTimeToFeed = 10;

    string[] resources = { "Wood", "Wool", "FishMeat" };
    string[] resourceNames = { "Wood", "Wool", "Fish Meat" };

    float hungerTimer;
    float feedTimer;
    int hungryCiv = -1;
    string hungryResource;

    private void Start()
    {
        if (builder == null)
        {
            builder = GetComponent<CivBuilder>();
        }
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<HubInventory>();
        }

        if (debugHunger)
        {
            timeBetweenHunger = debugTimeBetweenHunger;
            timeToFeed = debugTimeToFeed;
            Debug.Log("CivFeeder: debug hunger on, hunger every " + timeBetweenHunger + "s, " + timeToFeed + "s to feed");
        }

        hungerText.gameObject.SetActive(false);
    }

    private void Update()
    {
        hungerTimer += Time.deltaTime;
        if (hungerTimer >= timeBetweenHunger)
        {
            hungerTimer = 0;
            MakeHungry();
        }

        if (hungryCiv != -1)
        {
            feedTimer -= Time.deltaTime;
            if (feedTimer <= 0)
            {
                KillCiv(hungryCiv);
            }
        }
    }

    private bool IsBuilt(Renderer target)
    {
        return target.sharedMaterial == builder.aliveMaterial;
    }

    private void MakeHungry()
    {
        if (hungryCiv != -1)
        {
            return;
        }

        List<int> aliveCivs = new List<int>();
        for (int i = 0; i < builder.platforms.Length; i++)
        {
            if (IsBuilt(builder.platforms[i]) && builder.platforms[i] != firstCiv)
            {
                aliveCivs.Add(i);
            }
        }
        if (aliveCivs.Count == 0)
        {
            return;
        }

        hungryCiv = aliveCivs[Random.Range(0, aliveCivs.Count)];
        int food = Random.Range(0, resources.Length);
        hungryResource = resources[food];
        feedTimer = timeToFeed;

        Bounds top = builder.platforms[hungryCiv].bounds;
        hungerText.transform.position = new Vector3(top.center.x, top.max.y + 2.5f, top.center.z);
        hungerText.text = foodAmount + " " + resourceNames[food];
        hungerText.gameObject.SetActive(true);
    }

    public bool TryFeed(Renderer platform)
    {
        int index = System.Array.IndexOf(builder.platforms, platform);
        if (index != hungryCiv)
        {
            return false;
        }
        if (!inventory.UseResource(hungryResource, foodAmount))
        {
            builder.ShowError(platform, "Not enough resources!");
            return true;
        }

        hungryCiv = -1;
        hungerText.gameObject.SetActive(false);
        return true;
    }

    private void KillCiv(int index)
    {
        builder.platforms[index].sharedMaterial = deadMaterial;

        if (builder.civs[index] != null)
        {
            Destroy(builder.civs[index]);
            builder.civs[index] = null;
        }

        foreach (CivBuilder.Road road in builder.roads)
        {
            if (IsBuilt(road.road) && !IsBuilt(road.civA) && !IsBuilt(road.civB))
            {
                road.road.sharedMaterial = deadMaterial;
            }
        }

        hungryCiv = -1;
        hungerText.gameObject.SetActive(false);
    }
}
