using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using UnityEngine;

public class EyeLevelSpawner : MonoBehaviour
{
    [Header("Room Filter")]
    public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

    [Header("Eye Level Spawn Settings")]
    public Transform UserTransform;
    public float DistanceFromUser = 1.5f;
    public float EyeLevelOffset = 0f;
    public float HorizontalSpread = 0.5f;
    public float VerticalSpread = 0.3f;
    public bool UseRoomBounds = true;
    public bool AvoidObstacles = true;
    public LayerMask ObstacleLayerMask = -1;

    [Header("Spawn Configuration")]
    [Min(1)] public int SpawnAmount = 1;
    [Min(1)] public int MaxIterations = 100;
    public bool CheckOverlaps = true;
    public LayerMask OverlapLayerMask = -1;
    public List<GameObject> Prefabs = new();

    [Header("References")]
    [Tooltip("Drag your RegularFoldTutorial here")]
    [SerializeField] private RegularFoldTutorial tutorial;

    void Start()
    {
        if (UserTransform == null)
            UserTransform = Camera.main?.transform;
        if (UserTransform == null)
        {
            Debug.LogError("No UserTransform assigned or MainCamera found!");
            return;
        }

        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK not present");
            return;
        }

        MRUK.Instance.RegisterSceneLoadedCallback(() =>
        {
            if (SpawnOnStart == MRUK.RoomFilter.AllRooms)
                foreach (var room in MRUK.Instance.Rooms)
                    SpawnInRoom(room);
            else
                SpawnInRoom(MRUK.Instance.GetCurrentRoom());
        });
    }

    private void SpawnInRoom(MRUKRoom room)
    {
        if (room == null) return;
        foreach (var prefab in Prefabs)
        {
            if (prefab == null || prefab.scene.IsValid()) continue;
            SpawnPrefabsAtEyeLevel(room, prefab);
        }
    }

    private void SpawnPrefabsAtEyeLevel(MRUKRoom room, GameObject prefab)
    {
        var bounds = Utilities.GetPrefabBounds(prefab);
        float objectRadius = bounds.HasValue
            ? Mathf.Max(bounds.Value.extents.x, bounds.Value.extents.z)
            : 0.1f;

        for (int n = 0; n < SpawnAmount; n++)
        {
            bool placed = false;
            for (int attempt = 0; attempt < MaxIterations; attempt++)
            {
                if (!TryGetEyeLevelPosition(room, objectRadius, out var pos, out var rot))
                    continue;

                if (CheckOverlaps && bounds.HasValue &&
                    Physics.CheckBox(pos + rot * bounds.Value.center,
                                     bounds.Value.extents,
                                     rot,
                                     OverlapLayerMask,
                                     QueryTriggerInteraction.Ignore))
                    continue;

                var spawned = Instantiate(prefab, pos, rot, transform);
                FaceUser(spawned.transform, pos);

                if (tutorial != null)
                    tutorial.RegisterInteractable(spawned);
                else
                    Debug.LogError("EyeLevelSpawner: RegularFoldTutorial reference is missing!");

                placed = true;
                break;
            }

            if (!placed)
                Debug.LogWarning($"{name}: Couldn't place {prefab.name} after {MaxIterations} attempts");
        }
    }

    private bool TryGetEyeLevelPosition(MRUKRoom room, float objectRadius, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        Vector3 up      = UserTransform.up;
        Vector3 forward = UserTransform.forward;
        Vector3 right   = UserTransform.right;
        Vector3 basePos = UserTransform.position + forward * DistanceFromUser + up * EyeLevelOffset;

        Vector3 horOff = right * UnityEngine.Random.Range(-HorizontalSpread, HorizontalSpread);
        Vector3 verOff = up    * UnityEngine.Random.Range(-VerticalSpread, VerticalSpread);
        Vector3 target = basePos + horOff + verOff;

        if (UseRoomBounds && room != null && !room.IsPositionInRoom(target))
            return false;

        if (AvoidObstacles)
        {
            float dist = Vector3.Distance(UserTransform.position, target);
            if (Physics.SphereCast(UserTransform.position, objectRadius, (target - UserTransform.position).normalized,
                                   out _, dist, ObstacleLayerMask, QueryTriggerInteraction.Ignore))
                return false;
        }

        position = target;
        rotation = Quaternion.LookRotation(forward, up);
        return true;
    }

    private void FaceUser(Transform t, Vector3 pos)
    {
        var dir = (UserTransform.position - pos).normalized;
        t.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    /// <summary>
    /// Call this from a UI button OnClick to start the tutorial when ready.
    /// </summary>
    public void OnStartTutorialButton()
    {
        if (tutorial != null)
            tutorial.StartFoldingTutorial();
        else
            Debug.LogError("No RegularFoldTutorial assigned for OnStartTutorialButton()");
    }
}
