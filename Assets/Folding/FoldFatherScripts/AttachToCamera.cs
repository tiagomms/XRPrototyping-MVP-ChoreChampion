using UnityEngine;

public class AttachToCamera: MonoBehaviour
{
    [SerializeField] private GameObject objectToShow;  
    [SerializeField] private float distanceFromCamera = 2f;

    void Start()
    {
        objectToShow.SetActive(false);
        if (objectToShow == null)
        {
            Debug.LogError("AttachAndShow: objectToShow not set");
            return;
           
        }

        // 1. Activate it
        objectToShow.SetActive(true);

        // 2. Parent under the main camera
        Transform cam = Camera.main.transform;
        objectToShow.transform.SetParent(cam, worldPositionStays: false);

        // 3. Sit it right in front of the camera
        objectToShow.transform.localPosition = new Vector3(0f, 0f, distanceFromCamera);
        objectToShow.transform.localRotation = Quaternion.identity;
    }
}