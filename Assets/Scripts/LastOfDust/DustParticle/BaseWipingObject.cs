using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using ChoreChampion.XR.MRUtilityKit;
using System.Linq;

/// <summary>
/// Abstract base class for dust particle objects. Contains shared fields, cooldown, life, and event logic.
/// Does NOT implement trigger or collision logic; derived classes must handle those.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class BaseWipingObject : MonoBehaviour
{
    /// <summary>
    /// All child colliders of this dust particle.
    /// </summary>
    [SerializeField]
    private List<Collider> childColliders = new List<Collider>();

    /// <summary>
    /// The Rigidbody component attached to the root GameObject.
    /// </summary>
    protected Rigidbody rootRigidbody;
    public Rigidbody RootRigidbody => rootRigidbody;

    /// <summary>
    /// LayerMask to restrict which layers can interact with this particle.
    /// </summary>
    [SerializeField]
    protected LayerMask interactionLayerMask;

    /// <summary>
    /// Maximum life points for this dust particle.
    /// </summary>
    [SerializeField]
    private int maxLifePoints = 1;

    /// <summary>
    /// Life points for this dust particle. Reaching zero triggers onDustParticleKilled.
    /// </summary>
    private int lifePoints = 1;

    /// <summary>
    /// Invoked when the dust particle is hit (trigger or collision).
    /// </summary>
    [SerializeField]
    public UnityEvent onDustParticleHit = new UnityEvent();

    /// <summary>
    /// Invoked when the dust particle is killed (life points reach zero).
    /// </summary>
    [SerializeField]
    public UnityEvent onDustParticleKilled = new UnityEvent();

    /// <summary>
    /// Cooldown time (in seconds) to prevent multiple hits in rapid succession.
    /// </summary>
    [SerializeField]
    protected float hitCooldown = 0.2f;

    //[SerializeField] protected bool onDeathIsKinematic = true;

    /// <summary>
    /// Time when the last hit was registered.
    /// </summary>
    protected float lastHitTime = -Mathf.Infinity;

    protected bool isAlive = true;

    /// <summary>
    /// Property to get/set maximum life points with validation.
    /// </summary>
    public int MaxLifePoints
    {
        get { return maxLifePoints; }
        set { maxLifePoints = Mathf.Max(1, value); }
    }

    /// <summary>
    /// Property to get/set life points with validation.
    /// </summary>
    public int LifePoints
    {
        get { return lifePoints; }
        set { lifePoints = Mathf.Clamp(value, 0, maxLifePoints); }
    }

    /// <summary>
    /// Unity Awake: Collects child colliders and root Rigidbody.
    /// </summary>
    protected virtual void Awake()
    {
        // Get root Rigidbody
        rootRigidbody = GetComponent<Rigidbody>();
        if (rootRigidbody == null)
        {
            Debug.LogError("BaseDustParticle requires a Rigidbody on the root GameObject.", this);
        }
        isAlive = true;
        lifePoints = maxLifePoints;
    }

    /// <summary>
    /// Registers a valid hit: invokes hit event, reduces life, checks for kill, and sets cooldown.
    /// </summary>
    protected void RegisterHit()
    {
        lifePoints = Mathf.Max(0, lifePoints - 1);
        lastHitTime = Time.time;
        if (lifePoints != 0)
        {
            onDustParticleHit.Invoke();
        }
        else if (isAlive)
        {
            onDustParticleKilled.Invoke();
            isAlive = false;
            //rootRigidbody.isKinematic = onDeathIsKinematic;
        }
    }

    /// <summary>
    /// Checks if a layer is included in the interaction LayerMask.
    /// </summary>
    /// <param name="layer">Layer to check.</param>
    /// <returns>True if layer is in mask, false otherwise.</returns>
    protected bool IsLayerAllowed(int layer)
    {
        return interactionLayerMask.IsLayerInMask(layer);
    }

    protected void OnDestroy()
    {
        onDustParticleHit.RemoveAllListeners();
        onDustParticleKilled.RemoveAllListeners();
    }

    public void InitializeColliders<T>(T colliders) where T : IEnumerable<Collider>
    {
        ClearColliders();
        AddColliders(colliders);
    }

    public void ClearColliders()
    {
        childColliders.Clear();
    }

    /// <summary>
    /// Adds multiple colliders to the childColliders list. Accepts arrays or lists.
    /// </summary>
    /// <typeparam name="T">A collection type implementing IEnumerable<Collider>.</typeparam>
    /// <param name="colliders">The colliders to add.</param>
    public void AddColliders<T>(T colliders) where T : IEnumerable<Collider>
    {
        if (colliders == null)
        {
            //Debug.LogWarning("AddColliders: Provided collection is null.", this);
            return;
        }
        childColliders.AddRange(colliders);
    }

    /// <summary>
    /// Removes multiple colliders from the childColliders list. Accepts arrays or lists.
    /// </summary>
    /// <typeparam name="T">A collection type implementing IEnumerable<Collider>.</typeparam>
    /// <param name="colliders">The colliders to remove.</param>
    public void RemoveColliders<T>(T colliders) where T : IEnumerable<Collider>
    {
        if (colliders == null)
        {
            //Debug.LogWarning("RemoveColliders: Provided collection is null.", this);
            return;
        }
        foreach (Collider col in colliders)
        {
            childColliders.Remove(col);
        }
    }
}
