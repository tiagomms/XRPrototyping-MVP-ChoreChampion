using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Chores
{
    public class LaundryToss : Chore
    {
        //[SerializeField] private float choreTimer = 10f;
        [SerializeField] private TextMeshPro scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
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
            timerText.gameObject.SetActive(false);
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
            if (!isChoreActive) return;
            timerText.text = Mathf.Floor(choreTimer - timeElapsed).ToString();
        }

        public void AddScore()
        {
            if (!isChoreActive) return;
            score += 10f;
        }
    }
}