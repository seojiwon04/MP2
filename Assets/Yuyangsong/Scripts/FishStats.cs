using UnityEngine;

public class FishStats : MonoBehaviour
{   
    public enum FishType { Fish, Shark }

    public FishType fishType = FishType.Fish;
    public float growth = 0f;
    public float growthRate = 0.02f;
    public float baseWeight = 1f;

    public int Weight => Mathf.FloorToInt(baseWeight * (1f + 9f * growth));

    public bool canGrow = true;

    private Vector3 initialScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialScale = transform.localScale / 10f;
        transform.localScale = initialScale * (1f + 9f * growth);
    }

    // Update is called once per frame
    void Update()
    {
        if (!canGrow)
            return;
        
        growth = Mathf.MoveTowards(growth, 1f, growthRate * Time.deltaTime);
        transform.localScale = initialScale * (1f + 9f * growth);
    }
}
