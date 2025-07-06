using Oculus.Interaction;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class RegularFoldTutorial : MonoBehaviour
{
    public static RegularFoldTutorial Instance { get; private set; }

    [Header("References")]
    [Tooltip("The root object that contains the interactable T-shirt")]  
    [SerializeField] private GameObject interactableRoot;
    [Tooltip("Name of the child under interactableRoot that is the actual shirt")]  
    [SerializeField] private string interactableChildName = "InteractableTshirt";

    [Header("Prefabs")]
    [Tooltip("Animated, non-interactable T-shirt prefab with Animator")]
    [SerializeField] private GameObject nonInteractablePrefab;

    [Header("Steps")]
    [Tooltip("Animation trigger names for each step (must match SnapInteractors count)")]
    [SerializeField] private string[] animationTriggers;
    [Tooltip("Audio clips for each step (must match SnapInteractors count)")]
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
    private bool tutorialActive = false;
    private MonoBehaviour[] disabledComponents;

    // store original child transform relative to root
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Vector3 originalLocalScale;

    private GameObject interactableInstance;

    void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("Missing AudioSource on " + name);

        if (interactableRoot == null)
            Debug.LogWarning("interactableRoot not set. Call RegisterInteractable() after spawning.");
    }

    public void RegisterInteractable(GameObject root)
    {
        if (root == null)
        {
            Debug.LogError("RegularFoldTutorial: Cannot register null root");
            return;
        }
        interactableRoot = root;

        // find the actual shirt child
        var child = root.transform.Find(interactableChildName);
        interactableInstance = child != null ? child.gameObject : root;

        // record its local transform and scale
        originalLocalPos = interactableInstance.transform.localPosition;
        originalLocalRot = interactableInstance.transform.localRotation;
        originalLocalScale = interactableInstance.transform.localScale;

        Debug.Log($"RegularFoldTutorial: Registered {(child != null ? "child" : "root")} '{interactableInstance.name}' at localPos {originalLocalPos}, localScale {originalLocalScale}");
    }

    public void StartFoldingTutorial()
    {
        if (tutorialActive)
        {
            Debug.LogWarning("RegularFoldTutorial: Tutorial is already active");
            return;
        }
        if (interactableInstance == null)
        {
            Debug.LogError("RegularFoldTutorial: No interactableInstance. Cannot start tutorial.");
            return;
        }
        if (nonInteractablePrefab == null)
        {
            Debug.LogError("RegularFoldTutorial: No nonInteractablePrefab assigned. Cannot start tutorial.");
            return;
        }

        // get world transform and scale of the shirt
        var t = interactableInstance.transform;
        var worldPos = t.position;
        var worldRot = t.rotation;
        var worldScale = t.localScale;

        // disable all interactable scripts on root hierarchy
        DisableInteractableComponents();

        // spawn non-interactable at same world transform
        nonInteractableInstance = Instantiate(nonInteractablePrefab, worldPos, worldRot, t.parent);
        nonInteractableInstance.transform.localScale = worldScale;

        // reset the interactable child back to its original local transform and scale
        interactableInstance.transform.localPosition = originalLocalPos;
        interactableInstance.transform.localRotation = originalLocalRot;
        interactableInstance.transform.localScale = originalLocalScale;

        // setup tutorial steps
        animator = nonInteractableInstance.GetComponent<Animator>();
        snapInteractors = nonInteractableInstance.GetComponentsInChildren<SnapInteractor>(true);
        foreach (var si in snapInteractors)
        {
            si.enabled = false;
            si.WhenStateChanged -= OnSnap;
        }

        tutorialActive = true;
        currentStep = -1;
        AdvanceStep();
    }

    private void DisableInteractableComponents()
    {
        var all = interactableRoot.GetComponentsInChildren<MonoBehaviour>(true);
        var list = new System.Collections.Generic.List<MonoBehaviour>();
        foreach (var comp in all)
        {
            if (comp is IInteractable || comp.GetType().Name.Contains("Grabbable") || comp.GetType().Name.Contains("Snap"))
            {
                if (comp.enabled)
                {
                    list.Add(comp);
                    comp.enabled = false;
                }
            }
        }
        disabledComponents = list.ToArray();
    }

    private void ReenableInteractableComponents()
    {
        if (disabledComponents == null) return;
        foreach (var comp in disabledComponents)
            if (comp != null) comp.enabled = true;
        disabledComponents = null;
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
            audioSource.clip = stepClips[currentStep]; audioSource.Play();
            if (stepVFX != null && currentStep < stepVFX.Length && stepVFX[currentStep] != null)
                stepVFX[currentStep].Play();

            snapInteractors[currentStep].enabled = true;
            snapInteractors[currentStep].WhenStateChanged += OnSnap;
        }
        else EndTutorial();
    }

    private void OnSnap(InteractorStateChangeArgs args)
    {
        if (args.PreviousState != InteractorState.Select && args.NewState == InteractorState.Select)
            Invoke(nameof(AdvanceStep), 0.1f);
    }

    private void EndTutorial()
    {
        tutorialActive = false;
        foreach (var si in snapInteractors) si.WhenStateChanged -= OnSnap;

        if (outroClip != null)
        {
            audioSource.clip = outroClip; audioSource.Play();
            Invoke(nameof(CleanupTutorial), outroClip.length + 0.1f);
        }
        else CleanupTutorial();

        ReenableInteractableComponents();
    }

    private void CleanupTutorial()
    {
        if (nonInteractableInstance != null) Destroy(nonInteractableInstance);
    }

    public void StopTutorial()
    {
        if (tutorialActive) EndTutorial();
    }

    private void OnDestroy()
    {
        if (snapInteractors == null) return;
        foreach (var si in snapInteractors) si.WhenStateChanged -= OnSnap;
    }
}
