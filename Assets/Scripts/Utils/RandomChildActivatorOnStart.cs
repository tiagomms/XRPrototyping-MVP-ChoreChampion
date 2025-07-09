using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Utility for hiding all first-level child GameObjects and randomly showing one.
    /// </summary>
    public class RandomChildActivatorOnStart : MonoBehaviour
    {
        // class that runs the child single activator
        private void Start() 
        {
            RandomChildActivator.ActivateRandomChild(transform);
        }
    }
}