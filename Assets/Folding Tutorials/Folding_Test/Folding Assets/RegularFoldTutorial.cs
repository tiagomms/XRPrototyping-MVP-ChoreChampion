using Oculus.Interaction;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class RegularFoldTutorial : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The interactable T-shirt instance that the user positions")]
    [SerializeField] private GameObject interactableInstance;

    [Header("Prefabs")]
    [Tooltip("Animated, non-interactable T-shirt prefab with Animator")]
    [SerializeField] private GameObject nonInteractablePrefab;

    [Header("Steps")]
    [Tooltip("Animation trigger names for each step (must match SnapInteractors count)")]
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
    private GameObject nonInteractableInstance;
    private SnapInteractor[] snapInteractors;
    private int currentStep = -1;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("Missing AudioSource on " + name);

        if (interactableInstance == null)
            Debug.LogWarning("interactableInstance not set. Call RegisterInteractable() after spawning.");
    }

    /// <summary>
    /// Call this right after instantiating your interactable prefab
    /// so the tutorial knows which object to swap out.
    /// </summary>
    public void RegisterInteractable(GameObject instance)
    {
        interactableInstance = instance;
        Debug.Log($"RegularFoldTutorial: Registered interactable instance: {instance.name}");
    }

    /// <summary>
    /// Call when you're ready to swap to the animated variant.
    /// Hook this to a UI button via OnClick.
    /// </summary>
    public void StartFoldingTutorial()
    {
        if (interactableInstance == null)
        {
            Debug.LogError("RegularFoldTutorial: No interactableInstance. Cannot start tutorial.");
            return;
        }

        // capture world transform + scale
        Transform original = interactableInstance.transform;
        Vector3 worldPos      = original.position;
        Quaternion worldRot   = original.rotation;
        Vector3 originalScale = original.localScale;
        Transform parent      = original.parent;

        // hide interactable
        interactableInstance.SetActive(false);

        // instantiate non-interactable at same transform
        nonInteractableInstance = Instantiate(
            nonInteractablePrefab,
            worldPos,
            worldRot,
            parent);
        nonInteractableInstance.transform.localScale = originalScale;

        // get animator & interactors
        animator = nonInteractableInstance.GetComponent<Animator>();
        snapInteractors = nonInteractableInstance.GetComponentsInChildren<SnapInteractor>(true);
        foreach (var si in snapInteractors)
        {
            si.enabled = false;
            si.WhenStateChanged -= OnSnap;
        }

        // start steps
        AdvanceStep();
    }

    private void AdvanceStep()
    {
        foreach (var si in snapInteractors)
        {
            si.WhenStateChanged -= OnSnap;
            si.enabled = false;
        }

        currentStep++;
        if (currentStep < snapInteractors.Length)
        {
            animator.SetTrigger(animationTriggers[currentStep]);
            audioSource.clip = stepClips[currentStep];
            audioSource.Play();

            if (stepVFX != null && currentStep < stepVFX.Length && stepVFX[currentStep] != null)
                stepVFX[currentStep].Play();

            snapInteractors[currentStep].enabled = true;
            snapInteractors[currentStep].WhenStateChanged += OnSnap;
        }
        else
        {
            EndTutorial();
        }
    }

    private void OnSnap(InteractorStateChangeArgs args)
    {
        if (args.PreviousState != InteractorState.Select && args.NewState == InteractorState.Select)
            AdvanceStep();
    }

    private void EndTutorial()
    {
        audioSource.clip = outroClip;
        audioSource.Play();
        Destroy(gameObject, outroClip.length + 0.1f);
    }
}
