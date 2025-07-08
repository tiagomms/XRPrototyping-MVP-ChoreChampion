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
        [Header("Score System")]
        [SerializeField] private LastOfDustPointSystem pointSystem;

        [Header("Spawners")]
        [SerializeField] private ExtendedAnchorPrefabSpawner xtdAnchorPrefabSpawner;
        [SerializeField] private PlaceAndStretchSingleObjectOnAnchor tapAnchorMechanism;
        [SerializeField] private RandomSpawnPrefabsOnAnchorSurfaces randomMonsterSpawner;

        [Header("Other settings")]
        [SerializeField] private bool removeMonstersOnTimesUp = true;

        // Singleton instance
        public static LastOfDustChore Instance { get; protected set; }

        protected PlayModeEnum _playMode = PlayModeEnum.AllAreas;
        public PlayModeEnum PlayMode
        {
            get => _playMode;
            set => _playMode = value;
        }

        public UnityEvent<MRUKAnchor> onSelectGameAnchor;
        protected MRUKAnchor _selectedGameAnchor;

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

            // point system only enabled when game starts
            pointSystem.enabled = false;
        }

        protected virtual void Start()
        {
            randomMonsterSpawner.onSurfaceCleaned.AddListener(OnSurfaceCleaned);
            randomMonsterSpawner.onAnchorCleaned.AddListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.AddListener(OnRoomCleaned);

            onChoreTimesUp.AddListener(MonsterCleanUp);
        }

        protected virtual void OnDestroy()
        {
            randomMonsterSpawner.onSurfaceCleaned.RemoveListener(OnSurfaceCleaned);
            randomMonsterSpawner.onAnchorCleaned.RemoveListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.RemoveListener(OnRoomCleaned);

            onChoreTimesUp.RemoveListener(MonsterCleanUp);
        }

        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            isChoreActive = true;

            // point system only enabled when game starts
            pointSystem.enabled = true;

            Debug.Log("Starting Minigame for Example Chore");

            if (_playMode == PlayModeEnum.AllAreas)
            {
                randomMonsterSpawner.SpawnOnAllAnchors();
            }
            else if (_playMode == PlayModeEnum.SelectArea)
            {
                randomMonsterSpawner.SpawnOnAnchor(_selectedGameAnchor);
            }
        }

        public override void CompleteChore(bool inTime = true)
        {
            base.CompleteChore(inTime);

            // point system disabled once chore is complete
            pointSystem.enabled = false;

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
            CompleteChore();
        }

        #endregion
        #region Select Game Area

        public virtual void GameAreaSelected(MRUKAnchor anchor)
        {
            _playMode = PlayModeEnum.SelectArea;

            _selectedGameAnchor = anchor;
            onSelectGameAnchor?.Invoke(anchor);

            // NOTE: need to be maintained due to incompatibility issues
            xtdAnchorPrefabSpawner.SetGameAnchor(anchor);

            // ???: start game immediately or 3...2...1... then start game
        }

        public MRUKAnchor GetGameArea()
        {
            return _selectedGameAnchor;
        }

        private void MonsterCleanUp()
        {
            if (removeMonstersOnTimesUp)
            {
                randomMonsterSpawner.ClearSpawnedObjects();
            }
        }

        #endregion

        public override void Reset()
        {
            base.Reset();

            randomMonsterSpawner.ClearSpawnedObjects();
            tapAnchorMechanism.ClearSpawnedObjects();

            _playMode = PlayModeEnum.AllAreas;
            _selectedGameAnchor = null;
            onSelectGameAnchor?.Invoke(null);
        }

    }
}