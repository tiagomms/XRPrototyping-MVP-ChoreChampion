using Oculus.Interaction;
using UnityEngine;
using System.Collections;

/// <summary>
/// Plays a clip + VFX the moment a SnapInteractor reaches Select state,
/// then hides this helper once the effects finish.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RegularFoldTutorial : MonoBehaviour
{
    [Tooltip("SnapInteractor (socket) that receives the object.")]
    [SerializeField] private SnapInteractor snapInteractor;

    [Tooltip("AudioSource with the snap-in clip.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("ParticleSystem to play on snap-in.")]
    [SerializeField] private ParticleSystem snapVFX;

    /* ─────────────────────────────────────────────── */
    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (snapInteractor != null)
            snapInteractor.WhenStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        if (snapInteractor != null)
            snapInteractor.WhenStateChanged -= OnStateChanged;
    }

    /* ─────────────────────────────────────────────── */
    private void OnStateChanged(InteractorStateChangeArgs args)
    {
        // Fire only when the interactor moves INTO the Select state
        if (args.NewState == InteractorState.Select)
            HandleSnap();
    }

    void HandleSnap()
    {
        if (audioSource) audioSource.Play();
        if (snapVFX)     snapVFX.Play();

        StartCoroutine(DisappearAfterEffects());
    }

    IEnumerator DisappearAfterEffects()
    {
        float audioLen = (audioSource && audioSource.clip) ? audioSource.clip.length : 0f;

        float particleLen = 0f;
        if (snapVFX)
        {
            var main = snapVFX.main;
            particleLen = main.duration + main.startLifetime.constantMax;
        }

        yield return new WaitForSeconds(Mathf.Max(audioLen, particleLen));
        gameObject.SetActive(false);           // or Destroy(gameObject);
    }

    public void StartFoldingTutorial()
    {
        // disable overlay grab transform
        var child = transform.Find("FlatTshirtFinal");
        if (child != null)
            child.GetComponent<GrabFreeTransformer>().enabled = false;
        else
            Debug.LogError("FlatTshirtFinal not found!");


    }
    
    
}
