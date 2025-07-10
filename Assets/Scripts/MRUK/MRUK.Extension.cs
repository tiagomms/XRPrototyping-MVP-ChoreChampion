using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Meta.XR.MRUtilityKit;


namespace ChoreChampion.XR.MRUtilityKit
{
    /// <summary>
    /// Defines possible locations where objects can be spawned.
    /// </summary>
    public enum MRUKSpawnLocation
    {
        Floating, // Spawn somewhere floating in the free space within the room
        AnySurface, // Spawn on any surface (i.e. a combination of all 3 options below)
        VerticalSurfaces, // Spawn only on vertical surfaces such as walls, windows, wall art, doors, etc...
        OnTopOfSurfaces, // Spawn on surfaces facing upwards such as ground, top of tables, beds, couches, etc...
        HangingDown // Spawn on surfaces facing downwards such as the ceiling
    }
    public static class MRUKExtension
    {
        /// <summary>
        /// Public copy Surface used in MRUK (it is internal only there)
        /// </summary>
        public struct Surface
        {
            public string ID;
            public MRUKAnchor Anchor;
            public float UsableArea;
            public bool IsPlane;
            public Rect Bounds;
            public Matrix4x4 Transform;

            public bool Equals(Surface other)
            {
                return ID == other.ID; // reference equality, may be fragile
            }

            public override bool Equals(object obj)
            {
                return obj is Surface other && Equals(other);
            }

            public override int GetHashCode()
            {
                return ID != null ? ID.GetHashCode() : 0;
            }
        }

        /// <summary>
        /// Closest anchor based on the closest surface position (while respecting the filters)
        /// </summary>
        /// <param name="labels">if null. selects all possible (returns closest anchor)</param>
        /// <param name="worldPos">World position to deduce this</param>
        /// <returns></returns>
        public static MRUKAnchor GetClosestAnchorBasedOnSurfacePosition(MRUKAnchor.SceneLabels? labels, Vector3 worldPos)
        {
            LabelFilter labelFilterBasedOnAnchorLabel = new(labels, null);
            MRUK.Instance.GetCurrentRoom().TryGetClosestSurfacePosition(worldPos, out Vector3 surfacePosition, out MRUKAnchor newAnchor, labelFilterBasedOnAnchorLabel);
            return newAnchor;
        }

        /// <summary>
        /// Show all anchors of a given type in the room.
        /// </summary>
        /// <param name="room">The room to show anchors in.</param>
        /// <param name="labels">The labels to show.</param>
        /// <returns>A list of anchors of the given type.</returns>
        public static List<MRUKAnchor> ShowAnchorsOfType(MRUKRoom room, MRUKAnchor.SceneLabels labels)
        {
            return room.Anchors.Where(anchor => anchor.HasAnyLabel(labels)).ToList();
        }


        /// <summary>
        /// FROM MRUKRoom.cs - extracted from GenerateRandomPositionOnSurface - since we are using the same anchor over and over again, no need to calculate it every time.
        /// In our case: I don't want a plane rect in a volume bound for now
        /// </summary>
        /// <param name="surfaceTypes"></param>
        /// <param name="minDistanceToEdge"></param>
        /// <param name="anchor"></param>
        /// <param name="totalUsableSurfaceArea"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        /// ???: we may some issues with the code because we ignore the planeRect surface in a volume bound (which I guess is the on top) 
        public static List<Surface> GetAnchorSurfaces(MRUK.SurfaceType surfaceTypes, float minDistanceToEdge, MRUKAnchor anchor, ref float totalUsableSurfaceArea)
        {
            List<Surface> surfaces = new();
            float minWidth = 2f * minDistanceToEdge;

            Debug.Log($"[{nameof(GetAnchorSurfaces)}] Anchor {anchor.name}, Has PlaneRect: {anchor.PlaneRect.HasValue}, Has VolumeBounds: {anchor.VolumeBounds.HasValue}, components {anchor}");
            if (anchor.VolumeBounds.HasValue)
            {
                for (int i = 0; i < 6; ++i)
                {
                    Rect bounds;
                    Matrix4x4 faceTransform;
                    if (i == 0)
                    {
                        if ((surfaceTypes & MRUK.SurfaceType.FACING_UP) == 0)
                        {
                            continue;
                        }
                    }
                    else if (i == 1)
                    {
                        if ((surfaceTypes & MRUK.SurfaceType.FACING_DOWN) == 0)
                        {
                            continue;
                        }
                    }
                    else if ((surfaceTypes & MRUK.SurfaceType.VERTICAL) == 0)
                    {
                        continue;
                    }

                    switch (i)
                    {
                        case 0:
                            // +Z face
                            bounds = new()
                            {
                                xMin = anchor.VolumeBounds.Value.min.x,
                                xMax = anchor.VolumeBounds.Value.max.x,
                                yMin = anchor.VolumeBounds.Value.min.y,
                                yMax = anchor.VolumeBounds.Value.max.y
                            };
                            faceTransform = Matrix4x4.TRS(new Vector3(0f, 0f, anchor.VolumeBounds.Value.max.z), Quaternion.identity, Vector3.one);
                            break;
                        case 1:
                            // -Z face
                            bounds = new()
                            {
                                xMin = -anchor.VolumeBounds.Value.max.x,
                                xMax = -anchor.VolumeBounds.Value.min.x,
                                yMin = anchor.VolumeBounds.Value.min.y,
                                yMax = anchor.VolumeBounds.Value.max.y
                            };
                            faceTransform = Matrix4x4.TRS(new Vector3(0f, 0f, anchor.VolumeBounds.Value.min.z), Quaternion.Euler(0f, 180f, 0f), Vector3.one);
                            break;
                        case 2:
                            // +X face
                            bounds = new()
                            {
                                xMin = -anchor.VolumeBounds.Value.max.z,
                                xMax = -anchor.VolumeBounds.Value.min.z,
                                yMin = anchor.VolumeBounds.Value.min.y,
                                yMax = anchor.VolumeBounds.Value.max.y
                            };
                            faceTransform = Matrix4x4.TRS(new Vector3(anchor.VolumeBounds.Value.max.x, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
                            break;
                        case 3:
                            // -X face
                            bounds = new()
                            {
                                xMin = anchor.VolumeBounds.Value.min.z,
                                xMax = anchor.VolumeBounds.Value.max.z,
                                yMin = anchor.VolumeBounds.Value.min.y,
                                yMax = anchor.VolumeBounds.Value.max.y
                            };
                            faceTransform = Matrix4x4.TRS(new Vector3(anchor.VolumeBounds.Value.min.x, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
                            break;
                        case 4:
                            // +Y face
                            bounds = new()
                            {
                                xMin = anchor.VolumeBounds.Value.min.x,
                                xMax = anchor.VolumeBounds.Value.max.x,
                                yMin = -anchor.VolumeBounds.Value.max.z,
                                yMax = -anchor.VolumeBounds.Value.min.z
                            };
                            faceTransform = Matrix4x4.TRS(new Vector3(0f, anchor.VolumeBounds.Value.max.y, 0f), Quaternion.Euler(-90f, 0f, 0f), Vector3.one);
                            break;
                        case 5:
                            // -Y face
                            bounds = new()
                            {
                                xMin = anchor.VolumeBounds.Value.min.x,
                                xMax = anchor.VolumeBounds.Value.max.x,
                                yMin = anchor.VolumeBounds.Value.min.z,
                                yMax = anchor.VolumeBounds.Value.max.z
                            };
                            faceTransform = Matrix4x4.TRS(new Vector3(0f, anchor.VolumeBounds.Value.min.y, 0f), Quaternion.Euler(90f, 0f, 0f), Vector3.one);
                            break;
                        default:
                            throw new System.Exception();
                    }

                    var size = bounds.size;
                    if (size.x > minWidth && size.y > minWidth)
                    {
                        var usableArea = (size.x - minWidth) * (size.y - minWidth);
                        totalUsableSurfaceArea += usableArea;
                        surfaces.Add(new()
                        {
                            ID = Guid.NewGuid().ToString(),
                            Anchor = anchor,
                            UsableArea = usableArea,
                            IsPlane = false,
                            Bounds = bounds,
                            Transform = anchor.transform.localToWorldMatrix * faceTransform
                        });
                    }
                }
            }
            else if (anchor.PlaneRect.HasValue)
            {
                bool skipPlane = false;
                if (anchor.transform.forward.y >= Utilities.InvSqrt2)
                {
                    if ((surfaceTypes & MRUK.SurfaceType.FACING_UP) == 0)
                    {
                        skipPlane = true;
                    }
                }
                else if (anchor.transform.forward.y <= -Utilities.InvSqrt2)
                {
                    if ((surfaceTypes & MRUK.SurfaceType.FACING_DOWN) == 0)
                    {
                        skipPlane = true;
                    }
                }
                else if ((surfaceTypes & MRUK.SurfaceType.VERTICAL) == 0)
                {
                    skipPlane = true;
                }

                if (!skipPlane)
                {
                    var size = anchor.PlaneRect.Value.size;
                    if (size.x > minWidth && size.y > minWidth)
                    {
                        var usableArea = (size.x - minWidth) * (size.y - minWidth);
                        totalUsableSurfaceArea += usableArea;
                        surfaces.Add(new()
                        {
                            ID = Guid.NewGuid().ToString(),
                            Anchor = anchor,
                            UsableArea = usableArea,
                            IsPlane = true,
                            Bounds = anchor.PlaneRect.Value,
                            Transform = anchor.transform.localToWorldMatrix
                        });
                    }
                }
            }

            return surfaces;
        }

        public enum SnapTarget
        {
            None = 0, // Default behavior: clamp to surface bounds (interior)
            Corner = 1, // Force snap to the nearest corner of the surface bounds
            Center = 2, // Force snap to the exact center of the surface bounds
            NearestEdge = 3, // Snap to the nearest edge of the surface bounds (X or Z), clamping the other axis
            CenterLine = 4, // Align the closest axis (X or Z) to bounds center; clamp the other inside
            EdgeWithCenterLine = 5 // Snap to nearest edge, and center-align the other axis
        }

        public enum Clamp2DValues
        {
            None = 0,
            X = 1,
            Y = 2,
            XandY = 3
        }

        /// <summary>
        /// Finds the closest edge position to the surface provided, providing a given point, the position, normal at that position, and the distance.
        /// </summary>
        /// <param name="surface">The surface in which I doing this calculus based on the position to test</param>
        /// <param name="testPosition">The position to test.</param>
        /// <param name="minDistanceToEdge">minimum distance of any edge</param>
        /// <param name="mappedPosition"></param>
        /// <param name="closestPosition">The closest position on the surface.</param>
        /// <param name="normal">The normal at the closest position.</param>
        /// <param name="snapTarget">Enum to specify where it should snap object</param>
        /// <param name="clampOffsetToBounds">whether to restrict final offset point to surface bounds</param>
        /// <returns>The distance to the closest surface position.</returns>
        /// <exception cref="Exception">Snap target case not found</exception>
        public static float GetClosestPositionToSurface(this Surface surface, Vector3 testPosition, float minDistanceToEdge, out Vector2 mappedPosition, out Vector3 closestPosition, out Vector3 normal, SnapTarget snapTarget, Vector2? localOffset = null, Clamp2DValues clampOffsetToBounds = Clamp2DValues.None)
        {
            float candidateDistance = Mathf.Infinity;
            closestPosition = Vector3.zero;

            // Surface normal (always transform.forward of surface)
            normal = surface.Transform.MultiplyVector(Vector3.forward).normalized;
            mappedPosition = Vector2.zero;

            MRUKAnchor anchor = surface.Anchor;

            // Convert test position to local surface space
            Vector3 localPosition = surface.Transform.inverse.MultiplyPoint3x4(testPosition);
            //Vector3 localPosition = anchor.transform.InverseTransformPoint(testPosition);

            //Debug.Log($"[{nameof(GetClosestPositionToSurface)}] - Anchor: {anchor.name}, HasVolumeBound {anchor.VolumeBounds.HasValue}, HasPlaneBound {anchor.PlaneRect.HasValue}, WorldPosition {testPosition}, LocalPosition {localPosition}, normal {normal}, localOffset: {localOffset}");
            if (anchor.VolumeBounds.HasValue) // is volume
            {
                // bounds with minimum distance to edge included (for easier calculations)
                Rect surfaceBounds = CreateRectWithMinimumDistanceToEdge(surface.Bounds, minDistanceToEdge);
                // Project onto the XZ plane of the surface (Y is fixed at surface height)
                Vector2 local2D = new(localPosition.x, localPosition.y);

                // Clamp to rect by default (safe fallback)
                Vector2 clamped2D;

                //Debug.Log($"[{nameof(GetClosestPositionToSurface)}] - Volume Start Bounds: {surfaceBounds}");
                //Debug.Log($"[{nameof(GetClosestPositionToSurface)}] - Volume Start initial values: local2D - {local2D}");


                // Resolve snapping behavior
                switch (snapTarget)
                {
                    case SnapTarget.Corner:
                        // Snap to nearest corner
                        clamped2D = new(
                            (Mathf.Abs(local2D.x - surfaceBounds.xMin) < Mathf.Abs(local2D.x - surfaceBounds.xMax)) ? surfaceBounds.xMin : surfaceBounds.xMax,
                            (Mathf.Abs(local2D.y - surfaceBounds.yMin) < Mathf.Abs(local2D.y - surfaceBounds.yMax)) ? surfaceBounds.yMin : surfaceBounds.yMax
                        );
                        break;

                    case SnapTarget.Center:
                        // Snap to rect center
                        clamped2D = surfaceBounds.center;
                        break;

                    case SnapTarget.NearestEdge:
                        // Snap to nearest edge
                        clamped2D = SnapToNearestEdge(local2D, surfaceBounds);
                        break;

                    case SnapTarget.CenterLine:
                        // Align one axis to rect center
                        clamped2D = ApplyCenterAlignment(local2D, surfaceBounds);
                        break;

                    case SnapTarget.EdgeWithCenterLine:
                        // First snap to nearest edge
                        clamped2D = SnapToNearestEdge(local2D, surfaceBounds);
                        // Then align remaining axis to center
                        clamped2D = ApplyCenterAlignment(clamped2D, surfaceBounds);
                        break;

                    case SnapTarget.None:
                        clamped2D = new(
                            Mathf.Clamp(local2D.x, surfaceBounds.xMin, surfaceBounds.xMax),
                            Mathf.Clamp(local2D.y, surfaceBounds.yMin, surfaceBounds.yMax)
                        );
                        break;

                    default:
                        throw new Exception($"[{nameof(SnapTarget)}] - option missing");
                }

                Vector2 center2D = surfaceBounds.center;

                // Direction to center in Y (vertical axis)
                Vector2 dirToCenter = (center2D - local2D).normalized;

                int forwardDir = dirToCenter.y >= 0f ? 1 : -1;

                // NOTE: couldn't make it to work to go right by default, so we do it based on dirToCenter.x
                int rightDir = dirToCenter.x >= 0f ? 1 : -1;

                // ???: part of code always go to the right is not working
                /*
                // corner needs to always to go towards the center
                if (snapTarget == SnapTarget.Corner)
                {
                    // Evaluate horizontal direction to center
                    rightDir = (dirToCenter.x >= 0f ? 1 : -1);
                } 
                else
                {
                    // 90 rotated from forward
                    // ???: there might be an issue here if we want always to go towards the right
                    rightDir = -forwardDir;//(dirToCenter.x * forwardDir >= 0f) ? -1 : 1;
                }
                */

                Vector2 offsetAxis = new(rightDir, forwardDir);

                Vector2 auxOffset = localOffset != null ? (Vector2)localOffset : Vector2.zero;
                Vector2 offset = new(auxOffset.x * offsetAxis.x, auxOffset.y * offsetAxis.y);

                clamped2D += offset;

                // to make sure the offset is clamped
                if (clampOffsetToBounds != Clamp2DValues.None)
                {
                    var clampedX = clamped2D.x;
                    var clampedY = clamped2D.y;

                    if (clampOffsetToBounds == Clamp2DValues.X || clampOffsetToBounds == Clamp2DValues.XandY)
                    {
                        clampedX = Mathf.Clamp(clamped2D.x, surfaceBounds.xMin, surfaceBounds.xMax);
                    }
                    if (clampOffsetToBounds == Clamp2DValues.Y || clampOffsetToBounds == Clamp2DValues.XandY)
                    {
                        clampedY = Mathf.Clamp(clamped2D.y, surfaceBounds.xMin, surfaceBounds.xMax);
                    }

                    clamped2D = new(clampedX, clampedY);
                }

                // Rebuild world position from local 2D
                Vector3 localClosestPoint = new(clamped2D.x, clamped2D.y, 0f);
                closestPosition = surface.Transform.MultiplyPoint3x4(localClosestPoint);

                // Final distance from test point to chosen surface point
                candidateDistance = Vector3.Distance(closestPosition, testPosition);

                mappedPosition = clamped2D;

                //Debug.Log($"[{nameof(GetClosestPositionToSurface)}] - Volume Result: clamped2D - {mappedPosition}, closestPosition {closestPosition}, distance {candidateDistance}");

                // ???: unsure this is correct - not tested/not important
                //if (volumeBounds.Contains(localPosition))

            }
            else
            {
                var planeRect = anchor.PlaneRect.Value;
                localPosition.z = 0;

                if (localPosition.x > planeRect.max.x)
                {
                    localPosition.x = planeRect.max.x;
                }
                else if (localPosition.x < planeRect.min.x)
                {
                    localPosition.x = planeRect.min.x;
                }

                if (localPosition.y > planeRect.max.y)
                {
                    localPosition.y = planeRect.max.y;
                }
                else if (localPosition.y < planeRect.min.y)
                {
                    localPosition.y = planeRect.min.y;
                }
                var offset = localOffset != null ? new Vector3(((Vector2)localOffset).x, ((Vector2)localOffset).y, 0f) : Vector3.zero;

                localPosition += offset;

                closestPosition = anchor.transform.TransformPoint(localPosition);
                candidateDistance = Vector3.Distance(closestPosition, testPosition);
                mappedPosition = new(localPosition.x, localPosition.y);

                Debug.Log($"[{nameof(GetClosestPositionToSurface)}] - Plane Result: clamped2D - {mappedPosition}, closestPosition {closestPosition}, distance {candidateDistance}");
            }

            return candidateDistance;
        }

        #region IN-DEVELOPMENT(NOT-TESTED)

        /// <summary>
        /// Enum for specifying which edge to snap to on a surface.
        /// </summary>
        public enum SnapEdge
        {
            None = 0,
            ClosestEdge = 1,
            FurthestEdge = 2,
            LeftEdge = 3,
            RightEdge = 4
        }

        /// <summary>
        /// Identifies the closest edge (Top, Bottom, Left, Right) of a rect to a given point.
        /// </summary>
        /// <param name="point">The local 2D point.</param>
        /// <param name="rect">The rect to check against.</param>
        /// <returns>The closest edge as a ClosestRectEdge enum.</returns>
        private enum ClosestRectEdge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private static ClosestRectEdge GetClosestRectEdge(Vector2 point, Rect rect)
        {
            float distLeft = Mathf.Abs(point.x - rect.xMin);
            float distRight = Mathf.Abs(point.x - rect.xMax);
            float distBottom = Mathf.Abs(point.y - rect.yMin);
            float distTop = Mathf.Abs(point.y - rect.yMax);
            float minDist = Mathf.Min(distLeft, distRight, distBottom, distTop);
            if (minDist == distLeft) return ClosestRectEdge.Left;
            if (minDist == distRight) return ClosestRectEdge.Right;
            if (minDist == distBottom) return ClosestRectEdge.Bottom;
            return ClosestRectEdge.Top;
        }

        /// <summary>
        /// Finds a position on the specified edge of the surface, relative to the user's position.
        /// </summary>
        /// <param name="surface">The surface to evaluate.</param>
        /// <param name="testPosition">The user's world position.</param>
        /// <param name="minDistanceToEdge">Minimum distance from any edge.</param>
        /// <param name="mappedPosition">The mapped 2D position on the surface.</param>
        /// <param name="edgePosition">The resulting world position on the edge.</param>
        /// <param name="normal">The normal at the edge position.</param>
        /// <param name="snapEdge">Which edge to snap to.</param>
        /// <param name="localOffset">Optional local offset to apply after edge selection.</param>
        /// <param name="clampOffsetToBounds">Whether to clamp the offset to surface bounds.</param>
        /// <returns>Distance from the test position to the chosen edge position.</returns>
        /// <exception cref="ArgumentException">Thrown if snapEdge is None or invalid.</exception>
        public static float GetEdgePositionOnSurface(
            this Surface surface,
            Vector3 testPosition,
            float minDistanceToEdge,
            out Vector2 mappedPosition,
            out Vector3 edgePosition,
            out Vector3 normal,
            SnapEdge snapEdge,
            Vector2? localOffset = null,
            Clamp2DValues clampOffsetToBounds = Clamp2DValues.None)
        {
            if (snapEdge == SnapEdge.None)
            {
                throw new ArgumentException("SnapEdge must not be None.", nameof(snapEdge));
            }

            edgePosition = Vector3.zero;
            mappedPosition = Vector2.zero;
            normal = surface.Transform.MultiplyVector(Vector3.forward).normalized;
            MRUKAnchor anchor = surface.Anchor;
            Vector3 localPosition = surface.Transform.inverse.MultiplyPoint3x4(testPosition);

            if (anchor.VolumeBounds.HasValue)
            {
                Rect surfaceBounds = CreateRectWithMinimumDistanceToEdge(surface.Bounds, minDistanceToEdge);
                Vector2 local2D = new Vector2(localPosition.x, localPosition.y);
                Vector2 clamped2D = local2D;

                ClosestRectEdge closestEdge = GetClosestRectEdge(local2D, surfaceBounds);
                ClosestRectEdge targetEdge = closestEdge;

                switch (snapEdge)
                {
                    case SnapEdge.ClosestEdge:
                        targetEdge = closestEdge;
                        break;
                    case SnapEdge.FurthestEdge:
                        // Opposite of closest edge
                        targetEdge = closestEdge switch
                        {
                            ClosestRectEdge.Left => ClosestRectEdge.Right,
                            ClosestRectEdge.Right => ClosestRectEdge.Left,
                            ClosestRectEdge.Top => ClosestRectEdge.Bottom,
                            ClosestRectEdge.Bottom => ClosestRectEdge.Top,
                            _ => closestEdge
                        };
                        break;
                    case SnapEdge.LeftEdge:
                        targetEdge = closestEdge switch
                        {
                            ClosestRectEdge.Left => ClosestRectEdge.Bottom,
                            ClosestRectEdge.Right => ClosestRectEdge.Top,
                            ClosestRectEdge.Top => ClosestRectEdge.Left,
                            ClosestRectEdge.Bottom => ClosestRectEdge.Right,
                            _ => closestEdge
                        };
                        break;
                    case SnapEdge.RightEdge:
                        targetEdge = closestEdge switch
                        {
                            ClosestRectEdge.Left => ClosestRectEdge.Top,
                            ClosestRectEdge.Right => ClosestRectEdge.Bottom,
                            ClosestRectEdge.Top => ClosestRectEdge.Right,
                            ClosestRectEdge.Bottom => ClosestRectEdge.Left,
                            _ => closestEdge
                        };
                        break;
                    default:
                        throw new ArgumentException($"Unsupported SnapEdge: {snapEdge}", nameof(snapEdge));
                }

                // Snap to the target edge
                switch (targetEdge)
                {
                    case ClosestRectEdge.Left:
                        clamped2D = new Vector2(surfaceBounds.xMin, Mathf.Clamp(local2D.y, surfaceBounds.yMin, surfaceBounds.yMax));
                        break;
                    case ClosestRectEdge.Right:
                        clamped2D = new Vector2(surfaceBounds.xMax, Mathf.Clamp(local2D.y, surfaceBounds.yMin, surfaceBounds.yMax));
                        break;
                    case ClosestRectEdge.Top:
                        clamped2D = new Vector2(Mathf.Clamp(local2D.x, surfaceBounds.xMin, surfaceBounds.xMax), surfaceBounds.yMax);
                        break;
                    case ClosestRectEdge.Bottom:
                        clamped2D = new Vector2(Mathf.Clamp(local2D.x, surfaceBounds.xMin, surfaceBounds.xMax), surfaceBounds.yMin);
                        break;
                }

                // Offset logic (mirrored from GetClosestPositionToSurface)
                Vector2 center2D = surfaceBounds.center;
                Vector2 dirToCenter = (center2D - clamped2D).normalized;
                int forwardDir = dirToCenter.y >= 0f ? 1 : -1;
                int rightDir = dirToCenter.x >= 0f ? 1 : -1;
                Vector2 offsetAxis = new Vector2(rightDir, forwardDir);
                Vector2 auxOffset = localOffset != null ? (Vector2)localOffset : Vector2.zero;
                Vector2 offset = new Vector2(auxOffset.x * offsetAxis.x, auxOffset.y * offsetAxis.y);
                clamped2D += offset;

                if (clampOffsetToBounds != Clamp2DValues.None)
                {
                    float clampedX = clamped2D.x;
                    float clampedY = clamped2D.y;
                    if (clampOffsetToBounds == Clamp2DValues.X || clampOffsetToBounds == Clamp2DValues.XandY)
                    {
                        clampedX = Mathf.Clamp(clamped2D.x, surfaceBounds.xMin, surfaceBounds.xMax);
                    }
                    if (clampOffsetToBounds == Clamp2DValues.Y || clampOffsetToBounds == Clamp2DValues.XandY)
                    {
                        clampedY = Mathf.Clamp(clamped2D.y, surfaceBounds.yMin, surfaceBounds.yMax);
                    }
                    clamped2D = new Vector2(clampedX, clampedY);
                }

                Vector3 localEdgePoint = new Vector3(clamped2D.x, clamped2D.y, 0f);
                edgePosition = surface.Transform.MultiplyPoint3x4(localEdgePoint);
                mappedPosition = clamped2D;
                return Vector3.Distance(edgePosition, testPosition);
            }
            else if (anchor.PlaneRect.HasValue)
            {
                // For plane, only support ClosestEdge for now, others can be extended later
                var planeRect = anchor.PlaneRect.Value;
                localPosition.z = 0f;
                Vector2 local2D = new Vector2(localPosition.x, localPosition.y);
                Vector2 clamped2D = local2D;
                ClosestRectEdge closestEdge = GetClosestRectEdge(local2D, planeRect);
                ClosestRectEdge targetEdge = closestEdge;
                switch (snapEdge)
                {
                    case SnapEdge.ClosestEdge:
                        targetEdge = closestEdge;
                        break;
                    default:
                        throw new ArgumentException($"SnapEdge {snapEdge} not implemented for plane surfaces.", nameof(snapEdge));
                }
                switch (targetEdge)
                {
                    case ClosestRectEdge.Left:
                        clamped2D = new Vector2(planeRect.xMin, Mathf.Clamp(local2D.y, planeRect.yMin, planeRect.yMax));
                        break;
                    case ClosestRectEdge.Right:
                        clamped2D = new Vector2(planeRect.xMax, Mathf.Clamp(local2D.y, planeRect.yMin, planeRect.yMax));
                        break;
                    case ClosestRectEdge.Top:
                        clamped2D = new Vector2(Mathf.Clamp(local2D.x, planeRect.xMin, planeRect.xMax), planeRect.yMax);
                        break;
                    case ClosestRectEdge.Bottom:
                        clamped2D = new Vector2(Mathf.Clamp(local2D.x, planeRect.xMin, planeRect.xMax), planeRect.yMin);
                        break;
                }
                Vector3 offset = localOffset != null ? new Vector3(((Vector2)localOffset).x, ((Vector2)localOffset).y, 0f) : Vector3.zero;
                Vector3 localEdgePoint = new Vector3(clamped2D.x, clamped2D.y, 0f) + offset;
                if (clampOffsetToBounds != Clamp2DValues.None)
                {
                    float clampedX = localEdgePoint.x;
                    float clampedY = localEdgePoint.y;
                    if (clampOffsetToBounds == Clamp2DValues.X || clampOffsetToBounds == Clamp2DValues.XandY)
                    {
                        clampedX = Mathf.Clamp(localEdgePoint.x, planeRect.xMin, planeRect.xMax);
                    }
                    if (clampOffsetToBounds == Clamp2DValues.Y || clampOffsetToBounds == Clamp2DValues.XandY)
                    {
                        clampedY = Mathf.Clamp(localEdgePoint.y, planeRect.yMin, planeRect.yMax);
                    }
                    localEdgePoint = new Vector3(clampedX, clampedY, 0f);
                }
                edgePosition = anchor.transform.TransformPoint(localEdgePoint);
                mappedPosition = new Vector2(localEdgePoint.x, localEdgePoint.y);
                return Vector3.Distance(edgePosition, testPosition);
            }
            else
            {
                throw new ArgumentException("Surface must have either VolumeBounds or PlaneRect.", nameof(surface));
            }
        }

        #endregion

        #region AUX-METHODS

        // Snap to nearest edge of the rect, based on direction from center
        private static Vector2 SnapToNearestEdge(Vector2 local2D, Rect rect)
        {
            Vector2 center = rect.center;
            Vector2 delta = local2D - center;

            // Decide dominant axis from center to test point
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // X is dominant → snap X to edge, clamp Y
                float edgeX = (delta.x < 0f) ? rect.xMin : rect.xMax;
                float clampedY = Mathf.Clamp(local2D.y, rect.yMin, rect.yMax);
                return new Vector2(edgeX, clampedY);
            }
            else
            {
                // Y is dominant → snap Y to edge, clamp X
                float edgeY = (delta.y < 0f) ? rect.yMin : rect.yMax;
                float clampedX = Mathf.Clamp(local2D.x, rect.xMin, rect.xMax);
                return new Vector2(clampedX, edgeY);
            }
        }

        // Align closest axis to center of rect, based on direction from center
        private static Vector2 ApplyCenterAlignment(Vector2 local2D, Rect rect)
        {
            Vector2 center = rect.center;
            Vector2 delta = local2D - center;

            if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
            {
                // X is closer to center → align X to center, clamp Y
                float clampedY = Mathf.Clamp(local2D.y, rect.yMin, rect.yMax);
                return new Vector2(center.x, clampedY);
            }
            else
            {
                // Y is closer to center → align Y to center, clamp X
                float clampedX = Mathf.Clamp(local2D.x, rect.xMin, rect.xMax);
                return new Vector2(clampedX, center.y);
            }
        }

        public static Rect CreateRectWithMinimumDistanceToEdge(Rect rect, float minDistanceToEdge)
        {
            // NOTE: If minDistanceToEdge is too large (greater than half the width or height), the resulting Rect may have negative width/height.
            var newRect = new Rect(rect);
            newRect.xMin += minDistanceToEdge;
            newRect.yMin += minDistanceToEdge;
            newRect.xMax -= minDistanceToEdge;
            newRect.yMax -= minDistanceToEdge;
            return newRect;
        }

        #endregion
    }

}

