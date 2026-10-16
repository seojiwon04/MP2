using UnityEngine;

public class FishPath : MonoBehaviour
{   
    public float speed = 1f;
    public float turnSpeed = 120f;

    [Header("Path Sampling")]
    public Vector2 xRange = new Vector2(-15f, 15f);
    public Vector2 zRange = new Vector2(38.5f, 55f);
    public Vector2 depthRange = new Vector2(0.4f, 0.8f);
    public float waterHeight = -1f;
    public float firstWaypointZRange = 1.5f;
    public int waypointCount = 8;

    private Vector3[] waypoints;
    private int currentIndex;

    public Vector3[] SamplePath()
    {
        int count = Mathf.Max(2, waypointCount);
        Vector3[] path = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(xRange.x, xRange.y);
            float z = Random.Range(zRange.x, zRange.y);
            float y = waterHeight - Random.Range(depthRange.x, depthRange.y);

            if (i == 0)
                z = Random.Range(zRange.x, zRange.x + firstWaypointZRange);

            path[i] = new Vector3(x, y, z);
        }

        return path;
    }
    
    public void SetPath(Vector3[] path, int startIndex)
    {
        waypoints = path;
        currentIndex = (startIndex + 1) % path.Length;
    }

    // Update is called once per frame
    void Update()
    {   
        if (waypoints == null || waypoints.Length == 0)
            return;
        
        Vector3 direction = waypoints[currentIndex] - transform.position;

        if (direction.magnitude < 0.01f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, targetRotation) > 0f)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            waypoints[currentIndex],
            speed * Time.deltaTime
        );
    }
}
