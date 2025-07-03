using System.Collections.Generic;
using Chores;

namespace GameController
{
    /// <summary>
    /// Contains all persistent data for the player.
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public bool hasMadeIntroduction;
        public List<ChoreStats> allTimeStats = new();

        public void UpdateChoreStats(ChoreStats newStats)
        {
            allTimeStats.Add(newStats);
        }

        public void UpdateIntroduction(bool hasIntroduction)
        {
            hasMadeIntroduction = hasIntroduction;
        }

        public bool HasPlayedChore(string choreId)
        {
            int index = allTimeStats.FindIndex(g => g.choreId == choreId);
            return index > -1;
        }

        public bool HasMadeIntroduction() => hasMadeIntroduction;
    }
}