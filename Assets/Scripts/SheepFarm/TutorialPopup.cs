using TMPro;
using UnityEngine;

namespace SheepFarm
{
    // Shows a line of text above this object once, the first time the player comes close.
    public class TutorialPopup : MonoBehaviour
    {
        public string message = "";
        public float showDistance = 2f;
        public float height = 0.5f;
        public float showTime = 6f;

        TextMeshPro text;
        bool shown;
        float hideTime;

        void Start()
        {
            GameObject go = new GameObject("Tutorial Popup");
            text = go.AddComponent<TextMeshPro>();
            text.text = message;
            text.fontSize = 1;
            text.color = Color.yellow;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(2f, 0.5f);
            go.SetActive(false);
        }

        void Update()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            if (!shown && Vector3.Distance(cam.transform.position, transform.position) < showDistance)
            {
                shown = true;
                hideTime = Time.time + showTime;
                text.gameObject.SetActive(true);
            }

            if (text.gameObject.activeSelf)
            {
                text.transform.position = transform.position + Vector3.up * height;
                text.transform.rotation = Quaternion.LookRotation(text.transform.position - cam.transform.position);
                if (Time.time > hideTime) text.gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (text != null) Destroy(text.gameObject);
        }
    }
}
