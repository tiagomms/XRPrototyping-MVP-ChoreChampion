using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Chores
{
    public class FoldingClass : Chore
    {
        
        [SerializeField] private float choreTimer = 10f;
        [SerializeField] private TextMeshPro scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        public UnityEvent onMiniGameStarted = new();
        public bool autoStart = false;

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

        public override void CompleteChore()
        {
            base.CompleteChore();
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

