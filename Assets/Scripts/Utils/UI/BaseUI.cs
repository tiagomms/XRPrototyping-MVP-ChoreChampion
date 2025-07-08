using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class BaseUI : MonoBehaviour
    {
        
        [SerializeField, Tooltip("Manager reference to go back.")]
        protected BaseUIManager uiManager;
        [SerializeField, Tooltip("Change it if a parent is the UI object.")]
        protected GameObject uiObject;
        [SerializeField] protected Button backButton;

        /// <summary>
        /// Set default uiManager and uiObject values if both are null
        /// User is welcome to change
        /// </summary>
        protected virtual void OnValidate()
        {
            Validate();
        }

        private void Validate()
        {
            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<BaseUIManager>();
            }

            if (uiObject == null)
            {
                uiObject = gameObject;
            }
        }

        protected virtual void Awake()
        {
            Validate();
        }

        protected virtual void OnEnable()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(GoBack);
            }
        }
        protected virtual void OnDisable()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(GoBack);
            }
        }

        public virtual void Show(Action onShown = null)
        {
            uiObject.SetActive(true);
            onShown?.Invoke();
        }

        public virtual void Hide(Action onHide = null)
        {
            onHide?.Invoke();
            uiObject.SetActive(false);
        }

        [Button]
        public virtual void GoBack()
        {
            if (uiManager != null)
            {
                uiManager.GoBack();
            }
        }
    }
}