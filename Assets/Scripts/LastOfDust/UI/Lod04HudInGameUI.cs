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

namespace LastOfDust.UI
{
    public class Lod04HudInGameUI : BaseUI
    {
        [Header("UI")]
        [SerializeField] protected GameObject inGameUI;
        [SerializeField] protected GameObject countdownSection;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private AudioSource initialSound;
        [SerializeField] private Animator countdownAnimator;
        [SerializeField] private float countdownTime = 3f;


        [SerializeField] protected GameObject timerScoreSection;
        [SerializeField] protected TMP_Text timerText;
        [SerializeField] protected TMP_Text scoreText;

        [Header("Next")]
        [SerializeField] protected Lod05EndGameUI next;

        [Header("Debug - Readonly")]
        [SerializeField] protected string timerStr = "00:00";
        [SerializeField] protected string scoreStr = "0";

        private float _currentTime;
        private bool _isCountingDown;

        public UnityEvent onCountDownFinished;


        protected override void OnEnable()
        {
            base.OnEnable();
            countdownAnimator.enabled = false;
            //countdownSection.gameObject.SetActive(false);
            //timerScoreSection.SetActive(false);

            if (LastOfDustChore.Instance == null) return;
            StartCountDown();

            UpdateTimer(LastOfDustChore.Instance.ChoreCurrentTime);
            UpdateScore(LastOfDustChore.Instance.Score);
        }

        public void UpdateTimer(float newTime)
        {
            timerStr = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(newTime / 60), Mathf.FloorToInt(newTime % 60));

            if (timerText != null)
            {
                timerText.text = timerStr;
            }
        }

        public void UpdateScore(float newScore)
        {
            scoreStr = Mathf.FloorToInt(newScore).ToString();

            if (scoreText != null)
            {
                scoreText.text = scoreStr;
            }
        }

        public void StartCountDown()
        {
            _isCountingDown = true;
            _currentTime = countdownTime;
            UIToggle(_isCountingDown);
            countdownAnimator.enabled = true;

            if (initialSound)
            {
                initialSound.Play();
            }
        }

        private void UIToggle(bool isCountingDown)
        {
            countdownSection.gameObject.SetActive(isCountingDown);
            timerScoreSection.SetActive(!isCountingDown);
        }

        private void OnCountdownComplete()
        {
            _isCountingDown = false;
            _currentTime = 0f;
            onCountDownFinished.Invoke();
            countdownAnimator.enabled = false;

            UIToggle(_isCountingDown);
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

        private void UpdateInGameText()
        {
            if (LastOfDustChore.Instance.IsChoreActive)
            {
                UpdateTimer(LastOfDustChore.Instance.ChoreCurrentTime);
                UpdateScore(LastOfDustChore.Instance.Score);
            }
        }

        private void UpdateCountdownText()
        {
            _currentTime -= Time.deltaTime;
            countdownText.text = Mathf.CeilToInt(_currentTime).ToString();

            if (_currentTime <= 0f)
            {
                OnCountdownComplete();
            }
        }
    }
}
