using UnityEngine;
using UnityEngine.Events;

public class HandMapper : MonoBehaviour
{
    public OVRHand handController;

    private bool _isPinchingPressed;
    public UnityEvent onPinchingReleased;

    private void Update()
    {
        bool isPinching = handController.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (_isPinchingPressed && !isPinching)
        {
            _isPinchingPressed = false;
            OnPitchingReleased();
        }

        if (isPinching)
        {
            _isPinchingPressed = true;
        }
    }

    private void OnPitchingReleased()
    {
        onPinchingReleased.Invoke();
        Debug.Log("Hand stopped pinching");
    }
}