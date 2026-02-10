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
        Debug.Log("STARTING");
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
        // Ici tu peux ajouter un son ou une animation pour la descente si tu veux.
    }

    void OnPushUpUp()
    {
        Debug.Log("up");

        // Quand on remonte après être passé en bas, on considère que la pompe est validée.
        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.AddFollowers(1);
        }
        else
        {
            Debug.LogWarning("FollowersManager.Instance est null : aucun gestionnaire de followers trouvé dans la scène.");
        }
    }

}
