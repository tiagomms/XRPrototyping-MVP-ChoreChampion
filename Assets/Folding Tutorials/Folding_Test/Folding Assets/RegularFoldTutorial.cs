using Oculus.Interaction;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class RegularFoldTutorial : MonoBehaviour
{
    [Header("Prefabs & Spawn")]
    [Tooltip("Interactable stencil + T-shirt prefab (face-down)")]
    [SerializeField] private GameObject interactablePrefab;
    [Tooltip("Non-interactable T-shirt prefab with Animator")]
    [SerializeField] private GameObject nonInteractablePrefab;
    [Tooltip("Where to spawn both variants")]
    [SerializeField] private Transform spawnPoint;

    [Header("Steps")]
    [Tooltip("One SnapInteractor per fold step (in order)")]
    [SerializeField] private SnapInteractor[] snapInteractors;
    [Tooltip("Animation trigger names for each step")]
    [SerializeField] private string[] animationTriggers;
    [Tooltip("Audio clips for each step (must match count)")]
    [SerializeField] private AudioClip[] stepClips;
    [Tooltip("VFX (optional) for each step)")]
    [SerializeField] private ParticleSystem[] stepVFX;

    [Header("Outro")]
    [Tooltip("Audio clip to play when tutorial is done")]
    [SerializeField] private AudioClip outroClip;

    private AudioSource audioSource;
    private Animator animator;
    private GameObject interactableInstance;
    private GameObject nonInteractableInstance;
    private int currentStep = -1;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Spawn the interactable face-down variant
        interactableInstance = Instantiate(
            interactablePrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            spawnPoint
        );

        // Spawn the non-interactable animated variant, but keep it hidden
        nonInteractableInstance = Instantiate(
            nonInteractablePrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            spawnPoint
        );
        nonInteractableInstance.SetActive(false);

        animator = nonInteractableInstance.GetComponent<Animator>();
    }

    /// <summary>
    /// Call this (e.g. via UI Button OnClick) to swap to animated T-shirt and begin step 1.
    /// </summary>
    public void StartFoldingTutorial()
    {
        if (interactableInstance != null)
            Destroy(interactableInstance);

        nonInteractableInstance.SetActive(true);
        AdvanceStep();
    }

    private void AdvanceStep()
    {
        // Unsubscribe from previous step's snap event
        if (currentStep >= 0 && currentStep < snapInteractors.Length)
            snapInteractors[currentStep].WhenStateChanged -= OnSnap;

        currentStep++;

        if (currentStep < snapInteractors.Length)
        {
            // Trigger this step's animation
            animator.SetTrigger(animationTriggers[currentStep]);

            // Play this step's audio
            audioSource.clip = stepClips[currentStep];
            audioSource.Play();

            // Play VFX if assigned
            if (stepVFX != null &&
                currentStep < stepVFX.Length &&
                stepVFX[currentStep] != null)
            {
                stepVFX[currentStep].Play();
            }

            // Wait for the player to snap into the next socket
            snapInteractors[currentStep].WhenStateChanged += OnSnap;
        }
        else
        {
            EndTutorial();
        }
    }

    private void OnSnap(InteractorStateChangeArgs args)
    {
        if (args.NewState == InteractorState.Select)
            AdvanceStep();
    }

    private void EndTutorial()
    {
        audioSource.clip = outroClip;
        audioSource.Play();
        // Clean up the whole tutorial after outro finishes
        Destroy(gameObject, outroClip.length + 0.1f);
    }
}
