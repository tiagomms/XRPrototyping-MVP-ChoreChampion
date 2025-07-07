using UnityEngine;

public class StartTutorialButtonScript : MonoBehaviour
{
    // drag-&-drop the GameObject with RegularFoldTutorial in the Inspector
    public void StartTutorialButton()
    {
        RegularFoldTutorial.Instance.StartFoldingTutorial();

    }
}