using UnityEngine;
using UnityEngine.Events;

namespace Chores
{
    /// <summary>
    /// Abstract base class for all chore minigames.
    /// Defines the common structure and functionality that every chore must implement.
    /// </summary>
    public abstract class Chore : MonoBehaviour
    {
        [Header("Chore Configuration")] [SerializeField]
        protected string choreId;

        [SerializeField] protected string choreName;
        [TextArea] [SerializeField] protected string choreDescription;

        protected bool isChoreActive;
        protected float timeElapsed;
        protected float score;

        public UnityEvent onChoreActive = new();
        public UnityEvent<ChoreStats> onChoreCompleted = new();
        public UnityEvent onChoreEnded = new();

        /// <summary>
        /// Starts the core gameplay logic for this chore.
        /// </summary>
        public virtual void StartChore(bool hasPlayedChore = false)
        {
            timeElapsed = 0f;
            if (hasPlayedChore) // If this is the first time playing, show a tutorial or introduction
            {
                StarMiniGameChore();
            }
            else
            {
                StartTutorial();
            }

            onChoreActive.Invoke();
        }

        /// <summary>
        /// Starts the tutorial or introduction sequence for this chore.
        /// Should be implemented by derived classes to provide instructions or guidance to the player.
        /// </summary>
        protected abstract void StartTutorial();


        /// <summary>
        /// Starts the main minigame logic for this chore.
        /// Should be implemented by derived classes to define the core gameplay.
        /// </summary>
        public abstract void StarMiniGameChore();

        /// <summary>
        /// Gathers performance stats and tells the GameManager the chore is complete.
        /// </summary>
        public virtual void CompleteChore()
        {
            isChoreActive = false;
            var choreStats = new ChoreStats(choreId, score, timeElapsed);
            onChoreCompleted.Invoke(choreStats);
        }

        /// <summary>
        /// Ends the chore and cleans up the minigame.
        /// </summary>
        public abstract void EndChore();    
        /// <summary>
        /// The main update loop for the chore's gameplay mechanics.
        /// </summary>
        protected virtual void Update()
        {
            if (isChoreActive)
            {
                timeElapsed += Time.deltaTime;
            }
        }

        public string GetChoreId()
        {
            return choreId;
        }
    }
}