using UnityEngine;

public class Pushup : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tag of the floor object to detect (e.g., 'Floor')")]
    public string floorTag = "Floor";

    [Tooltip("Maximum distance to raycast forward (in meters)")]
    public float raycastDistance = 2f;

    [Tooltip("Distance threshold for 'down' position (in meters)")]
    public float downThreshold = 0.3f;

    [Tooltip("Distance threshold for 'up' position (in meters)")]
    public float upThreshold = 0.6f;

    [Header("Debug Settings")]
    [Tooltip("Show debug ray in Scene view")]
    public bool showDebugRay = true;

    // State tracking
    private bool isDown = false;
    private float currentDistance = Mathf.Infinity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DetectPushUpPosition();
    }

    void DetectPushUpPosition()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        // Cast ray forward from the headset
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance))
        {
            // Check if we hit the floor object
            if (hit.collider.CompareTag(floorTag))
            {
                currentDistance = hit.distance;

                // Debug visualization
                if (showDebugRay)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.green);
                }

                // Check for down position
                if (!isDown && currentDistance <= downThreshold)
                {
                    isDown = true;
                    OnPushUpDown();
                }
                // Check for up position
                else if (isDown && currentDistance >= upThreshold)
                {
                    isDown = false;
                    OnPushUpUp();
                }
            }
            else
            {
                // Hit something but not the floor
                if (showDebugRay)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.yellow);
                }
            }
        }
        else
        {
            // No hit detected
            currentDistance = Mathf.Infinity;
            if (showDebugRay)
            {
                Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red);
            }
        }
    }

    void OnPushUpDown()
    {
        Debug.Log("down");
        // Add any additional logic here (e.g., play sound, increment counter, etc.)
    }

    void OnPushUpUp()
    {
        Debug.Log("up");
        // Add any additional logic here (e.g., count as completed push-up, etc.)
    }

    // Optional: Display current distance in the inspector
    void OnGUI()
    {
        if (showDebugRay)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Distance to floor: {currentDistance:F2}m");
            GUI.Label(new Rect(10, 30, 300, 20), $"State: {(isDown ? "DOWN" : "UP")}");
        }
    }
}
