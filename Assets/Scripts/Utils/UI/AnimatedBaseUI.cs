using System;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    /// <summary>
    /// Extends BaseUI to support fade-in/fade-out animations using CanvasGroup and DOTween.
    /// </summary>
    public abstract class AnimatedBaseUI : BaseUI
    {
        protected CanvasGroup _canvasGroup;
        [Header("Animation")]
        [SerializeField] protected float showAnimDuration = 0.5f;
        [SerializeField] protected float hideAnimDuration = 0.2f;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = _obj.GetComponentInChildren<CanvasGroup>();
        }

        /// <summary>
        /// Shows the UI, optionally with fade animation.
        /// </summary>
        /// <param name="onShown">Callback after shown (after fade if used).</param>
        public override void Show(Action onShown = null)
        {
            if (_obj.activeSelf) return;
            _obj.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, showAnimDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => onShown?.Invoke());
            }
            else
            {
                onShown?.Invoke();
            }
        }

        /// <summary>
        /// Hides the UI, optionally with fade animation.
        /// </summary>
        /// <param name="onHide">Callback after hidden (after fade if used).</param>
        public override void Hide(Action onHide = null)
        {
            if (!_obj.activeSelf) return;
            if (_canvasGroup != null)
            {
                _canvasGroup.DOFade(0f, hideAnimDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => {
                        base.Hide(onHide);
                    });
            }
            else
            {
                base.Hide(onHide);
            }
        }
    }
} 