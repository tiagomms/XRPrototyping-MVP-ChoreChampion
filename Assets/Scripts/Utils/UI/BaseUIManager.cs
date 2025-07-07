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
        private Stack<BaseUI> navigationStack = new Stack<BaseUI>();
        private BaseUI currentUI;

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
            foreach (var panel in uiPanels)
            {
                panel.Hide();
            }

            // Show first panel (assumed to be main menu)
            if (uiPanels.Count > 0)
            {
                currentUI = uiPanels[0];
                currentUI.Show();
            }
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