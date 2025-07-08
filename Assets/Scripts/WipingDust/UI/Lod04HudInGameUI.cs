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

namespace LastOfDust.UI
{
    public class Lod04HudInGameUI : BaseUI
    {
        [Header("UI")]
        [SerializeField] protected TMP_Text timerText;
        [SerializeField] protected TMP_Text scoreText;

        [Header("Next")]
        [SerializeField] protected Lod05EndGameUI next;

        [Header("Debug - Readonly")]
        [SerializeField] protected string timerStr = "00:00";
        [SerializeField] protected string scoreStr = "0";

        protected override void OnEnable()
        {
            base.OnEnable();
            if (LastOfDustChore.Instance == null) return;
            UpdateTimer(LastOfDustChore.Instance.ChoreCurrentTime);
            UpdateScore(LastOfDustChore.Instance.Score);

            LastOfDustChore.Instance.onChoreCompleted.AddListener(GameStop);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (LastOfDustChore.Instance == null) return;

            LastOfDustChore.Instance.onChoreCompleted.RemoveListener(GameStop);
        }

        private void GameStop(ChoreStats arg0)
        {
            // ???: unsure here - should I disappear? - my stack does not allow me to exist along the end screen
            BaseUIManager.Instance.GoTo(next);

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

        void Update()
        {
            if (LastOfDustChore.Instance.IsChoreActive)
            {
                UpdateTimer(LastOfDustChore.Instance.ChoreCurrentTime);
                UpdateScore(LastOfDustChore.Instance.Score);
            }
        }

    }
}
