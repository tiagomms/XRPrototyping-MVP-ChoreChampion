using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoreLoader : MonoBehaviour
{
      [Tooltip("Name of the scene to load")]
        public string sceneName;
    
        // Call this (e.g. via UI Button OnClick) to load the scene
        public void LoadScene()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneLoader: sceneName is empty!");
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
    }