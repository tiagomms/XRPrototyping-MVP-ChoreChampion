using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AOT;
using Unity.Collections;
using UnityEngine.Android;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using System.Linq;
using Meta.XR.MRUtilityKit;

namespace ChoreChampion.XR.MRUtilityKit
{
    public class MRUKExtension
    {
        /// <summary>
        /// Public copy Surface used in MRUK (it is internal only there)
        /// </summary>
        public struct Surface
        {
            public MRUKAnchor Anchor;
            public float UsableArea;
            public bool IsPlane;
            public Rect Bounds;
            public Matrix4x4 Transform;
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
        /// FROM MRUKRoom.cs - extracted from GenerateRandomPositionOnSurface - since we are using the same anchor over and over again, no need to calculate it every time
        /// </summary>
        /// <param name="surfaceTypes"></param>
        /// <param name="minDistanceToEdge"></param>
        /// <param name="anchor"></param>
        /// <param name="totalUsableSurfaceArea"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static List<Surface> GetAnchorSurfaces(MRUK.SurfaceType surfaceTypes, float minDistanceToEdge, MRUKAnchor anchor, ref float totalUsableSurfaceArea)
        {
            List<Surface> surfaces = new();
            float minWidth = 2f * minDistanceToEdge;
            if (anchor.PlaneRect.HasValue)
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
                            Anchor = anchor,
                            UsableArea = usableArea,
                            IsPlane = true,
                            Bounds = anchor.PlaneRect.Value,
                            Transform = anchor.transform.localToWorldMatrix
                        });
                    }
                }
            }

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
                            Anchor = anchor,
                            UsableArea = usableArea,
                            IsPlane = false,
                            Bounds = bounds,
                            Transform = anchor.transform.localToWorldMatrix * faceTransform
                        });
                    }
                }
            }

            return surfaces;
        }
    }

}

