using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Linq;
using System.Collections.Generic;

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
            spawnBasedOn = SpawnBasedOn.AnchorTransform;
            // Force single object placement
            SpawnAmountPerSurface = 1;
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

        protected override List<MRUKExtension.Surface> GetAnchorSurfaces(MRUK.SurfaceType surfaceTypes, MRUKAnchor anchor, ref float totalUsableSurfaceArea)
        {
            // set minDistanceToEdge 0f to avoid issue with stretching (otherwise it fails)
            return MRUKExtension.GetAnchorSurfaces(surfaceTypes, 0f, anchor, ref totalUsableSurfaceArea);
        }
    }
}