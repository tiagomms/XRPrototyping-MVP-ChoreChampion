using UnityEngine;

public class BasketTriggerDetector : MonoBehaviour
{
    [SerializeField] private AudioSource triggerSound;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Object entered trigger: {other.gameObject.name}");

        Debug.Log($"{other.gameObject.name} fell into the basket!");

        if (triggerSound != null)
        {
            triggerSound.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Object exited trigger: {other.gameObject.name}");
    }

    private void OnTriggerStay(Collider other)
    {
    }
}