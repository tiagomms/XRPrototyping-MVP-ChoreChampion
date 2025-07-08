using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages a stack-based navigation of UI panels derived from BaseUI.
    /// </summary>
    public class BaseUIManager : MonoBehaviour
    {
        public static BaseUIManager Instance { get; protected set; }

        [Header("Assign all UI panels here (Main Menu should be first)")]
        [SerializeField] private List<BaseUI> uiPanels = new List<BaseUI>();
        public List<BaseUI> UiPanels => uiPanels;


        // TODO: pause is not here it is somewhere else
        [SerializeField] private BaseUI pausePanel;
        public BaseUI PausePanel => pausePanel;


        private Stack<BaseUI> navigationStack = new Stack<BaseUI>();
        private BaseUI currentUI;

        private bool isPausing;

        protected virtual void Awake()
        {
            // If an instance already exists and it's not this, destroy this object
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Hide all panels initially
            HideAll();
            GoTo(0);
        }

        public void HideAll()
        {
            foreach (var panel in uiPanels)
            {
                panel.Hide();
            }
        }

        // TODO: pause is not here, it is somewhere else
        public void ShowPause()
        {
            pausePanel.Show();
        }

        public void HidePause()
        {
            pausePanel.Hide();
        }

        public void GoTo(int panelIndex)
        {
            if (panelIndex < 0 || panelIndex >= uiPanels.Count)
            {
                Debug.LogWarning($"[{nameof(BaseUIManager)}] - {nameof(GoTo)}: index {panelIndex} is not between 0 and the total of {uiPanels.Count} panels we have in uiPanels");
            }
            GoTo(uiPanels[panelIndex]);
        }

        public void GoTo(BaseUI nextUI)
        {
            if (nextUI == null || nextUI == currentUI)
                return;

            if (currentUI != null)
            {
                currentUI.Hide();
                navigationStack.Push(currentUI);
            }

            nextUI.Show();
            currentUI = nextUI;
        }

        public void ResetAndGoTo(int panelIndex)
        {
            if (panelIndex < 0 || panelIndex >= uiPanels.Count)
            {
                Debug.LogWarning($"[{nameof(BaseUIManager)}] - {nameof(ResetAndGoTo)}: index {panelIndex} is not between 0 and the total of {uiPanels.Count} panels we have in uiPanels");
            }
            GoTo(uiPanels[panelIndex]);
        }

        public void ResetAndGoTo(BaseUI nextUI)
        {
            currentUI = null;
            if (nextUI == null || nextUI == currentUI)
                return;

            navigationStack.Clear();

            nextUI.Show();
            currentUI = nextUI;
        }

        public void GoBack()
        {
            if (navigationStack.Count == 0)
                return;

            if (currentUI != null)
                currentUI.Hide();

            currentUI = navigationStack.Pop();
            currentUI.Show();
        }

        public void HideCurrentPanel()
        {
            if (currentUI != null)
                currentUI.Hide();
        }

        public void ShowCurrentPanel()
        {
            if (currentUI != null)
                currentUI.Show();
        }
    }
}