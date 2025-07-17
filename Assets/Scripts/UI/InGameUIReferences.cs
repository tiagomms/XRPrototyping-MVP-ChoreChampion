using TMPro;
using UnityEngine;
using UnityEngine.Events;

/**
 * Pure UI view for in-game HUD. No game logic, only exposes methods to update UI elements.
 */
namespace ChoreChampion.UI
{
    public class InGameUIReferences : MonoBehaviour
    {
        [Header("UI - Countdown")]
        [SerializeField] private GameObject countdownSection;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private AudioSource countdownSound;
        [SerializeField] private Animator countdownAnimator;

        [Header("UI - Timer/Score")]
        [SerializeField] private GameObject timerScoreSection;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;

        [Header("Debug - Readonly")]
        [SerializeField] protected string timerStr = "00:00";
        [SerializeField] protected string scoreStr = "0";

        private void OnEnable() {
            countdownSection.SetActive(false);
            timerScoreSection.SetActive(false);
            countdownAnimator.enabled = false;
        }

        public void ShowCountdownSection()
        {
            if (countdownSection == null || timerScoreSection == null || countdownAnimator == null) return;
            countdownSection.SetActive(true);
            timerScoreSection.SetActive(false);
            countdownAnimator.enabled = true;
            if (countdownSound != null) countdownSound.Play();
        }

        public void ShowTimerSection()
        {
            if (countdownSection == null || timerScoreSection == null || countdownAnimator == null) return;
            countdownSection.SetActive(false);
            timerScoreSection.SetActive(true);
            countdownAnimator.enabled = false;
        }
        public void HideAllSections()
        {
            if (countdownSection != null) countdownSection.SetActive(false);
            if (timerScoreSection != null) timerScoreSection.SetActive(false);
            if (countdownAnimator != null) countdownAnimator.enabled = false;
        }

        public void UpdateCountdown(float time)
        {
            string countdownStr = Mathf.CeilToInt(time).ToString();
            countdownText.text = countdownStr;
        }

        public void UpdateTimer(float newTime)
        {
            timerStr = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(newTime / 60), Mathf.FloorToInt(newTime % 60));
            timerText.text = timerStr;
        }

        public void UpdateScore(float newScore)
        {
            scoreStr = Mathf.FloorToInt(newScore).ToString();
            scoreText.text = scoreStr;
        }
    }
}