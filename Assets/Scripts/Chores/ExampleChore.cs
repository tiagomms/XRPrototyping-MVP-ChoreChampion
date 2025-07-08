using UnityEngine;

namespace Chores
{
    /// <summary>
    /// ExampleChore is a concrete implementation of the Chore base class, providing specific logic for the Example Chore.
    /// </summary>
    public class ExampleChore : Chore
    {
        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            Debug.Log("Starting Minigame for Example Chore");
        }

        public override void CompleteChore(bool inTime = true)
        {
            base.CompleteChore(inTime);
            Debug.Log("Example Chore completed!");
        }

        public override void EndChore()
        {
            onChoreEnded.Invoke();
            Debug.Log("Example Chore Ended!");
        }
    }
}