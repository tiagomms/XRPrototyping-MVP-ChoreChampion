using UnityEngine;

namespace Chores
{
    public class Basket : MonoBehaviour
    {
        [SerializeField] private LaundryToss laundryToss;
        [SerializeField] private ParticleSystem particleSystem;

        public void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Marker")) return;
            particleSystem.Play();
            laundryToss.AddScore();
        }
    }
}