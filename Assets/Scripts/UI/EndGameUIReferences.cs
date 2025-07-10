using TMPro;
using UnityEngine;

namespace ChoreChampion.UI
{
    public class EndGameUIReferences : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        //public TMP_Text ScoreText => scoreText;
        [SerializeField] private TMP_Text timeText;
        //public TMP_Text TimeText => timeText;

        public void SetResults(float newScore, float newTime)
        {
            string timerStr = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(newTime / 60), Mathf.FloorToInt(newTime % 60));
            timeText.text = timerStr;

            string scoreStr = Mathf.FloorToInt(newScore).ToString();
            scoreText.text = scoreStr;
        }
    }
}