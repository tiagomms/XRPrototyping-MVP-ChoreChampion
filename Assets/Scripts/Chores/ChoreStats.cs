namespace Chores
{
    /// <summary>
    /// A data structure to hold the results of a single chore session.
    /// </summary>
    [System.Serializable]
    public class ChoreStats
    {
        public string choreId;
        public float score;
        public float timeTaken;
        public bool inTime;

        public ChoreStats(string choreId, float score, float timeTaken, bool inTime)
        {
            this.choreId = choreId;
            this.score = score;
            this.timeTaken = timeTaken;
            this.inTime = inTime;
        }
    }
}