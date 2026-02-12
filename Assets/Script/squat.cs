using UnityEngine;
using Oculus.Interaction;

public class Squat : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Tag of the floor object to detect (e.g., 'Floor')")]
    public string floorTag = "Floor";

    [Tooltip("Maximum distance to raycast downward (in meters)")]
    public float raycastDistance = 2f;

    [Tooltip("Small upward offset applied to the ray origin to avoid hitting self-colliders when inside geometry")]
    public float originOffset = 0.05f;

    [Tooltip("Distance threshold for 'down' position (in meters)")]
    public float downThreshold = 0.3f;

    [Tooltip("Distance threshold for 'up' position (in meters)")]
    public float upThreshold = 0.6f;

    [Header("Reward Settings")]
    [Tooltip("Minimum time in seconds between two follower rewards")]
    public float minTimeBetweenRewards = 1.0f;

    [Header("Grab Detection")]
    [SerializeField] private Grabbable grabbable;

    [Header("Debug Settings")]
    [Tooltip("Show debug ray in Scene view")]
    public bool showDebugRay = true;

    [Header("Floor Filter")]
    [Tooltip("Layer mask used to recognize the floor. If empty (Nothing), tag matching will be used.")]
    public LayerMask floorLayer = ~0;

    // State tracking
    private bool isDown = false;
    private float currentDistance = Mathf.Infinity;
    private bool hasRewardedThisUp = false;
    private float lastRewardTime = Mathf.NegativeInfinity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("STARTING");

        if (grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        DetectSquatPosition();
    }

    void DetectSquatPosition()
    {
        // Use RaycastAll and ignore collisions with this object (or its children).
        Vector3 rayOrigin = transform.position + Vector3.up * originOffset;
        Vector3 rayDirection = Vector3.down; // downward raycast for squat detection

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, raycastDistance);

        if (hits == null || hits.Length == 0)
        {
            // No hit detected
            currentDistance = Mathf.Infinity;
            if (showDebugRay)
            {
                Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red);
            }
            return;
        }

        // Sort hits by distance (closest first)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit nearestNonSelfHit = hits[0];
        bool foundFloor = false;
        RaycastHit floorHit = new RaycastHit();

        // Cache references for faster checks
        GameObject selfGO = gameObject;
        Rigidbody selfRb = GetComponent<Rigidbody>();

        foreach (var h in hits)
        {
            if (h.collider == null) continue;

            // Ignore colliders that are this object or its children
            if (h.collider.gameObject == selfGO || h.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            // Also ignore if collider belongs to the same Rigidbody (common on grabbable prefabs)
            if (selfRb != null && h.collider.attachedRigidbody == selfRb)
            {
                continue;
            }

            nearestNonSelfHit = h;

            bool tagMatches = h.collider.CompareTag(floorTag);
            bool layerMatches = (floorLayer.value & (1 << h.collider.gameObject.layer)) != 0;

            if (tagMatches || layerMatches)
            {
                floorHit = h;
                foundFloor = true;
                break;
            }
        }

        if (foundFloor)
        {
            currentDistance = floorHit.distance;

            if (showDebugRay)
            {
                Debug.DrawRay(rayOrigin, rayDirection * floorHit.distance, Color.green);
            }

            // Check for down position
            if (!isDown && currentDistance <= downThreshold)
            {
                isDown = true;
                hasRewardedThisUp = false; // reset reward when going down
                OnSquatDown();
            }
            // Check for up position
            else if (isDown && currentDistance >= upThreshold)
            {
                isDown = false;

                if (!hasRewardedThisUp)
                {
                    hasRewardedThisUp = true;
                    OnSquatUp();
                }
            }
        }
        else
        {
            // Hit something else but not the floor (likely the grabbed object)
            currentDistance = Mathf.Infinity;
            if (showDebugRay)
            {
                Debug.DrawRay(rayOrigin, rayDirection * nearestNonSelfHit.distance, Color.yellow);
                // Extra debug: log the collider name once to help identify what blocks the ray
                Debug.Log("Squat ray hit (non-floor): " + nearestNonSelfHit.collider.name + " (layer=" + LayerMask.LayerToName(nearestNonSelfHit.collider.gameObject.layer) + ")");
            }
        }
    }

    void OnSquatDown()
    {
        Debug.Log("Squat down");
        // Ici tu peux ajouter un son ou une animation pour la descente si tu veux.
    }

    void OnSquatUp()
    {
        Debug.Log("Squat up");

        // Ne gagne des followers que si l'objet est saisi.
        if (grabbable == null || grabbable.SelectingPointsCount <= 0)
        {
            return;
        }

        // Quand on remonte après être passé en bas, on considère que la pompe est validée.
        // Ajoute un délai minimum entre deux récompenses pour éviter
        // de gagner des followers trop rapidement si la hauteur oscille.
        if (Time.time - lastRewardTime < minTimeBetweenRewards)
        {
            return;
        }

        lastRewardTime = Time.time;

        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.AddFollowers(30);
            Debug.Log("+30 followers !!!!!");
        }
        else
        {
            Debug.LogWarning("FollowersManager.Instance est null : aucun gestionnaire de followers trouvé dans la scène.");
        }
    }

}
