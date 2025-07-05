using UnityEngine;

namespace Chores
{
    public class LaundryToss : Chore
    {
        private float choreTimer = 10f;

        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            isChoreActive = true;
            score = 0f;
            timeElapsed = 0f;
            Debug.Log("Starting Example Chore minigame!");
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

        protected override void Update()
        {
            base.Update();
            if (timeElapsed >= choreTimer)
            {
                CompleteChore();
            }
        }

        public void AddScore()
        {
            if (!isChoreActive) return;
            score += 10f;
        }
    }
}