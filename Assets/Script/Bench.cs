using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;

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

    // Track pending grab attempts
    private HashSet<int> _pendingGrabs = new HashSet<int>();
    private bool _isFullyGrabbed = false;

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

    protected override void Update()
    {
        base.Update();

        // Monitor grab state and enforce two-hand requirement
        if (_isFullyGrabbed && SelectingPointsCount < _minHandsRequired)
        {
            // One hand released - force full release
            _isFullyGrabbed = false;
            _pendingGrabs.Clear();
            ShowNeedTwoHandsFeedback(false);
        }
    }

    public override void ProcessPointerEvent(PointerEvent evt)
    {
        int pointerId = evt.Identifier;

        if (evt.Type == PointerEventType.Select)
        {
            // Track this grab attempt
            _pendingGrabs.Add(pointerId);

            // Check if we now have enough hands
            if (_pendingGrabs.Count >= _minHandsRequired)
            {
                // We have enough hands - allow the grab
                _isFullyGrabbed = true;
                ShowNeedTwoHandsFeedback(false);
                base.ProcessPointerEvent(evt);
            }
            else
            {
                // Not enough hands yet - show feedback but don't grab
                ShowNeedTwoHandsFeedback(true);
                // Don't call base - prevents single-hand grab
            }
        }
        else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
        {
            // Remove from pending grabs
            _pendingGrabs.Remove(pointerId);

            // If we're fully grabbed, process the unselect
            if (_isFullyGrabbed)
            {
                base.ProcessPointerEvent(evt);

                // If this drops us below minimum, force full release
                if (_pendingGrabs.Count < _minHandsRequired)
                {
                    _isFullyGrabbed = false;
                    _pendingGrabs.Clear();
                }
            }

            ShowNeedTwoHandsFeedback(false);
        }
        else
        {
            // For other events (Hover, Move, etc.), only process if fully grabbed
            if (_isFullyGrabbed || evt.Type == PointerEventType.Hover || evt.Type == PointerEventType.Unhover)
            {
                base.ProcessPointerEvent(evt);
            }
        }
    }

    private void ShowNeedTwoHandsFeedback(bool show)
    {
        if (_needTwoHandsFeedback != null)
        {
            _needTwoHandsFeedback.SetActive(show);
        }
    }
}