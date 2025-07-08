using UnityEngine;

public static class PhysicsLayerExtensions 
{
    
    /// <summary>
    /// Checks if a layer is included in a LayerMask.
    /// </summary>
    /// <param name="layer">Layer to check.</param>
    /// <param name="mask">LayerMask to check against.</param>
    /// <returns>True if layer is in mask, false otherwise.</returns>
    public static bool IsLayerInMask(this LayerMask mask, int layer)
    {
        return ((1 << layer) & mask.value) != 0;
    }
}