using System;
using Chores;
using NaughtyAttributes;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LastOfDust.UI
{
    public class LodIntroGameUI : BaseUI
    {
        [Header("UI")]
        [SerializeField] private Button startGame;
        [SerializeField] private Button quitGame;


        [Header("Next")]
        [SerializeField] private LodSelectGameTypeUI selectGameTypeScreen;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (startGame != null)
            {
                startGame.onClick.AddListener(OnStartGame);
            }
            if (quitGame != null)
            {
                quitGame.onClick.AddListener(OnQuitGame);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (startGame != null)
            {
                startGame.onClick.RemoveListener(OnStartGame);
            }
            if (quitGame != null)
            {
                quitGame.onClick.RemoveListener(OnQuitGame);
            }
        }

        [Button]
        private void OnStartGame()
        {
            //LastOfDustChore.Instance.StartChore(true);
            BaseUIManager.Instance.GoTo(selectGameTypeScreen);
        }

        [Button]
        private void OnQuitGame()
        {
            LastOfDustChore.Instance.QuitGame();
        }

    }
}
