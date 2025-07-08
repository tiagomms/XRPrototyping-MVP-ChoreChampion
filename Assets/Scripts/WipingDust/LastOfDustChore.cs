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

        [Header("UI")]
        [SerializeField] private Lod01IntroGameUI startMenu;
        [SerializeField] private Lod02SelectGameTypeUI selectGameTypeMenu;
        [SerializeField] private Lod03SurfaceSelectionScreenUI selectSurfaceMenu;
        [SerializeField] private Lod04HudInGameUI inGameUI;
        [SerializeField] private Lod05EndGameUI endGameUI;
        
        [Space]
        [SerializeField] private BaseUIManager uiManager;



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
            //randomMonsterSpawner.onSurfaceCleaned.AddListener(OnSurfaceCleaned);
            //randomMonsterSpawner.onAnchorCleaned.AddListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.AddListener(OnRoomCleaned);

            onChoreTimesUp.AddListener(OnTimesUp);
        }

        protected virtual void OnDestroy()
        {
            //randomMonsterSpawner.onSurfaceCleaned.RemoveListener(OnSurfaceCleaned);
            //randomMonsterSpawner.onAnchorCleaned.RemoveListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.RemoveListener(OnRoomCleaned);

            onChoreTimesUp.RemoveListener(OnTimesUp);
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


        #region 01-Start Menu
        public void SelectGameTypeScreen()
        {
            uiManager.GoTo(selectGameTypeMenu);
        }
        #endregion

        #region 02-Select Game Type

        public void StartDeepCleanGameMode()
        {
            _playMode = PlayModeEnum.AllAreas;
            uiManager.GoTo(inGameUI);
            StartChore(true);
        }


        public void InitializeSurfaceSelection()
        {
            uiManager.GoTo(selectSurfaceMenu);
            tapAnchorMechanism.SpawnOnAllAnchors();
        }

        #endregion
        #region 03-Select Game Area

        public virtual void GameAreaSelected(MRUKAnchor anchor)
        {
            _playMode = PlayModeEnum.SelectArea;

            _selectedGameAnchor = anchor;
            onSelectGameAnchor?.Invoke(anchor);

            // NOTE: need to be maintained due to incompatibility issues
            xtdAnchorPrefabSpawner.SetGameAnchor(anchor);


            // clear spawned objects
            tapAnchorMechanism.ClearSpawnedObjects();

            // TODO: 3...2...1... Initialize game
            uiManager.GoTo(inGameUI);

            StartChore(true);

            // ???: start game immediately or 3...2...1... then start game
        }

        public MRUKAnchor GetGameArea()
        {
            return _selectedGameAnchor;
        }

        #endregion

        #region 04-InGame

        private void OnRoomCleaned()
        {
            Debug.Log($"[{nameof(LastOfDustChore)}] - {nameof(OnRoomCleaned)}");

            // get the last one - should be the end menu
            CompleteChore(true);
            uiManager.GoTo(endGameUI);
        }

        private void OnTimesUp()
        {
            Debug.Log($"[{nameof(LastOfDustChore)}] - {nameof(OnTimesUp)}");

            // clear monsters if intended
            if (removeMonstersOnTimesUp)
            {
                randomMonsterSpawner.ClearSpawnedObjects();
            }

            // no need to CompleteChore - already done in base Chore - set to false

            // go to endGameUI
            uiManager.GoTo(endGameUI);
        }

        #endregion

        #region 05-EndGame

        public void RestartGame()
        {
            Reset();
            uiManager.ResetAndGoTo(0);
        }

        public void RestartOnSelectSurface()
        {
            RestartGame();
            InitializeSurfaceSelection();
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