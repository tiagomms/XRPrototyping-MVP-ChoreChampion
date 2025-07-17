using Chores;
using UnityEngine;
using UnityEngine.UI;
using UI;
using ChoreChampion.XR.MRUtilityKit;
using System;
using Oculus.Interaction;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine.Events;
using ChoreChampion.UI;

namespace LastOfDust.UI
{
    public class Lod04HudInGameUI : BaseUI
    {
        [Header("UI")]
        [SerializeField] private InGameUIReferences inGameUIReferences;
        [SerializeField] private float countdownTime = 3f;

        private float _currentTime;
        private bool _isCountingDown;

        public UnityEvent onCountDownFinished;


        protected override void OnEnable()
        {
            base.OnEnable();

            if (LastOfDustChore.Instance == null) return;
            StartCountDown();
        }

        public void UpdateTimer(float newTime)
        {
            inGameUIReferences.UpdateTimer(newTime);
        }

        public void UpdateScore(float newScore)
        {
            inGameUIReferences.UpdateScore(newScore);
        }

        /**
         * Start the countdown and update the UI accordingly.
         */
        public void StartCountDown()
        {
            _isCountingDown = true;
            _currentTime = countdownTime;

            inGameUIReferences.ShowCountdownSection();
            inGameUIReferences.UpdateCountdown(_currentTime);
        }

        /**
         * Update the countdown text and handle completion.
         */
        private void UpdateCountdownText()
        {
            _currentTime -= Time.deltaTime;
            inGameUIReferences.UpdateCountdown(_currentTime);

            if (_currentTime <= 0f)
            {
                OnCountdownComplete();
            }
        }

        /**
         * Handle countdown completion and switch UI to timer/score section.
         */
        private void OnCountdownComplete()
        {
            _isCountingDown = false;
            _currentTime = 0f;
            onCountDownFinished.Invoke();

            inGameUIReferences.ShowTimerSection();
        }

        void Update()
        {
            if (_isCountingDown)
            {
                UpdateCountdownText();
            }
            else
            {
                UpdateInGameText();
            }
        }

        /**
         * Update timer and score UI during gameplay.
         */
        private void UpdateInGameText()
        {
            if (LastOfDustChore.Instance != null && LastOfDustChore.Instance.IsChoreActive)
            {
                UpdateTimer(LastOfDustChore.Instance.ChoreCurrentTime);
                UpdateScore(LastOfDustChore.Instance.Score);
            }
        }
    }
}
