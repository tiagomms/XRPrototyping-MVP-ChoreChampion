using UnityEngine;

namespace Chores
{
    public class Basket : MonoBehaviour
    {
        [SerializeField] private LaundryToss laundryToss;

        private void OnCollisionEnter(Collision other)
        {
            laundryToss.AddScore();
        }
    }
}