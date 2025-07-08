using System.Linq;
using Chores;
using UnityEngine;

namespace GameController
{
    /// <summary>
    /// Manages the overall game state, chore selection, and coordination between managers.
    /// Implemented as a Singleton to be easily accessible from anywhere in the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UIController uiController;
        [SerializeField] private Chore[] availableChores;
        private static GameManager Instance { get; set; }
        public bool IsGameActive { get; private set; }

        private Chore _currentChore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
            }
        }

        public void Start()
        {
            PlayerData playerData = DataManager.LoadPlayerData();
            if (playerData.HasMadeIntroduction())
            {
                uiController.ShowMainMenuPanel();
            }
            else
            {
                ShowIntroduction();
            }
        }

        /// <summary>
        /// Shows the introduction panel to the player.
        /// </summary>
        private void ShowIntroduction()
        {
            uiController.ShowIntroductionPanel();
        }

        /// <summary>
        /// Ends the introduction sequence, updates player data, and shows the main menu.
        /// </summary>
        public void EndIntroduction()
        {
            uiController.ShowMainMenuPanel();
            DataManager.GetPlayerData().UpdateIntroduction(true);
            DataManager.SavePlayerData();
        }


        /// <summary>
        /// Starts the currently selected chore's gameplay.
        /// </summary>
        /// <param name="choreId">The unique identifier for the selected chore.</param>
        public void SelectChore(string choreId)
        {
            _currentChore = availableChores.First(c => c.GetChoreId() == choreId);
            if (_currentChore == null)
            {
                Debug.LogError($"Chore with ID '{choreId}' not found!");
                return;
            }

            uiController.ShowChoreSelectionPanel();
            var hasPlayedChore = DataManager.GetPlayerData().HasPlayedChore(choreId);
            _currentChore.StartChore(hasPlayedChore);
            IsGameActive = true;
            _currentChore.onChoreCompleted.AddListener(OnChoreCompleted);
            _currentChore.onChoreEnded.AddListener(OnChoreEnded);
        }

        /// <param name="choreStats">The performance data from the completed chore.</param>
        private void OnChoreCompleted(ChoreStats choreStats)
        {
            DataManager.SaveChoreStats(choreStats);
        }

        /// <summary>
        /// Handles the end of a chore, resets state, and returns to the main menu.
        /// </summary>
        private void OnChoreEnded()
        {
            IsGameActive = false;
            _currentChore.onChoreCompleted.RemoveListener(OnChoreCompleted);
            _currentChore.onChoreEnded.RemoveListener(OnChoreEnded);
            _currentChore = null;
            uiController.ShowMainMenuPanel();
        }

        /// <summary>
        /// Returns to the main chore selection menu.
        /// </summary>
        public void ReturnToMenu()
        {
            uiController.ShowMainMenuPanel();
        }

        public T GetCurrentChore<T>() where T : Chore
        {
            return _currentChore as T; // if cast fails it will return null
        }
    }
}