using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Chores
{
    public class LaundryToss : Chore
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        public UnityEvent onMiniGameStarted = new();
        public bool autoStart = false;

        private void Start()
        {
            if (autoStart)
            {
                StarMiniGameChore();
            }
        }

        protected override void StartTutorial()
        {
            Debug.Log("Starting tutorial for Example Chore");
        }

        public override void StarMiniGameChore()
        {
            isChoreActive = true;
            score = 0f;
            timeElapsed = 0f;
            onMiniGameStarted.Invoke();
            Debug.Log("Starting Example Chore minigame!");
        }

        public override void CompleteChore(bool inTime = true)
        {
            base.CompleteChore(inTime);
            scoreText.text = score.ToString();
            Debug.Log("Example Chore completed!");
        }

        public override void Reset()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public override void EndChore()
        {
            onChoreEnded.Invoke();
            Debug.Log("Example Chore Ended!");
        }

        protected override void Update()
        {
            base.Update();
            if (!isChoreActive) return;
        }

        public void AddScore()
        {
            if (!isChoreActive) return;
            score += 10f;
        }
    }
}