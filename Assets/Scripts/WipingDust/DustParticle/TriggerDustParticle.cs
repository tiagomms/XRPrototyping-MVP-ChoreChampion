using UnityEngine;

/// <summary>
/// Dust particle that responds to trigger events.
/// </summary>
public class TriggerDustParticle : BaseWipingObject
{
    /// <summary>
    /// Unity OnTriggerEnter: Handles trigger-based hit logic with cooldown.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!IsLayerAllowed(other.gameObject.layer))
        {
            return;
        }
        if (Time.time - lastHitTime < hitCooldown)
        {
            // Still in cooldown, ignore this trigger
            return;
        }
        RegisterHit();
    }
} 