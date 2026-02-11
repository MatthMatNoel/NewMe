using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Makes an object grabbable only with two hands.
/// Extends the Meta Grabbable functionality to enforce two-hand interaction.
/// </summary>
public class TwoHandsOnlyGrabbable : Grabbable
{
    [Header("Two Hands Only Settings")]
    [SerializeField]
    [Tooltip("Minimum number of hands required before the object can be grabbed")]
    private int _minHandsRequired = 2;

    [SerializeField]
    [Tooltip("Optional visual feedback when trying to grab with only one hand")]
    private GameObject _needTwoHandsFeedback;

    protected override void Start()
    {
        base.Start();

        // Force max grab points to exactly 2
        MaxGrabPoints = 2;

        // Hide feedback initially
        if (_needTwoHandsFeedback != null)
        {
            _needTwoHandsFeedback.SetActive(false);
        }
    }

    public override void ProcessPointerEvent(PointerEvent evt)
    {
        // Check if we have enough hands before allowing interaction
        if (evt.Type == PointerEventType.Select)
        {
            if (SelectingPointsCount < _minHandsRequired - 1)
            {
                // Not enough hands yet - show feedback and block the grab
                ShowNeedTwoHandsFeedback(true);
                // DO NOT call base - this prevents grabbing with one hand
                return;
            }
            else
            {
                // We have enough hands now
                ShowNeedTwoHandsFeedback(false);
            }
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            ShowNeedTwoHandsFeedback(false);
        }

        base.ProcessPointerEvent(evt);
    }

    private void ShowNeedTwoHandsFeedback(bool show)
    {
        if (_needTwoHandsFeedback != null)
        {
            _needTwoHandsFeedback.SetActive(show);
        }
    }
}