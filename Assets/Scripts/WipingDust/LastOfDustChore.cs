using UnityEngine;
using LastOfDust.UI;
using ChoreChampion.XR.MRUtilityKit;
using System.Xml.Serialization;
using Meta.XR.MRUtilityKit;
using UnityEngine.Events;

namespace Chores
{
    public class LastOfDustChore : Chore
    {
        [SerializeField] private ExtendedAnchorPrefabSpawner xtdAnchorPrefabSpawner;

        /// <summary>
        /// Chores only exist in their own scene
        /// </summary>
        protected virtual void Awake()
        {
            // If an instance already exists and it's not this, destroy this object
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            Debug.Log("Starting Minigame for Example Chore");
        }

        public override void CompleteChore()
        {
            base.CompleteChore();
            Debug.Log("Example Chore completed!");
        }

        public override void EndChore()
        {
            onChoreEnded.Invoke();
            Debug.Log("Example Chore Ended!");
        }


        #region Game Logic

        public override void GameAreaSelected(MRUKAnchor anchor)
        {
            base.GameAreaSelected(anchor);
            // NOTE: need to be maintained due to incompatibility issues
            xtdAnchorPrefabSpawner.SetGameAnchor(anchor);
        }

        #endregion
    }
}