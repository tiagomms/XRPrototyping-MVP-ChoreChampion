using System;
using Chores;
using Meta.XR.MRUtilityKit;
using NaughtyAttributes;
using Oculus.Interaction;
using UI;
using UnityEngine;
using UnityEngine.UI;
using XR;

namespace LastOfDust.UI
{
    public class Lod01IntroGameUI : BaseUI
    {
        [Header("UI")]
        [SerializeField] private PokeInteractableCartoonUIButton surfaceCleanMode;
        [SerializeField] private PokeInteractableCartoonUIButton deepCleanMode;
        [SerializeField] private PokeInteractableCartoonUIButton startGame;

        private PlayModeEnum? selectedPlayMode;
        private bool isSelected;

        protected override void OnEnable()
        {
            base.OnEnable();

            isSelected = false;
            OnSelectedPlayMode(null);

            if (startGame != null)
            {
                startGame.InteractableEventWrapper.WhenSelect.AddListener(OnStartGame);
            }
            if (deepCleanMode != null)
            {
                deepCleanMode.InteractableEventWrapper.WhenSelect.AddListener(SelectDeepCleanMode);
            }
            if (surfaceCleanMode != null)
            {
                surfaceCleanMode.InteractableEventWrapper.WhenSelect.AddListener(SelectSurfaceCleanMode);
            }

            

            MRUK.Instance.RegisterSceneLoadedCallback(() => {
                startGame.ResetToInitialState();
                deepCleanMode.ResetToInitialState();
                surfaceCleanMode.ResetToInitialState();
            });
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (startGame != null)
            {
                startGame.InteractableEventWrapper.WhenSelect.RemoveListener(OnStartGame);
            }
            if (deepCleanMode != null)
            {
                deepCleanMode.InteractableEventWrapper.WhenSelect.RemoveListener(SelectDeepCleanMode);
            }
            if (surfaceCleanMode != null)
            {
                surfaceCleanMode.InteractableEventWrapper.WhenSelect.RemoveListener(SelectSurfaceCleanMode);
            }
        }

        private void OnSelectedPlayMode(PlayModeEnum? newPlayMode)
        {
            Debug.Log($"{nameof(OnSelectedPlayMode)} - playmode: {newPlayMode}");

            // Enable/disable surface clean interactable based on SelectArea mode
            if (newPlayMode == PlayModeEnum.SelectArea)
            {
                surfaceCleanMode.OnSelected();
                deepCleanMode.OnDisable();
                startGame.OnEnable();
            }

            // Enable/disable deep clean interactable based on AllAreas mode
            else if (newPlayMode == PlayModeEnum.AllAreas)
            {
                surfaceCleanMode.OnDisable();
                deepCleanMode.OnSelected();
                startGame.OnEnable();
            }

            // Enable/disable start game interactable based on whether a mode is selected
            else if (newPlayMode == null)
            {
                surfaceCleanMode.OnEnable();
                deepCleanMode.OnEnable();
                startGame.OnDisable();
            }
            selectedPlayMode = newPlayMode;

        }

        [Button]
        private void SelectSurfaceCleanMode()
        {
            SelectPlayModeAndToggleStart(PlayModeEnum.SelectArea);
        }

        [Button]
        private void SelectDeepCleanMode()
        {
            SelectPlayModeAndToggleStart(PlayModeEnum.AllAreas);
        }

        private void SelectPlayModeAndToggleStart(PlayModeEnum newPlayMode)
        {
            isSelected = !isSelected;
            PlayModeEnum? mode = isSelected ? newPlayMode : null;

            OnSelectedPlayMode(mode);
        }

        [Button]
        private void OnStartGame()
        {
            LastOfDustChore.Instance.AfterGameTypeSelection(selectedPlayMode);
        }

        [Button]
        private void OnQuitGame()
        {
            LastOfDustChore.Instance.QuitGame();
        }

    }
}
