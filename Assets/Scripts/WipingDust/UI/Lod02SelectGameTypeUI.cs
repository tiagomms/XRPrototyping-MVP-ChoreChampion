using Chores;
using UnityEngine;
using UnityEngine.UI;
using UI;
using NaughtyAttributes;

namespace LastOfDust.UI
{
    public class Lod02SelectGameTypeUI : BaseUI
    {
        [Header("UI")]

        [SerializeField] private Button surfaceCleanMode;
        [SerializeField] private Button deepCleanMode;


        [Header("Next UI depending on Button Select")]
        [SerializeField] private Lod03SurfaceSelectionScreenUI surfaceSelectionUiScreen;
        [SerializeField] private Lod04HudInGameUI hudInGameUI;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (surfaceCleanMode != null)
            {
                surfaceCleanMode.onClick.AddListener(OnSurfaceSelection);
            }
            if (deepCleanMode != null)
            {
                deepCleanMode.onClick.AddListener(OnStartDeepCleanGameMode);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (surfaceCleanMode != null)
            {
                surfaceCleanMode.onClick.RemoveListener(OnSurfaceSelection);
            }
            if (deepCleanMode != null)
            {
                deepCleanMode.onClick.RemoveListener(OnStartDeepCleanGameMode);
            }
        }

        [Button]
        private void OnStartDeepCleanGameMode()
        {
            LastOfDustChore.Instance.StartDeepCleanGameMode();
        }

        [Button]
        private void OnSurfaceSelection()
        {
            LastOfDustChore.Instance.InitializeSurfaceSelection();
        }
    }
}
