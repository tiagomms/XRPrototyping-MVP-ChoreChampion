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

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool hasSpawned = false;

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

        // Check if scene is already loaded
        if (MRUK.Instance.IsInitialized)
        {
            SpawnObjects();
        }
        else
        {
            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                SpawnObjects();
            });
        }
    }

    private void SpawnObjects()
    {
        if (hasSpawned)
        {
            Debug.LogWarning("EyeLevelSpawner: Objects already spawned, skipping duplicate spawn");
            return;
        }

        if (SpawnOnStart == MRUK.RoomFilter.AllRooms)
        {
            foreach (var room in MRUK.Instance.Rooms)
            {
                SpawnInRoom(room);
            }
        }
        else
        {
            var currentRoom = MRUK.Instance.GetCurrentRoom();
            if (currentRoom != null)
            {
                SpawnInRoom(currentRoom);
            }
            else
            {
                Debug.LogWarning("No current room found for spawning");
            }
        }

        hasSpawned = true;
    }

    private void SpawnInRoom(MRUKRoom room)
    {
        if (room == null) 
        {
            Debug.LogWarning("Room is null, skipping spawn");
            return;
        }

        if (Prefabs == null || Prefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs assigned to spawn");
            return;
        }

        foreach (var prefab in Prefabs)
        {
            if (prefab == null) 
            {
                Debug.LogWarning("Null prefab found in prefabs list, skipping");
                continue;
            }
            
            SpawnPrefabsAtEyeLevel(room, prefab);
        }
    }

    private void SpawnPrefabsAtEyeLevel(MRUKRoom room, GameObject prefab)
    {
        var bounds = Utilities.GetPrefabBounds(prefab);
        float objectRadius = bounds.HasValue
            ? Mathf.Max(bounds.Value.extents.x, bounds.Value.extents.z)
            : 0.1f;

        int successfulSpawns = 0;

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

                // Track spawned objects
                spawnedObjects.Add(spawned);

                // Clear selection to prevent Inspector issues
                #if UNITY_EDITOR
                if (UnityEditor.Selection.activeGameObject == spawned)
                    UnityEditor.Selection.activeGameObject = null;
                #endif

                // Register with tutorial - but only register the LAST spawned object
                // This prevents multiple registrations which could cause issues
                if (tutorial != null)
                {
                    tutorial.RegisterInteractable(spawned);
                    Debug.Log($"Registered {spawned.name} with tutorial");
                }
                else
                {
                    Debug.LogWarning("EyeLevelSpawner: RegularFoldTutorial reference is missing! Objects spawned but not registered with tutorial.");
                }

                placed = true;
                successfulSpawns++;
                break;
            }

            if (!placed)
                Debug.LogWarning($"{name}: Couldn't place {prefab.name} after {MaxIterations} attempts");
        }

        Debug.Log($"Successfully spawned {successfulSpawns} out of {SpawnAmount} {prefab.name} objects");
    }

    private bool TryGetEyeLevelPosition(MRUKRoom room, float objectRadius, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (UserTransform == null)
        {
            Debug.LogError("UserTransform is null in TryGetEyeLevelPosition");
            return false;
        }

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
        if (t == null || UserTransform == null) return;
        
        var dir = (UserTransform.position - pos).normalized;
        t.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    /// <summary>
    /// Call this from a UI button OnClick to start the tutorial when ready.
    /// </summary>
    public void OnStartTutorialButton()
    {
        if (tutorial != null)
        {
            tutorial.StartFoldingTutorial();
        }
        else
        {
            Debug.LogError("No RegularFoldTutorial assigned for OnStartTutorialButton()");
        }
    }

    /// <summary>
    /// Manually trigger spawning (useful for testing)
    /// </summary>
    [ContextMenu("Spawn Objects")]
    public void ManualSpawn()
    {
        if (MRUK.Instance != null && MRUK.Instance.IsInitialized)
        {
            // Reset spawn state to allow manual respawning
            hasSpawned = false;
            SpawnObjects();
        }
        else
        {
            Debug.LogError("MRUK is not initialized. Cannot spawn objects.");
        }
    }

    /// <summary>
    /// Clear all spawned objects
    /// </summary>
    [ContextMenu("Clear Spawned Objects")]
    public void ClearSpawnedObjects()
    {
        #if UNITY_EDITOR
        // Clear selection to avoid inspector errors
        UnityEditor.Selection.activeGameObject = null;
        #endif
        
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                #if UNITY_EDITOR
                if (UnityEditor.Selection.activeGameObject == obj)
                    UnityEditor.Selection.activeGameObject = null;
                #endif
                
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
        }
        
        spawnedObjects.Clear();
        hasSpawned = false;
        Debug.Log("Cleared all spawned objects");
    }

    /// <summary>
    /// Reset spawner state without destroying objects
    /// </summary>
    public void ResetSpawner()
    {
        hasSpawned = false;
        spawnedObjects.Clear();
    }

    private void OnDestroy()
    {
        // Clean up tracked objects
        spawnedObjects.Clear();
    }
}