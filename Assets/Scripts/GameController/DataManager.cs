using System.IO;
using Chores;
using UnityEngine;

namespace GameController
{
    /// <summary>
    /// Handles saving and loading of all player data.
    /// </summary>
    public static class DataManager
    {
        private static PlayerData _playerData;
        private static readonly string PlayerDataPath = Path.Combine(Application.persistentDataPath, "playerData.json");

        /// <summary>
        /// Loads the player data. If the data file does not exist, creates a new PlayerData instance.
        /// </summary>
        public static PlayerData LoadPlayerData()
        {
            if (File.Exists(PlayerDataPath))
            {
                string json = File.ReadAllText(PlayerDataPath);
                _playerData = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log("Player data loaded from: " + PlayerDataPath);
            }
            else
            {
                Debug.LogWarning("Player data file not found, creating new PlayerData instance.");
                _playerData = new PlayerData();
            }

            return _playerData;
        }

        /// <summary>
        /// Saves the entire player data object to a file.
        /// </summary>
        public static void SavePlayerData()
        {
            string json = JsonUtility.ToJson(_playerData, true);
            File.WriteAllText(PlayerDataPath, json);
            Debug.Log("Player data saved to: " + PlayerDataPath);
        }

        /// <summary>
        /// Updates the player's chore statistics and saves the player data.
        /// </summary>
        public static void SaveChoreStats(ChoreStats stats)
        {
            Debug.Log("Saving chore stats: " + stats);
            _playerData.UpdateChoreStats(stats);
            SavePlayerData();
        }

        public static PlayerData GetPlayerData() => _playerData;
    }
}