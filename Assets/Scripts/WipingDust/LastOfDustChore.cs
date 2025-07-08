using UnityEngine;
using LastOfDust.UI;
using ChoreChampion.XR.MRUtilityKit;
using System.Xml.Serialization;
using Meta.XR.MRUtilityKit;
using UnityEngine.Events;
using System;
using UI;

namespace Chores
{
    public class LastOfDustChore : Chore
    {
        [Header("Spawners")]
        [SerializeField] private ExtendedAnchorPrefabSpawner xtdAnchorPrefabSpawner;
        [SerializeField] private PlaceAndStretchSingleObjectOnAnchor tapAnchorMechanism;
        [SerializeField] private RandomSpawnPrefabsOnAnchorSurfaces randomMonsterSpawner;


        // Singleton instance
        public static LastOfDustChore Instance { get; protected set; }

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

        protected virtual void Start()
        {
            randomMonsterSpawner.onSurfaceCleaned.AddListener(OnSurfaceCleaned);
            randomMonsterSpawner.onAnchorCleaned.AddListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.AddListener(OnRoomCleaned);
        }

        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            Debug.Log("Starting Minigame for Example Chore");

            if (_playMode == PlayModeEnum.AllAreas)
            {
                randomMonsterSpawner.SpawnOnAllAnchors();
            }
            else if (_playMode == PlayModeEnum.SelectArea)
            {
                randomMonsterSpawner.SetGameAnchor(_selectedGameAnchor);
                randomMonsterSpawner.SpawnOnGameAnchor();
            }
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
        public void InitializeSurfaceSelection()
        {
            tapAnchorMechanism.SpawnOnAllAnchors();
        }

        public override void GameAreaSelected(MRUKAnchor anchor)
        {
            base.GameAreaSelected(anchor);
            // NOTE: need to be maintained due to incompatibility issues
            xtdAnchorPrefabSpawner.SetGameAnchor(anchor);
        }

        private void OnSurfaceCleaned()
        {
            Debug.Log($"[{nameof(LastOfDustChore)}] - {nameof(OnSurfaceCleaned)}");
        }

        private void OnAnchorCleaned()
        {
            Debug.Log($"[{nameof(LastOfDustChore)}] - {nameof(OnAnchorCleaned)}");
        }

        private void OnRoomCleaned()
        {
            Debug.Log($"[{nameof(LastOfDustChore)}] - {nameof(OnRoomCleaned)}");
            var uiManager = BaseUIManager.Instance;
            
            // get the last one - should be the end menu
            // ???: use a int to state the last 
            uiManager.GoTo(uiManager.UiPanels.Count - 1);
        }

        #endregion
    }
}