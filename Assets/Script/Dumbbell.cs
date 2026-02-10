using UnityEngine;

public class Dumbbell : MonoBehaviour
{
    [Header("Movement Detection")]
    [SerializeField] private float speedThreshold = 3.0f; // Meters per second
    [SerializeField] private float cooldownTime = 0.5f;

    private Vector3 previousPosition;
    private float lastTriggerTime;

    void Start()
    {
        previousPosition = transform.position;
    }

    void Update()
    {
        // Calculate speed based on position change
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);
        float speed = distanceMoved / Time.deltaTime;

        // Check if moving fast enough
        if (speed >= speedThreshold && Time.time >= lastTriggerTime + cooldownTime)
        {
            OnQuickMovement(speed);
            lastTriggerTime = Time.time;
        }

        // Update previous position for next frame
        previousPosition = transform.position;
    }

    private void OnQuickMovement(float speed)
    {
        Debug.Log($"Quick movement detected! Speed: {speed:F2} m/s");

        // Your custom logic here
    }
}