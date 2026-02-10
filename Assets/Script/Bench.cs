using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Configures a bench to be grabbable only with two hands.
/// Add this alongside the Grabbable component.
/// </summary>
[RequireComponent(typeof(Grabbable))]
public class Bench : MonoBehaviour
{
    private Grabbable _grabbable;

    [Header("Two-Hand Grab Settings")]
    [SerializeField]
    [Tooltip("The Two Grab Transformer to use (e.g., TwoGrabRotateTransformer, TwoGrabFreeTransformer)")]
    private Component _twoGrabTransformer;

    void Awake()
    {
        _grabbable = GetComponent<Grabbable>();
        ConfigureTwoHandGrab();
    }

    private void ConfigureTwoHandGrab()
    {
        if (_grabbable == null) return;

        // Set to only allow 2 grab points (no more, no less for transformation)
        _grabbable.MaxGrabPoints = 2;

        // Inject the two-grab transformer if provided
        if (_twoGrabTransformer != null && _twoGrabTransformer is ITransformer)
        {
            _grabbable.InjectOptionalTwoGrabTransformer(_twoGrabTransformer as ITransformer);
        }

        // Don't inject a one-grab transformer - this prevents single-hand grabbing
        // from actually moving the object
    }
}