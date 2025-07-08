using System;
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
        [SerializeField] private GameObject gameLostSection;
        [SerializeField] private GameObject gameWonSection;

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

        private void DisplayGameResultSection()
        {
            if (LastOfDustChore.Instance == null) return;
            bool timesUp = LastOfDustChore.Instance.ChoreCurrentTime == 0f;

            // TODO: here UI team, you define what you want to do - I literally created 2 fake gameobjects for tests
            gameLostSection.SetActive(timesUp);
            gameWonSection.SetActive(!timesUp);
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
            FullReset();
        }

        private static void FullReset()
        {
            LastOfDustChore.Instance.Reset();
            BaseUIManager.Instance.ResetAndGoTo(0);
        }

        [Button]
        private void GoToReanchorSurface()
        {
            FullReset();

            LastOfDustChore.Instance.PlayMode = PlayModeEnum.SelectArea;
            BaseUIManager.Instance.GoTo(surfaceSelectionScreenUI);
        }

        [Button]
        private void OnQuitGame()
        {
            LastOfDustChore.Instance.QuitGame();
        }

    }
}
