using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using UnityEngine;

public class MultiItemSpawner : MonoBehaviour
{
    /* ── GLOBAL FILTER ────────────────────────────────────────── */
    public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

    /* ── FRONT SURFACE DETECTION ─────────────────────────────── */
    [Header("Front Surface Detection")]
    public Transform UserTransform; // Assign main camera or user's head transform
    public float MaxAngleFromForward = 45f; // Maximum angle from user's forward direction
    public float MinDistance = 0.5f; // Minimum distance from user
    public float MaxDistance = 3f; // Maximum distance from user

    /* ── GROUP CONFIG ─────────────────────────────────────────── */
    [Serializable]
    public class LabelGroup
    {
        public MRUKAnchor.SceneLabels Labels = MRUKAnchor.SceneLabels.TABLE;

        public enum SpawnLocation
        {
            Floating         = 0,
            AnySurface       = 1,
            VerticalSurfaces = 2,
            OnTopOfSurfaces  = 3,
            HangingDown      = 4
        }
        public SpawnLocation Surface = SpawnLocation.OnTopOfSurfaces;

        [Min(1)] public int  SpawnAmount   = 1;
        [Min(1)] public int  MaxIterations = 1000;

        public bool  CheckOverlaps            = true;
        public float SurfaceClearanceDistance = 0.1f;
        public float OverrideBounds           = -1;
        public LayerMask LayerMask            = -1;

        public List<GameObject> Prefabs = new();
    }

    public List<LabelGroup> Groups = new();

    /* ── INITIALISE ───────────────────────────────────────────── */
    private void Start()
    {
        // Auto-assign user transform if not set
        if (UserTransform == null)
        {
            UserTransform = Camera.main?.transform;
            if (UserTransform == null)
            {
                Debug.LogError("No UserTransform assigned and no main camera found!");
                return;
            }
        }

        if (MRUK.Instance == null) { Debug.LogError("MRUK not present"); return; }

        MRUK.Instance.RegisterSceneLoadedCallback(() =>
        {
            switch (SpawnOnStart)
            {
                case MRUK.RoomFilter.AllRooms:
                    foreach (var room in MRUK.Instance.Rooms) SpawnInRoom(room);
                    break;
                case MRUK.RoomFilter.CurrentRoomOnly:
                    SpawnInRoom(MRUK.Instance.GetCurrentRoom());
                    break;
            }
        });
    }

    /* ── PER-ROOM LOOP ────────────────────────────────────────── */
    private void SpawnInRoom(MRUKRoom room)
    {
        if (room == null) return;

        foreach (var g in Groups)
        {
            foreach (var prefab in g.Prefabs)
            {
                if (prefab == null || prefab.scene.IsValid()) continue;
                SpawnPrefab(room, g, prefab);
            }
        }
    }

    /* ── SPAWN ONE PREFAB ─────────────────────────────────────── */
    private void SpawnPrefab(MRUKRoom room, LabelGroup g, GameObject prefab)
    {
        var bounds = Utilities.GetPrefabBounds(prefab);
        float yOffset = -bounds?.min.y ?? 0f;
        float radius  = g.OverrideBounds > 0
                        ? g.OverrideBounds
                        : bounds.HasValue ? Mathf.Max(bounds.Value.extents.x, bounds.Value.extents.z)
                        : 0f;

        for (int n = 0; n < g.SpawnAmount; ++n)
        {
            bool placed = false;

            for (int attempt = 0; attempt < g.MaxIterations; ++attempt)
            {
                if (!TryPickPointInFrontOfUser(room, g, radius, yOffset,
                                              out var pos, out var normal, out var anchor))
                    continue;

                if (g.CheckOverlaps && bounds.HasValue &&
                    Physics.CheckBox(pos + Quaternion.FromToRotation(Vector3.up, normal) * bounds.Value.center,
                                     bounds.Value.extents,
                                     Quaternion.FromToRotation(Vector3.up, normal),
                                     g.LayerMask,
                                     QueryTriggerInteraction.Ignore))
                    continue;

                Instantiate(prefab,
                            pos,
                            Quaternion.FromToRotation(Vector3.up, normal),
                            anchor.transform);

                placed = true;
                break;
            }

            if (!placed)
            {
                Debug.LogWarning($"{name}: couldn't place {prefab.name} in {room.name} in front of user");
                break;
            }
        }
    }

    /* ── PICK RANDOM SURFACE POINT IN FRONT OF USER ─────────── */
    private bool TryPickPointInFrontOfUser(
        MRUKRoom room, LabelGroup g, float minRadius, float baseOffset,
        out Vector3 worldPos, out Vector3 normal, out MRUKAnchor anchor)
    {
        worldPos = normal = Vector3.zero;
        anchor   = null;

        // Get all valid anchors in front of user first
        var validAnchors = GetAnchorsInFrontOfUser(room, g.Labels);
        if (validAnchors.Count == 0)
        {
            Debug.LogWarning("No valid anchors found in front of user");
            return false;
        }

        // Try to spawn on one of the valid anchors
        for (int i = 0; i < 10; i++) // Try up to 10 random anchors
        {
            var randomAnchor = validAnchors[UnityEngine.Random.Range(0, validAnchors.Count)];
            
            if (TryPickPointOnAnchor(randomAnchor, g, minRadius, baseOffset, 
                                   out worldPos, out normal))
            {
                anchor = randomAnchor;
                return true;
            }
        }

        return false;
    }

    /* ── GET ANCHORS IN FRONT OF USER ────────────────────────── */
    private List<MRUKAnchor> GetAnchorsInFrontOfUser(MRUKRoom room, MRUKAnchor.SceneLabels targetLabels)
    {
        var validAnchors = new List<MRUKAnchor>();
        Vector3 userPos = UserTransform.position;
        Vector3 userForward = UserTransform.forward;

        foreach (var anchor in room.Anchors)
        {
            // Check if anchor has the correct label
            if (!anchor.Label.HasFlag(targetLabels)) continue;

            // Get anchor position (center of bounds or plane)
            Vector3 anchorPos = anchor.transform.position;
            if (anchor.VolumeBounds.HasValue)
            {
                anchorPos = anchor.transform.TransformPoint(anchor.VolumeBounds.Value.center);
            }
            else if (anchor.PlaneRect.HasValue)
            {
                anchorPos = anchor.transform.TransformPoint(anchor.PlaneRect.Value.center);
            }

            // Check distance
            float distance = Vector3.Distance(userPos, anchorPos);
            if (distance < MinDistance || distance > MaxDistance) continue;

            // Check angle (is it in front of user?)
            Vector3 directionToAnchor = (anchorPos - userPos).normalized;
            float angle = Vector3.Angle(userForward, directionToAnchor);
            
            if (angle <= MaxAngleFromForward)
            {
                validAnchors.Add(anchor);
            }
        }

        return validAnchors;
    }

    /* ── PICK POINT ON SPECIFIC ANCHOR ───────────────────────── */
    private bool TryPickPointOnAnchor(MRUKAnchor anchor, LabelGroup g, float minRadius, float baseOffset,
                                     out Vector3 worldPos, out Vector3 normal)
    {
        worldPos = normal = Vector3.zero;

        // Handle floating spawn location
        if (g.Surface == LabelGroup.SpawnLocation.Floating)
        {
            if (anchor.VolumeBounds.HasValue)
            {
                var bounds = anchor.VolumeBounds.Value;
                var localPos = new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                    UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
                );
                worldPos = anchor.transform.TransformPoint(localPos);
                normal = Vector3.up;
                return true;
            }
            return false;
        }

        // Surface-type mask
        MRUK.SurfaceType surfaceMask = g.Surface switch
        {
            LabelGroup.SpawnLocation.AnySurface       => MRUK.SurfaceType.FACING_UP |
                                                         MRUK.SurfaceType.VERTICAL  |
                                                         MRUK.SurfaceType.FACING_DOWN,
            LabelGroup.SpawnLocation.VerticalSurfaces => MRUK.SurfaceType.VERTICAL,
            LabelGroup.SpawnLocation.OnTopOfSurfaces  => MRUK.SurfaceType.FACING_UP,
            LabelGroup.SpawnLocation.HangingDown      => MRUK.SurfaceType.FACING_DOWN,
            _                                         => 0
        };

        // Try to get a random position on this specific anchor's surface
        if (anchor.PlaneRect.HasValue)
        {
            var rect = anchor.PlaneRect.Value;
            var localPos = new Vector3(
                UnityEngine.Random.Range(rect.xMin + minRadius, rect.xMax - minRadius),
                0,
                UnityEngine.Random.Range(rect.yMin + minRadius, rect.yMax - minRadius)
            );
            
            worldPos = anchor.transform.TransformPoint(localPos);
            normal = anchor.transform.up;
            
            // Apply surface type filtering
            bool validSurface = false;
            if (surfaceMask.HasFlag(MRUK.SurfaceType.FACING_UP) && Vector3.Dot(normal, Vector3.up) > 0.7f)
                validSurface = true;
            else if (surfaceMask.HasFlag(MRUK.SurfaceType.VERTICAL) && Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.3f)
                validSurface = true;
            else if (surfaceMask.HasFlag(MRUK.SurfaceType.FACING_DOWN) && Vector3.Dot(normal, Vector3.up) < -0.7f)
                validSurface = true;

            if (validSurface)
            {
                worldPos += normal * baseOffset;
                return true;
            }
        }

        return false;
    }
}