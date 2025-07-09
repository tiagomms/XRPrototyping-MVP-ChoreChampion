using UnityEngine;

/// <summary>
/// Dust particle that responds to collision events and accumulates force.
/// </summary>
public class CollisionWipingObject : BaseWipingObject
{
    /// <summary>
    /// Minimum collision force required to register a hit.
    /// </summary>
    [SerializeField]
    protected float collisionForceThreshold = 1.0f;

    /// <summary>
    /// Accumulated collision force during cooldown.
    /// </summary>
    protected float accumulatedCollisionForce = 0.0f;

    /// <summary>
    /// Unity OnCollisionEnter: Handles collision-based hit logic with force accumulation and cooldown.
    /// </summary>
    /// <param name="collision">Collision data.</param>
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!IsLayerAllowed(collision.gameObject.layer))
        {
            return;
        }
        //Debug.Log($"[{nameof(CollisionDustParticle)}] Collision Enter: force {collision.relativeVelocity.magnitude.ToString("0.##")}, accumulated: {accumulatedCollisionForce.ToString("0.##")}");
        if (Time.time - lastHitTime < hitCooldown)
        {
            // Still in cooldown, ignore this collision
            return;
        }
        accumulatedCollisionForce += collision.relativeVelocity.magnitude;
        if (accumulatedCollisionForce >= collisionForceThreshold)
        {
            RegisterHit();
            accumulatedCollisionForce = 0.0f;
        }
    }
}