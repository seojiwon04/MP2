using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SheepFarm
{
    // Holds the player's resources and shows them on the wrist text.
    public class PlayerResources : MonoBehaviour
    {
        public static PlayerResources Instance;

        public float maxWater = 100;

        Dictionary<string, float> amounts = new Dictionary<string, float>();
        TextMeshPro text;

        void Awake()
        {
            Instance = this;
            amounts["Water"] = 50;
            amounts["Wool"] = 0;
            amounts["Seeds"] = 3;
            amounts["Sheep"] = 0;
            amounts["Grass"] = 0;
            text = GetComponent<TextMeshPro>();
        }

        public float Get(string name)
        {
            return amounts.ContainsKey(name) ? amounts[name] : 0;
        }

        public void Add(string name, float amount)
        {
            amounts[name] = Get(name) + amount;
            if (name == "Water") amounts[name] = Mathf.Min(amounts[name], maxWater);
        }

        public bool Spend(string name, float amount)
        {
            if (Get(name) < amount) return false;
            amounts[name] -= amount;
            return true;
        }

        void Update()
        {
            text.text = "Water: " + Mathf.Floor(Get("Water")) + "/" + maxWater
                + "\nWool: " + Get("Wool")
                + "\nSeeds: " + Get("Seeds")
                + "\nSheep: " + Get("Sheep")
                + "\nGrass: " + Get("Grass");
        }
    }
}
