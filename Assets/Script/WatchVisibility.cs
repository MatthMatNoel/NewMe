using UnityEngine;

public class WatchVisibility : MonoBehaviour
{
// Drag the OVRHand component (left or right) here in inspector
    public OVRHand ovrHand; 
    private Renderer _renderer;
    private Collider _collider;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
    }

    void Update()
    {
        if (ovrHand != null)
        {
            // Check if the hand is actively tracked
            bool isVisible = ovrHand.IsTracked;
            
            // Set active status based on tracking
            if (_renderer.enabled != isVisible)
            {
                _renderer.enabled = isVisible;
                if (_collider != null) _collider.enabled = isVisible;
            }
        }
    }
}
