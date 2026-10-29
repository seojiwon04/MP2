using TMPro;
using UnityEngine;

namespace SheepFarm
{
    // Small red text placed by an object in the scene, e.g. "Not enough resources!".
    // Hidden when the game starts; Show() makes it appear for 1.5 seconds.
    public class ErrorPopup : MonoBehaviour
    {
        public float lifetime = 1.5f;

        TextMeshPro text;
        float timer;

        void Awake()
        {
            text = GetComponent<TextMeshPro>();
            gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            text.text = message;
            timer = lifetime;
            gameObject.SetActive(true);
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0) gameObject.SetActive(false);
        }
    }
}
