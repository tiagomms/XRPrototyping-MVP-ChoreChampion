using UnityEngine;

namespace Chores
{
    public class Basket : MonoBehaviour
    {
        [SerializeField] private LaundryToss laundryToss;
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private GameObject scoreVisual;
        [SerializeField] private Transform scoreVisualPosition;
        [SerializeField] private float ttl = 2f;

        public void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Marker")) return;
            particleSystem.Play();
            audioSource.Play();
            laundryToss.AddScore();
            var score = Instantiate(scoreVisual, scoreVisualPosition.position, Quaternion.identity);
            Destroy(score, ttl);
        }
    }
}