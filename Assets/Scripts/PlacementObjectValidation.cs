using Meta.XR.MRUtilityKit.BuildingBlocks;
using UnityEngine;
using UnityEngine.Events;

public class PlacementObjectValidation : MonoBehaviour
{
    [SerializeField] private SpaceLocator spaceLocator;

    public UnityEvent<Pose, bool> onSpaceLocateSuccess;
    public UnityEvent<Pose, bool> onSpaceLocateFailure;

    private void Start()
    {
        spaceLocator.OnSpaceLocateCompleted.AddListener(
            (pose, isValid) =>
            {
                if (isValid)
                {
                    onSpaceLocateSuccess?.Invoke(pose, true);
                }
                else
                {
                    onSpaceLocateFailure?.Invoke(pose, false);
                }
            });
    }
}