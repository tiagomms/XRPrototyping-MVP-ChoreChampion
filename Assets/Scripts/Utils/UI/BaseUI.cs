using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class BaseUI : MonoBehaviour
    {
        [SerializeField] protected GameObject uiObject;
        [SerializeField] protected Button backButton;
        protected GameObject _obj;
        protected virtual void Awake()
        {
            _obj = uiObject != null ? uiObject : gameObject;
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
            _obj.SetActive(true);
            onShown?.Invoke();
        }

        public virtual void Hide(Action onHide = null)
        {
            onHide?.Invoke();
            _obj.SetActive(false);
        }

        [Button]
        public virtual void GoBack()
        {
            if (BaseUIManager.Instance != null)
            {
                BaseUIManager.Instance.GoBack();
            }
        }
    }
}