using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class ShirtFolding : MonoBehaviour
{
    public Transform leftSleeveBone;
    public Transform rightSleeveBone;
    public float foldAngle = 90f;
    public float foldSpeed = 90f;

    private bool isFolding = false;
    private HandGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<HandGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.WhenPointerEventRaised += HandlePointerEvent;
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }

    private void HandlePointerEvent(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            isFolding = true;
        }
    }

    void Update()
    {
        if (!isFolding) return;

        leftSleeveBone.localRotation = Quaternion.RotateTowards(
            leftSleeveBone.localRotation,
            Quaternion.Euler(0, 0, foldAngle),
            foldSpeed * Time.deltaTime
        );

        rightSleeveBone.localRotation = Quaternion.RotateTowards(
            rightSleeveBone.localRotation,
            Quaternion.Euler(0, 0, -foldAngle),
            foldSpeed * Time.deltaTime
        );
    }
}