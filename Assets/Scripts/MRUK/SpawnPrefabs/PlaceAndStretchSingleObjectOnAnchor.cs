using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Linq;

namespace ChoreChampion.XR.MRUtilityKit
{
    /// <summary>
    /// Places a single object as a child of the anchor and stretches it to fully cover the surface.
    /// Use case is specific (e.g., background plane, masking zones, etc.).
    /// </summary>
    public class PlaceAndStretchSingleObjectOnAnchor : FixedSpawnPrefabsOnAnchorSurfaces
    {
        protected override void OnValidate()
        {
            base.OnValidate();
            // Force single object placement
            SpawnAmount = 1;
            // Force parenting and stretching
            parentToAnchor = true;
            allowStretch = true;

            // make sure just one entry
            if (fixedLocalPositions.Count == 0)
            {
                fixedLocalPositions.Add(new());
            }
            else if (fixedLocalPositions.Count > 1)
            {
                Vector2 firstValue = fixedLocalPositions.FirstOrDefault();
                fixedLocalPositions.Clear();
                fixedLocalPositions.Add(firstValue);
            }
        }
    }
}