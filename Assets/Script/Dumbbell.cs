using UnityEngine;

public class Dumbbell : MonoBehaviour
{
    [Header("Movement Detection")]
    [SerializeField] private float velocityThreshold = 2.0f; // Adjust based on testing
    [SerializeField] private float cooldownTime = 0.5f; // Prevent multiple triggers

    private Rigidbody rb;
    private float lastTriggerTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Dumbbell needs a Rigidbody component!");
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Get the velocity magnitude (speed regardless of direction)
        float currentSpeed = rb.linearVelocity.magnitude;

        // Check if moving fast enough and cooldown has passed
        if (currentSpeed >= velocityThreshold && Time.time >= lastTriggerTime + cooldownTime)
        {
            OnQuickMovement(currentSpeed);
            lastTriggerTime = Time.time;
        }
    }

    private void OnQuickMovement(float speed)
    {
        Debug.Log($"Quick movement detected! Speed: {speed:F2} m/s");

        // Add your custom logic here
        // For example: play sound, add score, trigger animation, etc.
    }
}