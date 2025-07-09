using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Chores
{
    public enum PlayModeEnum
    {
        SelectArea = 0,
        AllAreas = 1
    }

    /// <summary>
    /// Abstract base class for all chore minigames.
    /// Defines the common structure and functionality that every chore must implement.
    /// </summary>
    public abstract class Chore : MonoBehaviour
    {

        [Header("Chore Configuration")][SerializeField] protected string choreId;
        [SerializeField] protected string choreName;
        [TextArea][SerializeField] protected string choreDescription;

        [SerializeField] protected float choreTimer = 10f;
        protected float choreCurrentTime;
        public float ChoreCurrentTime => choreCurrentTime;

        protected bool isChoreActive;
        public bool IsChoreActive => isChoreActive;
        protected float timeElapsed;
        protected float score;
        public float Score => score;

        public UnityEvent onChoreActive = new();
        public UnityEvent onChoreTimesUp = new();
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
                // I don't think the tutorial needs a timer
                // otherwise you need to trigger a reset at the end
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
        public virtual void CompleteChore(bool inTime = true)
        {
            isChoreActive = false;

            var choreStats = new ChoreStats(choreId, score, timeElapsed, inTime);
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

                choreCurrentTime = choreTimer - timeElapsed;
                if (timeElapsed > choreTimer)
                {
                    choreCurrentTime = 0f;
                    CompleteChore(false);
                    onChoreTimesUp.Invoke();
                }
            }
        }

        public string GetChoreId()
        {
            return choreId;
        }

        /// <summary>
        /// @Daniel - please review this - it was missing
        /// </summary>
        public void QuitGame()
        {
            // ???: unsure if I want this part

            if (isChoreActive)
            {
                Reset();
            }


            // go back to main menu
            SceneManager.LoadScene(0);
        }

        public virtual void Reset()
        {
            isChoreActive = false;
            timeElapsed = 0f; // reset timer and chores
            score = 0f;
        }

        public virtual void AddScore(float points)
        {
            if (!isChoreActive) return;
            score += points;
        }

    }
}