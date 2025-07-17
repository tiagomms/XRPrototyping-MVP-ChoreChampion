using TMPro;
using UnityEngine;
using Utils;

namespace ChoreChampion.UI
{
    public class AddScoreUIReferences : MonoBehaviour
    {
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private Animator animator;
        public void SetPoints(float newPoints)
        {
            string scoreStr = Mathf.FloorToInt(newPoints).ToString();
            pointsText.text = scoreStr;
        }
    }
}