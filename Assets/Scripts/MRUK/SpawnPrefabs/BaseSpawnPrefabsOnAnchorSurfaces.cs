using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using NaughtyAttributes;
using UnityEngine.Serialization;

namespace ChoreChampion.XR.MRUtilityKit
{
    /// <summary>
    /// Base class for managing spawn positions relative to game anchors.
    /// Provides shared fields, anchor detection, and extensibility points for spawn logic.
    /// </summary>
    public abstract class BaseSpawnPrefabsOnAnchorSurfaces : MonoBehaviour
    {
        [Tooltip("Class that selects Game Anchor.")]
        [SerializeField] protected GameAnchorSelection gameAnchorSelection;

        /// <summary>
        /// Volume/Plane where our game will play out
        /// </summary>
        [Tooltip("Volume/Plane where our game will play out. In the end it should be selected by user.")]
        [SerializeField] protected MRUKAnchor _gameAnchor;

        /// <summary>
        /// When the scene data is loaded, this controls what room(s) the prefabs will spawn in.
        /// </summary>
        [Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
        public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

        /// <summary>
        /// Prefab to be placed into the scene, or object in the scene to be moved around.
        /// </summary>
        [SerializeField, Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
        public GameObject SpawnObject;

        /// <summary>
        /// Number of SpawnObject(s) to place into the scene per room, only applies to Prefabs.
        /// </summary>
        [SerializeField, Tooltip("Number of SpawnObject(s) to place into the scene per room, only applies to Prefabs.")]
        public int SpawnAmount = 1000;

        /// <summary>
        /// Maximum number of times to attempt spawning/moving an object before giving up.
        /// </summary>
        [SerializeField, Tooltip("Maximum number of times to attempt spawning/moving an object before giving up.")]
        public int MaxIterations = 1000;

        /// <summary>
        /// Attach content to scene surfaces.
        /// </summary>
        [SerializeField, Tooltip("Attach content to scene surfaces.")]
        public MRUKSpawnLocation SpawnLocations = MRUKSpawnLocation.OnTopOfSurfaces;

        /// <summary>
        /// If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.
        /// </summary>
        [SerializeField, Tooltip("If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.")]
        public bool CheckOverlaps = true;

        /// <summary>
        /// Required free space for the object (Set negative to auto-detect using GetPrefabBounds)
        /// default to auto-detect. This value represents the extents of the bounding box
        /// </summary>
        [SerializeField, Tooltip("Required free space for the object (Set negative to auto-detect using GetPrefabBounds)")]
        public float OverrideBounds = -1;

        /// <summary>
        /// Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.
        /// </summary>
        [FormerlySerializedAs("layerMask")]
        [SerializeField, Tooltip("Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.")]
        public LayerMask LayerMask = -1;

        /// <summary>
        /// The clearance distance required in front of the surface in order for it to be considered a valid spawn position
        /// </summary>
        [SerializeField, Tooltip("The clearance distance required in front of the surface in order for it to be considered a valid spawn position")]
        public float SurfaceClearanceDistance = 0.1f;

        /// <summary>
        /// If true, spawned objects become children of the anchor; if false, they are child of this gameObject.
        /// </summary>
        [SerializeField, Tooltip("If true, spawned objects become children of the anchor; if false, they remain in world space.")]
        protected bool parentToAnchor = false;

        /// <summary>
        /// If true, allows stretching objects to match anchor scale (Vector3.one).
        /// </summary>
        [SerializeField, Tooltip("If true, allows stretching objects to match anchor scale (Vector3.one).")]
        protected bool allowStretch = false;

        /// <summary>
        /// Bounds of the adjusted prefab bounds.
        /// </summary>
        protected Bounds _adjustedBounds;
        protected Bounds? _prefabBounds;
        protected float _minRadius;
        protected float _baseOffset;
        protected float _centerOffset;

        /// <summary>
        /// List of game anchor surfaces
        /// </summary>
        protected List<MRUKExtension.Surface> _gameAnchorSurfaces;

        /// <summary>
        /// Current number of spawned objects.
        /// </summary>
        protected int _currentSpawnedObjects;
        public int CurrentSpawnedObjects => _currentSpawnedObjects;

        /// <summary>
        /// Unity Start: calculates prefab bounds and subscribes to anchor selection events.
        /// </summary>
        protected virtual void Start()
        {
            // NOTE: important part - calculates prefab bounds used here
            CalculatePrefabBounds();
            if (gameAnchorSelection != null)
            {
                gameAnchorSelection.onSelectGameAnchor.AddListener(SetGameAnchor);
            }
        }

        /// <summary>
        /// Unity OnDestroy: unsubscribes from anchor selection events.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (gameAnchorSelection != null)
            {
                gameAnchorSelection.onSelectGameAnchor.RemoveListener(SetGameAnchor);
            }
        }

        /// <summary>
        /// Spawns objects on the current room.
        /// </summary>
        [Button]
        public virtual void SpawnOnCurrentRoom()
        {
            if (MRUK.Instance && MRUK.Instance.IsInitialized)
            {
                var currentRoom = MRUK.Instance.GetCurrentRoom();
                if (_gameAnchor == null)
                {
                    Debug.LogError($"[{GetType().Name}] - ERROR: No GameAnchor defined yet. Please set one before proceeding");
                    return;
                }

                SpawnObjectsInGameAnchor(currentRoom, _gameAnchor);
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] - MRUK not initialized yet");
            }
        }

        /// <summary>
        /// In case we don't have GameAnchorSelection - we need to set one
        /// </summary>
        [Button]
        public virtual void SetHardcodedGameAnchor()
        {
            SetGameAnchor(MRUKExtension.GetClosestAnchorBasedOnSurfacePosition(null, FindFirstObjectByType<Camera>().transform.position));
        }

        /// <summary>
        /// Sets the game anchor reference.
        /// </summary>
        /// <param name="anchor">Anchor to set.</param>
        public virtual void SetGameAnchor(MRUKAnchor anchor)
        {
            _gameAnchor = anchor;
        }

        /// <summary>
        /// Sets the prefab to be spawned and triggers any necessary recalculation.
        /// </summary>
        /// <param name="newPrefab">The new prefab to spawn.</param>
        public virtual void SetNewPrefab(GameObject newPrefab)
        {
            SpawnObject = newPrefab;
            CalculatePrefabBounds();
        }

        /// <summary>
        /// Sets the spawn amount.
        /// </summary>
        /// <param name="newMax">New maximum spawn amount.</param>
        public virtual void SetSpawnAmount(int newMax)
        {
            SpawnAmount = newMax;
        }

        /// <summary>
        /// Utility for derived classes: checks if the anchor is valid.
        /// </summary>
        /// <returns>True if anchor is valid, false otherwise.</returns>
        protected bool HasValidAnchor()
        {
            return _gameAnchor != null;
        }

        /// <summary>
        /// FROM FindSpawnPositions.cs - get prefab bounds and calculate adjusted bounds in order to reuse them later on.
        /// </summary>
        protected virtual void CalculatePrefabBounds()
        {
            _prefabBounds = Utilities.GetPrefabBounds(SpawnObject);
            _minRadius = 0.0f;
            const float clearanceDistance = 0.01f;
            _baseOffset = -_prefabBounds?.min.y ?? 0.0f;
            _centerOffset = _prefabBounds?.center.y ?? 0.0f;
            _adjustedBounds = new();

            if (_prefabBounds.HasValue)
            {
                _minRadius = Mathf.Min(-_prefabBounds.Value.min.x, -_prefabBounds.Value.min.z, _prefabBounds.Value.max.x, _prefabBounds.Value.max.z);
                if (_minRadius < 0f)
                {
                    _minRadius = 0f;
                }

                var min = _prefabBounds.Value.min;
                var max = _prefabBounds.Value.max;
                min.y += clearanceDistance;
                if (max.y < min.y)
                {
                    max.y = min.y;
                }

                _adjustedBounds.SetMinMax(min, max);
                if (OverrideBounds > 0)
                {
                    Vector3 center = new Vector3(0f, clearanceDistance, 0f);
                    Vector3 size = new Vector3(OverrideBounds * 2f, clearanceDistance * 2f, OverrideBounds * 2f); // OverrideBounds represents the extents, not the size
                    _adjustedBounds = new Bounds(center, size);
                }
            }
        }

        /// <summary>
        /// Initializes anchor surfaces if not already done.
        /// </summary>
        /// <param name="surfaceTypes">Type of surfaces to get.</param>
        /// <param name="minDistanceToEdge">Minimum distance to edge.</param>
        /// <param name="anchor">Anchor to get surfaces from.</param>
        /// <returns>True if surfaces were initialized successfully, false otherwise.</returns>
        private bool InitializeAnchorSurfaces(MRUK.SurfaceType surfaceTypes, float minDistanceToEdge, MRUKAnchor anchor)
        {
            if (_gameAnchorSurfaces == null)
            {
                float totalUsableSurfaceArea = 0f;
                _gameAnchorSurfaces = MRUKExtension.GetAnchorSurfaces(surfaceTypes, minDistanceToEdge, anchor, ref totalUsableSurfaceArea);
                if (_gameAnchorSurfaces.Count == 0)
                {
                    Debug.LogError($"[{GetType().Name} - {nameof(_gameAnchorSurfaces)}]: Anchor {anchor.name} does not have surfaces!");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// FROM MRUKRoom.cs - copy of GenerateRandomPositionOnSurface, but for a single Anchor
        /// </summary>
        /// <param name="surfaceTypes"></param>
        /// <param name="minDistanceToEdge"></param>
        /// <param name="anchor">Anchor to generate random position</param>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public virtual bool GenerateRandomPositionOnSpecificSurfaceAnchor(MRUK.SurfaceType surfaceTypes, float minDistanceToEdge, MRUKAnchor anchor, out Vector3 position, out Vector3 normal)
        {
            // define these as the negative early exit conditions
            position = Vector3.zero;
            normal = Vector3.zero;

            if (!InitializeAnchorSurfaces(surfaceTypes, minDistanceToEdge, anchor))
            {
                return false;
            }

            float totalUsableSurfaceArea = 0f;
            for (int i = 0; i < _gameAnchorSurfaces.Count; i++)
            {
                totalUsableSurfaceArea += _gameAnchorSurfaces[i].UsableArea;
            }

            for (int i = 0; i < MaxIterations; ++i)
            {
                // Pick a random surface weighted by surface area (_gameAnchorSurfaces with a larger
                // area have more chance of being chosen)
                var rand = UnityEngine.Random.Range(0, totalUsableSurfaceArea);
                int index = 0;
                for (; index < _gameAnchorSurfaces.Count - 1; ++index)
                {
                    rand -= _gameAnchorSurfaces[index].UsableArea;
                    if (rand <= 0.0f)
                    {
                        break;
                    }
                }

                var surface = _gameAnchorSurfaces[index];
                var bounds = surface.Bounds;
                Vector2 pos = new Vector2(
                    UnityEngine.Random.Range(bounds.xMin + minDistanceToEdge, bounds.xMax - minDistanceToEdge),
                    UnityEngine.Random.Range(bounds.yMin + minDistanceToEdge, bounds.yMax - minDistanceToEdge)
                );

                if (surface.IsPlane && !surface.Anchor.IsPositionInBoundary(pos))
                {
                    continue;
                }

                //Debug.Log($"pos: {pos}");
                position = surface.Transform.MultiplyPoint3x4(new Vector3(pos.x, pos.y, 0f));
                normal = surface.Transform.MultiplyVector(Vector3.forward);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a fixed position on a specific surface anchor using provided local coordinates.
        /// </summary>
        /// <param name="surfaceTypes">Type of surface to spawn on.</param>
        /// <param name="localPosition">Local position on the surface (x,y coordinates).</param>
        /// <param name="minDistanceToEdge">Minimum distance to edge.</param>
        /// <param name="anchor">Anchor to generate position on.</param>
        /// <param name="position">Output world position.</param>
        /// <param name="normal">Output surface normal.</param>
        /// <returns>True if position was generated successfully, false otherwise.</returns>
        public virtual bool GenerateFixedPositionOnSpecificSurfaceAnchor(MRUK.SurfaceType surfaceTypes, Vector2 localPosition, float minDistanceToEdge, MRUKAnchor anchor, out Vector3 position, out Vector3 normal)
        {
            // define these as the negative early exit conditions
            position = Vector3.zero;
            normal = Vector3.zero;

            // FIXME: ignoring minDistanceToEdge right now due to rotation issues ahead
            if (!InitializeAnchorSurfaces(surfaceTypes, 0f, anchor))
            {
                return false;
            }

            // Use the first surface for fixed positioning (or could be made configurable)
            if (_gameAnchorSurfaces.Count > 0)
            {
                var surface = _gameAnchorSurfaces[0];
                var bounds = surface.Bounds;

                // Lerp local position from [-0.5, 0.5] range to bounds coordinates respecting minDistanceToEdge
                // localPosition.x = -0.5 maps to bounds.xMin + minDistanceToEdge
                // localPosition.x = 0.5 maps to bounds.xMax - minDistanceToEdge
                float mappedX = Mathf.Lerp(bounds.xMin + 0f, bounds.xMax - 0f, localPosition.x + 0.5f);
                float mappedY = Mathf.Lerp(bounds.yMin + 0f, bounds.yMax - 0f, localPosition.y + 0.5f);

                Vector2 mappedPosition = new Vector2(mappedX, mappedY);

                if (surface.IsPlane && !surface.Anchor.IsPositionInBoundary(mappedPosition))
                {
                    return false;
                }

                position = surface.Transform.MultiplyPoint3x4(new Vector3(mappedPosition.x, mappedPosition.y, 0f));
                normal = surface.Transform.MultiplyVector(Vector3.forward);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called to spawn objects in the anchor. Derived classes must implement this.
        /// </summary>
        /// <param name="room">Room context for spawning.</param>
        /// <param name="anchor">Anchor to spawn on.</param>
        /// <returns>True if successful, false otherwise.</returns>
        protected bool SpawnObjectsInGameAnchor(MRUKRoom room, MRUKAnchor anchor)
        {
            int i = _currentSpawnedObjects;
            while (i < SpawnAmount)
            {
                bool foundValidSpawnPosition = false;
                for (int j = 0; j < MaxIterations; ++j)
                {
                    Vector3 spawnPosition = Vector3.zero;
                    Vector3 spawnNormal = Vector3.zero;
                    if (SpawnLocations == MRUKSpawnLocation.Floating)
                    {
                        var randomPos = room.GenerateRandomPositionInRoom(_minRadius, true);
                        if (!randomPos.HasValue)
                        {
                            break;
                        }

                        spawnPosition = randomPos.Value;
                    }
                    else
                    {
                        MRUK.SurfaceType surfaceType = 0;
                        switch (SpawnLocations)
                        {
                            case MRUKSpawnLocation.AnySurface:
                                surfaceType |= MRUK.SurfaceType.FACING_UP;
                                surfaceType |= MRUK.SurfaceType.VERTICAL;
                                surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                                break;
                            case MRUKSpawnLocation.VerticalSurfaces:
                                surfaceType |= MRUK.SurfaceType.VERTICAL;
                                break;
                            case MRUKSpawnLocation.OnTopOfSurfaces:
                                surfaceType |= MRUK.SurfaceType.FACING_UP;
                                break;
                            case MRUKSpawnLocation.HangingDown:
                                surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                                break;
                        }

                        if (!CalculateSpawnPositionAndNormal(surfaceType, anchor, out spawnPosition, out spawnNormal, j))
                        {
                            continue;
                        }

                        var center = spawnPosition + spawnNormal * _centerOffset;
                        //Debug.Log($"Spawn - Position: {spawnPosition}, Normal: {spawnNormal}, Center: {center}");
                        //Debug.Log($"Prefab - Bounds: {_prefabBounds}, adjusted: {_adjustedBounds}, base offset: {_baseOffset}");
                        if (!room.IsPositionInRoom(center))
                        {
                            //Debug.Log($"Spawn CONTINUE ITERATION - Not in room");
                            continue;
                        }

                        if (room.IsPositionInSceneVolume(center))
                        {
                            //Debug.Log($"Spawn CONTINUE ITERATION - InSceneVolume");
                            continue;
                        }

                        if (room.Raycast(new Ray(spawnPosition - spawnNormal * _baseOffset, spawnNormal), SurfaceClearanceDistance, out _))
                        {
                            //Debug.Log($"Spawn CONTINUE ITERATION - Raycast thing");
                            continue;
                        }
                    }

                    // Calculate world rotation based on surface normal
                    Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);

                    if (CheckOverlaps && _prefabBounds.HasValue)
                    {
                        if (Physics.CheckBox(spawnPosition + spawnRotation * _adjustedBounds.center, _adjustedBounds.extents, spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                        {
                            continue;
                        }
                    }

                    foundValidSpawnPosition = true;

                    bool shouldContinue = InstantiateOrMoveObject(spawnPosition, spawnRotation);
                    if (!shouldContinue)
                    {
                        return false;
                    }

                    break;
                }

                if (!foundValidSpawnPosition)
                {
                    Debug.LogWarning($"Failed to find valid spawn position after {MaxIterations} iterations. Only spawned {i} prefabs instead of {SpawnAmount}.");
                    break;
                }

                ++i;
            }
            _currentSpawnedObjects = i;

            return true;
        }

        /// <summary>
        /// Calculates spawn position and normal. Derived classes can override this to change positioning logic.
        /// </summary>
        /// <param name="surfaceType">Type of surface to spawn on.</param>
        /// <param name="anchor">Anchor to spawn on.</param>
        /// <param name="spawnPosition">Output spawn position.</param>
        /// <param name="spawnNormal">Output spawn normal.</param>
        /// <param name="iteration">Current iteration number for incremental offset calculation.</param>
        /// <returns>True if position was calculated successfully, false otherwise.</returns>
        protected abstract bool CalculateSpawnPositionAndNormal(MRUK.SurfaceType surfaceType, MRUKAnchor anchor, out Vector3 spawnPosition, out Vector3 spawnNormal, int iteration = 0);

        /// <summary>
        /// Instantiates or moves the spawn object. Derived classes can override this to change instantiation logic.
        /// </summary>
        /// <param name="spawnPosition">Position to spawn at.</param>
        /// <param name="spawnRotation">Rotation to spawn with.</param>
        /// <returns>True to continue spawning, false to stop (for moving existing objects).</returns>
        protected virtual bool InstantiateOrMoveObject(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (SpawnObject.gameObject.scene.path == null)
            {
                // Instantiate new object
                Transform parentTransform = parentToAnchor ? GetAnchorGameObjectTransform() : transform;
                GameObject spawnedObject = Instantiate(SpawnObject, spawnPosition, spawnRotation, parentTransform);

                // When parenting to anchor, set local rotation to identity (inherits anchor's rotation)
                if (parentToAnchor)
                {
                    spawnedObject.transform.localRotation = RoundRotationToNearest90Degrees(spawnedObject.transform.localRotation);
                }

                // Apply stretching if enabled
                if (allowStretch && parentToAnchor)
                {
                    spawnedObject.transform.localScale = Vector3.one;
                }

                return true;
            }
            else
            {
                // Move existing object
                if (parentToAnchor)
                {
                    SpawnObject.transform.SetParent(GetAnchorGameObjectTransform());
                    SpawnObject.transform.localRotation = RoundRotationToNearest90Degrees(SpawnObject.transform.localRotation);
                }
                else
                {
                    SpawnObject.transform.rotation = spawnRotation;
                }

                SpawnObject.transform.position = spawnPosition;


                // Apply stretching if enabled
                if (allowStretch && parentToAnchor)
                {
                    SpawnObject.transform.localScale = Vector3.one;
                }

                return false; // ignore SpawnAmount once we have a successful move of existing object in the scene
            }

            Transform GetAnchorGameObjectTransform()
            {
                var anchorGameObject = gameAnchorSelection.AnchorPrefabSpawnerObjects[_gameAnchor];
                var parentIdentifier = anchorGameObject.GetComponentInChildren<RuntimeSpawnObjectsParentIdentifier>();
                return parentIdentifier.transform ?? anchorGameObject.transform;
            }
        }

        /// <summary>
        /// Rounds a quaternion rotation to the nearest 90-degree increment (0°, 90°, 180°, 270°).
        /// This helps avoid small tilting offsets when parenting objects to anchors.
        /// </summary>
        /// <param name="rotation">The quaternion to round.</param>
        /// <returns>A quaternion rounded to the nearest 90-degree increment.</returns>
        protected Quaternion RoundRotationToNearest90Degrees(Quaternion rotation)
        {
            // Convert quaternion to euler angles
            Vector3 eulerAngles = rotation.eulerAngles;

            // Round each axis to nearest 90 degrees
            float roundedX = Mathf.Round(eulerAngles.x / 90f) * 90f;
            float roundedY = Mathf.Round(eulerAngles.y / 90f) * 90f;
            float roundedZ = Mathf.Round(eulerAngles.z / 90f) * 90f;

            // Convert back to quaternion
            return Quaternion.Euler(roundedX, roundedY, roundedZ);
        }

    }
}