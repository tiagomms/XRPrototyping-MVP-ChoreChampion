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

        /// <summary>
        /// Every anchor that may use this - needs the list of surfaces where objects may spawn and the toal usable surface area (for random spawns)
        /// </summary>
        protected override void BuildAnchorsSurfaceDataDictionary()
        {
            anchorsSurfaceData = new();

            foreach (var keyPair in gameAnchorSelection.AnchorPrefabSpawnerObjects)
            {
                MRUKAnchor anchor = keyPair.Key;
                float totalUsableSurfaceArea = 0f;

                // FIXME: minDistance to edge => issue on stretching - if you stretch there is no minDistance to edge
                var anchorSurfacesList = MRUKExtension.GetAnchorSurfaces(GetSurfaceTypes(), 0f, anchor, ref totalUsableSurfaceArea);
                if (anchorSurfacesList.Count == 0)
                {
                    Debug.LogWarning($"[{GetType().Name} - {nameof(BuildAnchorsSurfaceDataDictionary)}]: Anchor {anchor.name} does not have surfaces!");
                    continue;
                }

                anchorsSurfaceData.Add(key: anchor,
                    value: new AnchorSurfaceData
                    {
                        SurfaceList = anchorSurfacesList,
                        TotalUsableSurfaceArea = totalUsableSurfaceArea
                    }
                );
            }
        }
    }
}