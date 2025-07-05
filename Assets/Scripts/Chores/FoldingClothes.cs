using UnityEngine;

namespace Chores
{
    public class FoldingClass : Chore
    {
        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            Debug.Log("Starting Minigame for Example Chore");
        }

        public override void CompleteChore()
        {
            base.CompleteChore();
            Debug.Log("Example Chore completed!");
        }

        public override void EndChore()
        {
            onChoreEnded.Invoke();
            Debug.Log("Example Chore Ended!");
        }
    }
}