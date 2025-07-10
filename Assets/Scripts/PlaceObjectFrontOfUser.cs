using UnityEngine;

public class PlaceObjectFrontOfUser : MonoBehaviour
{
    [SerializeField] private Transform userTransform;
    [SerializeField] private float distanceFromUser = 1.5f;
    [SerializeField] private float yDistance = 1f;

    private void Start()
    {
        PlaceObject();
    }

    private void PlaceObject()
    {
        if (userTransform == null)
        {
            Debug.LogError("User Transform is not assigned.");
            return;
        }

        Vector3 position = userTransform.position + userTransform.forward * distanceFromUser + Vector3.up * yDistance;
        transform.position = position;
        transform.rotation = userTransform.rotation;
    }
}