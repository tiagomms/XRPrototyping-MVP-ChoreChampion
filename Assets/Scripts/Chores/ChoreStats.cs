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

        public ChoreStats(string choreId, float score, float timeTaken)
        {
            this.choreId = choreId;
            this.score = score;
            this.timeTaken = timeTaken;
        }
    }
}