using System;
using ChoreChampion.UI;
using Chores;
using NaughtyAttributes;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LastOfDust.UI
{
    public class Lod05EndGameUI : BaseUI
    {
        [Header("UI")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button reanchorSurfaceButton;
        [SerializeField] private Button quitGame;

        // TODO: here UI team, you define what you want to do - I literally created 2 fake gameobjects for tests
        // NOTE: here it is unclear if they are different panels or text with changes, I assume game objects
        [SerializeField] private EndGameUIReferences gameLostSection;
        [SerializeField] private EndGameUIReferences gameWonSection;

        [SerializeField] private Lod03SurfaceSelectionScreenUI surfaceSelectionScreenUI;

        protected override void OnEnable()
        {
            base.OnEnable();
            DisplayGameResultSection();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetry);
            }
            if (reanchorSurfaceButton != null)
            {
                // ???: interactable or not visible?
                reanchorSurfaceButton.interactable = LastOfDustChore.Instance.PlayMode == PlayModeEnum.SelectArea;

                reanchorSurfaceButton.onClick.AddListener(GoToReanchorSurface);
            }
            if (quitGame != null)
            {
                quitGame.onClick.AddListener(OnQuitGame);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(OnRetry);
            }
            if (reanchorSurfaceButton != null)
            {
                reanchorSurfaceButton.onClick.RemoveListener(GoToReanchorSurface);
            }
            if (quitGame != null)
            {
                quitGame.onClick.RemoveListener(OnQuitGame);
            }
        }

        [Button]
        private void OnRetry()
        {
            LastOfDustChore.Instance.RestartGame();
        }

        [Button]
        private void GoToReanchorSurface()
        {
            LastOfDustChore.Instance.RestartOnSelectSurface();
        }

        [Button]
        private void OnQuitGame()
        {
            LastOfDustChore.Instance.QuitGame();
        }
        
        private void DisplayGameResultSection()
        {
            if (LastOfDustChore.Instance == null) return;
            bool timesUp = LastOfDustChore.Instance.ChoreCurrentTime == 0f;

            // TODO: here UI team, you define what you want to do - I literally created 2 fake gameobjects for tests
            gameLostSection.gameObject.SetActive(timesUp);
            gameWonSection.gameObject.SetActive(!timesUp);

            if (timesUp)
            {
                gameLostSection.SetResults(LastOfDustChore.Instance.Score, LastOfDustChore.Instance.TimeElapsed);
            }
            else
            {
                gameWonSection.SetResults(LastOfDustChore.Instance.Score, LastOfDustChore.Instance.TimeElapsed);
            }
        }

    }
}
