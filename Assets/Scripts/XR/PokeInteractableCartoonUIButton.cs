using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using System.Collections;

namespace XR
{
    /// <summary>
    /// Component that manages a cartoon UI button with poke interaction capabilities.
    /// Automatically retrieves required components on Awake/Validate.
    /// </summary>
    public class PokeInteractableCartoonUIButton : MonoBehaviour
    {
        private enum State { Enable = 0, Disable = 1, Selected = 2 }

        [Header("Required Components")]
        [SerializeField] private State initialState = State.Enable;
        [SerializeField] private PokeInteractable pokeInteractable;
        [SerializeField] private InteractableUnityEventWrapper interactableEventWrapper;
        [SerializeField] private Image uiImage;

        [Header("UI Colors")]
        [SerializeField] private Color imageNormal = Color.white;
        [SerializeField] private Color imageSelected = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private Color imageDisabled = new Color(1f, 1f, 1f, 160f / 255f);

        private State state;

        /// <summary>
        /// Gets the PokeInteractable component.
        /// </summary>
        public PokeInteractable PokeInteractable => pokeInteractable;

        /// <summary>
        /// Gets the InteractableUnityEventWrapper component.
        /// </summary>
        public InteractableUnityEventWrapper InteractableEventWrapper => interactableEventWrapper;

        /// <summary>
        /// Gets the UI Image component.
        /// </summary>
        public Image UIImage => uiImage;

        private void Awake()
        {
            ValidateAndGetComponents();
        }

        private void OnValidate()
        {
            ValidateAndGetComponents();
        }

        /// <summary>
        /// Waits for 1 second before setting the initial state to avoid being overridden by other component logic.
        /// </summary>
        public void ResetToInitialState()
        {
            // Set the initial UI state
            switch (initialState)
            {
                case State.Enable:
                    SetEnableState();
                    break;
                case State.Disable:
                    SetDisableState();
                    break;
                case State.Selected:
                    SetSelectedState();
                    break;
            }
            state = initialState;
        }

        /// <summary>
        /// Changes the image color to the selected state.
        /// </summary>
        public void OnSelected()
        {
            if (state == State.Selected) return;
            SetSelectedState();
        }

        public void OnUnselected()
        {
            if (state == State.Enable) return;
            SetEnableState();
        }

        /// <summary>
        /// Changes the image color to the normal (default) state and enables the interactable.
        /// </summary>
        public void OnEnable()
        {
            if (state == State.Enable) return;
            SetEnableState();
        }

        /// <summary>
        /// Changes the image color to the disabled state and disables the interactable.
        /// </summary>
        public void OnDisable()
        {
            if (state == State.Disable) return;
            SetDisableState();
        }

        private void SetEnableState()
        {
            Debug.Log($"{gameObject.name} - Try Enable: {uiImage.name}, {pokeInteractable.gameObject.name}");
            if (uiImage != null)
            {
                uiImage.color = imageNormal;
            }
            if (pokeInteractable != null)
            {
                pokeInteractable.Enable();
            }
            state = State.Enable;
        }

        private void SetSelectedState()
        {
            if (uiImage != null)
            {
                uiImage.color = imageSelected;
            }
            state = State.Selected;
        }

        private void SetDisableState()
        {
            Debug.Log($"{gameObject.name} - Try Disable: {uiImage.name}, {pokeInteractable.gameObject.name}");
            if (uiImage != null)
            {
                uiImage.color = imageDisabled;
            }
            if (pokeInteractable != null)
            {
                pokeInteractable.Disable();
            }
            state = State.Disable;
        }

        /// <summary>
        /// Validates and retrieves required components if they are not already assigned.
        /// </summary>
        private void ValidateAndGetComponents()
        {
            // Get PokeInteractable component if not assigned
            if (pokeInteractable == null)
            {
                pokeInteractable = GetComponent<PokeInteractable>();
                if (pokeInteractable == null)
                {
                    Debug.LogWarning($"{nameof(PokeInteractableCartoonUIButton)}: No PokeInteractable component found on {gameObject.name}");
                }
            }

            // Get InteractableUnityEventWrapper component if not assigned
            if (interactableEventWrapper == null)
            {
                interactableEventWrapper = GetComponent<InteractableUnityEventWrapper>();
                if (interactableEventWrapper == null)
                {
                    Debug.LogWarning($"{nameof(PokeInteractableCartoonUIButton)}: No InteractableUnityEventWrapper component found on {gameObject.name}");
                }
            }

            // Get Image component from active children if not assigned
            if (uiImage == null)
            {
                uiImage = GetComponentInChildren<Image>(false); // true = include inactive children
                if (uiImage == null)
                {
                    Debug.LogWarning($"{nameof(PokeInteractableCartoonUIButton)}: No Image component found in children of {gameObject.name}");
                }
            }
        }
    }
}