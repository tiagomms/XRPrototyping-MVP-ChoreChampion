using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Utility for hiding all first-level child GameObjects and randomly showing one.
    /// </summary>
    public static class RandomChildActivator
    {
        private static readonly System.Random random = new System.Random();

        /// <summary>
        /// Hides all first-level child GameObjects of the given parent, then randomly shows one.
        /// </summary>
        /// <param name="parent">The parent Transform whose children will be affected.</param>
        public static GameObject ActivateRandomChild(Transform parent)
        {
            int childCount = parent.childCount;
            if (childCount == 0)
            {
                Debug.LogWarning("RandomChildActivator: No child GameObjects found.");
                return null;
            }

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }

            int randomIndex = random.Next(0, childCount);
            Transform selectedChild = parent.GetChild(randomIndex);
            selectedChild.gameObject.SetActive(true);
            return selectedChild.gameObject;
        }
    }
}