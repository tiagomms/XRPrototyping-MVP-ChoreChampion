using UnityEngine;

namespace Chores
{
    /// <summary>
    /// Controls the visibility and switching of UI panels in the game.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("Assign all UI panels here (Main Menu should be first)")] [SerializeField]
        private GameObject mainMenuPanel;

        [SerializeField] private GameObject choreSelectionPanel;
        [SerializeField] private GameObject introductionPanel;

        private GameObject _currentUI;

        private void Start()
        {
            ShowPanel(mainMenuPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            if (_currentUI != null)
                _currentUI.SetActive(false);

            _currentUI = panel;
            _currentUI.SetActive(true);
        }

        public void ShowMainMenuPanel()
        {
            ShowPanel(mainMenuPanel);
        }

        public void ShowIntroductionPanel()
        {
            ShowPanel(introductionPanel);
        }

        public void ShowChoreSelectionPanel()
        {
            ShowPanel(choreSelectionPanel);
        }
    }
}