using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using ChoreChampion.XR.MRUtilityKit;

/// <summary>
/// Example dust particle cube that animates its scale on hit and kill events.
/// </summary>
public class TestDustCubeMesh : MonoBehaviour
{
    private BaseWipingObject _wipingObj;
    private MRUKSpawnedObject _mrukSpawnedObject;
    /// <summary>
    /// Minimum Y scale when flattened.
    /// </summary>
    private const float MinYScale = 0.1f;
    /// <summary>
    /// Maximum XZ scale when spread.
    /// </summary>
    private const float MaxXZScale = 2.0f;
    /// <summary>
    /// Default scale (100%).
    /// </summary>
    private Vector3 defaultScale;
    /// <summary>
    /// Duration for scale animations.
    /// </summary>
    [SerializeField]
    private float scaleAnimDuration = 0.2f;

    /// <summary>
    /// Unity Awake: Store default scale and subscribe to events.
    /// </summary>
    private void Awake()
    {
        _wipingObj = GetComponentInParent<BaseWipingObject>();

        defaultScale = transform.localScale;
    }

    
    private void Start() 
    {
        _mrukSpawnedObject = GetComponentInChildren<MRUKSpawnedObject>();
        // no need to remove listeners because I am deleting the BaseDustParticle
        _wipingObj.onDustParticleHit.AddListener(AnimateHitScale);
        _wipingObj.onDustParticleKilled.AddListener(AnimateKillAndDestroy);
    }

    /// <summary>
    /// Animates the scale of the cube based on remaining life points.
    /// </summary>
    private void AnimateHitScale()
    {
        Vector3 targetScale = CalculateNewScale();
        transform.DOScale(targetScale, scaleAnimDuration).SetEase(Ease.OutBack);
    }

    private Vector3 CalculateNewScale()
    {
        // 1 is subtracted to both because because on the last hit the thing is killed
        int maxLife = _wipingObj.MaxLifePoints - 1; 
        int currentLife = _wipingObj.LifePoints - 1;
        float t = 1.0f - (float)currentLife / maxLife;
        // Interpolate Y from 1.0 to MinYScale, XZ from 1.0 to MaxXZScale
        float yScale = Mathf.Lerp(1.0f, MinYScale, t);
        float xzScale = Mathf.Lerp(1.0f, MaxXZScale, t);
        Vector3 targetScale = new Vector3(defaultScale.x * xzScale, defaultScale.y * yScale, defaultScale.z * xzScale);
        return targetScale;
    }

    /// <summary>
    /// Animates the cube shrinking to zero and destroys the GameObject.
    /// </summary>
    private void AnimateKillAndDestroy()
    {
        transform.DOScale(Vector3.zero, scaleAnimDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => _mrukSpawnedObject.Delete());
    }
} 