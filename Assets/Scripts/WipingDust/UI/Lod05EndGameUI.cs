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


        [SerializeField] private Lod03SurfaceSelectionScreenUI surfaceSelectionScreenUI;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetry);
            }
            if (reanchorSurfaceButton != null && LastOfDustChore.Instance.PlayMode == Chore.PlayModeEnum.SelectArea)
            {
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
            if (reanchorSurfaceButton != null && LastOfDustChore.Instance.PlayMode == Chore.PlayModeEnum.SelectArea)
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

            LastOfDustChore.Instance.PlayMode = Chore.PlayModeEnum.SelectArea;
            BaseUIManager.Instance.GoTo(surfaceSelectionScreenUI);
        }

        [Button]
        private void OnQuitGame()
        {
            LastOfDustChore.Instance.QuitGame();
        }

    }
}
