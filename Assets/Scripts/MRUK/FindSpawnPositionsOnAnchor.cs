/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using ChoreChampion.XR.MRUtilityKit;
using Meta.XR.Util;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit
{
    /// <summary>
    /// Allows for fast generation of valid (inside the room, outside furniture bounds) random positions for content spawning.
    /// Optional method to pin directly to surfaces.
    /// This class leverages the <see cref="MRUKRoom.GenerateRandomPositionInRoom"/> and <see cref="MRUKRoom.GenerateRandomPositionOnSurface"/> methods
    /// to provide a simple interface for spawning content in the room.
    /// </summary>
    public class FindSpawnPositionsOnAnchor : MonoBehaviour
    {
        [Tooltip("Class that selects Game Anchor.")]
        [SerializeField] private GameAnchorSelection gameAnchorSelection;

        /// <summary>
        /// Volume/Plane where our game will play out
        /// </summary>
        [Tooltip("Volume/Plane where our game will play out. In the end it should be selected by user.")]
        [SerializeField] private MRUKAnchor _gameAnchor;

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
        /// Defines possible locations where objects can be spawned.
        /// </summary>
        public enum SpawnLocation
        {
            Floating, // Spawn somewhere floating in the free space within the room
            AnySurface, // Spawn on any surface (i.e. a combination of all 3 options below)
            VerticalSurfaces, // Spawn only on vertical surfaces such as walls, windows, wall art, doors, etc...
            OnTopOfSurfaces, // Spawn on surfaces facing upwards such as ground, top of tables, beds, couches, etc...
            HangingDown // Spawn on surfaces facing downwards such as the ceiling
        }


        /// <summary>
        /// Attach content to scene surfaces.
        /// </summary>
        [SerializeField, Tooltip("Attach content to scene surfaces.")]
        public SpawnLocation SpawnLocations = SpawnLocation.OnTopOfSurfaces;

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
        /// Bounds of the adjusted prefab bounds.
        /// </summary>
        private Bounds _adjustedBounds;
        private Bounds? _prefabBounds;
        private float _minRadius;
        private float _baseOffset;
        private float _centerOffset;


        /// <summary>
        /// List of game anchor surfaces
        /// </summary>
        private List<MRUKExtension.Surface> _gameAnchorSurfaces;

        private int _currentSpawnedObjects;
        public int CurrentSpawnedObjects => _currentSpawnedObjects;


        private void Start()
        {
            // NOTE: important part - calculates prefab bounds used here
            CalculatePrefabBounds();
            gameAnchorSelection.onSelectGameAnchor.AddListener(SetGameAnchor);
        }

        private void OnDestroy()
        {
            gameAnchorSelection.onSelectGameAnchor.RemoveListener(SetGameAnchor);
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void SpawnOnCurrentRoom()
        {
            if (MRUK.Instance && MRUK.Instance.IsInitialized)
            {
                var currentRoom = MRUK.Instance.GetCurrentRoom();
                if (_gameAnchor == null)
                {
                    Debug.LogError($"[{nameof(FindSpawnPositionsOnAnchor)}] - ERROR: No GameAnchor defined yet. Please set one before proceeding");
                    return;
                }

                SpawnObjectsInGameAnchor(currentRoom, _gameAnchor);
            }
            else
            {
                Debug.LogWarning($"[{nameof(FindSpawnPositionsOnAnchor)}] - MRUK not initialized yet");
            }
        }

        #region DEBUG
        /// <summary>
        /// In case we don't have GameAnchorSelection - we need to set one
        /// </summary>
#if ODIN_INSPECTOR
        [Button]
#endif        
        public void SetHardcodedGameAnchor()
        {
            SetGameAnchor(MRUKExtension.GetClosestAnchorBasedOnSurfacePosition(null, FindFirstObjectByType<Camera>().transform.position));
        }

        #endregion

        #region SETTERS
        public void SetGameAnchor(MRUKAnchor arg0)
        {
            _gameAnchor = arg0;
        }

        public void SetSpawnAmount(int newMax)
        {
            SpawnAmount = newMax;
        }


        public void SetNewPrefab(GameObject newPrefab)
        {
            SpawnObject = newPrefab;
            CalculatePrefabBounds();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="anchor"></param>
        /// <param name="isCalculatingMaxSpawnedObjects">Max amount of spawned objects is calculated based on not being able to place any other object in here. Important when running the first time</param>
        /// <returns></returns>
        private bool SpawnObjectsInGameAnchor(MRUKRoom room, MRUKAnchor anchor)
        {
            int i = _currentSpawnedObjects;
            while (i < SpawnAmount)
            {
                bool foundValidSpawnPosition = false;
                for (int j = 0; j < MaxIterations; ++j)
                {
                    Vector3 spawnPosition = Vector3.zero;
                    Vector3 spawnNormal = Vector3.zero;
                    if (SpawnLocations == SpawnLocation.Floating)
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
                            case SpawnLocation.AnySurface:
                                surfaceType |= MRUK.SurfaceType.FACING_UP;
                                surfaceType |= MRUK.SurfaceType.VERTICAL;
                                surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                                break;
                            case SpawnLocation.VerticalSurfaces:
                                surfaceType |= MRUK.SurfaceType.VERTICAL;
                                break;
                            case SpawnLocation.OnTopOfSurfaces:
                                surfaceType |= MRUK.SurfaceType.FACING_UP;
                                break;
                            case SpawnLocation.HangingDown:
                                surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                                break;
                        }

                        // NOTE: Code Change in here
                        //if (room.GenerateRandomPositionOnSurface(surfaceType, _minRadius, new LabelFilter(Labels), out var pos, out var normal))
                        if (GenerateRandomPositionOnSpecificSurfaceAnchor(surfaceType, _minRadius, anchor, out var pos, out var normal))
                        {
                            spawnPosition = pos + normal * _baseOffset;
                            spawnNormal = normal;
                            var center = spawnPosition + normal * _centerOffset;
                            // In some cases, surfaces may protrude through walls and end up outside the room
                            // check to make sure the center of the prefab will spawn inside the room
                            if (!room.IsPositionInRoom(center))
                            {
                                continue;
                            }

                            // Ensure the center of the prefab will not spawn inside a scene volume
                            if (room.IsPositionInSceneVolume(center))
                            {
                                continue;
                            }

                            // Also make sure there is nothing close to the surface that would obstruct it
                            if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
                            {
                                continue;
                            }
                        }
                    }

                    Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
                    if (CheckOverlaps && _prefabBounds.HasValue)
                    {
                        if (Physics.CheckBox(spawnPosition + spawnRotation * _adjustedBounds.center, _adjustedBounds.extents, spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                        {
                            continue;
                        }
                    }

                    foundValidSpawnPosition = true;

                    if (SpawnObject.gameObject.scene.path == null)
                    {
                        Instantiate(SpawnObject, spawnPosition, spawnRotation, transform);
                    }
                    else
                    {
                        SpawnObject.transform.position = spawnPosition;
                        SpawnObject.transform.rotation = spawnRotation;
                        return false; // ignore SpawnAmount once we have a successful move of existing object in the scene
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
        /// FROM FindSpawnPositions.cs - get prefab bounds and calculate adjusted bounds in order to reuse them later on.
        /// </summary>
        private void CalculatePrefabBounds()
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
        /// FROM MRUKRoom.cs - copy of GenerateRandomPositionOnSurface, but for a single Anchor
        /// </summary>
        /// <param name="surfaceTypes"></param>
        /// <param name="minDistanceToEdge"></param>
        /// <param name="anchor">Anchor to generate random position</param>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public bool GenerateRandomPositionOnSpecificSurfaceAnchor(MRUK.SurfaceType surfaceTypes, float minDistanceToEdge, MRUKAnchor anchor, out Vector3 position, out Vector3 normal)
        {
            float totalUsableSurfaceArea = 0f;
            // define these as the negative early exit conditions
            position = Vector3.zero;
            normal = Vector3.zero;

            if (_gameAnchorSurfaces == null)
            {
                //???: since it is always the same anchor, there is no point in recalculting every time (I think)
                _gameAnchorSurfaces = MRUKExtension.GetAnchorSurfaces(surfaceTypes, minDistanceToEdge, anchor, ref totalUsableSurfaceArea);
                if (_gameAnchorSurfaces.Count == 0)
                {
                    Debug.LogError($"[{nameof(FindSpawnPositionsOnAnchor)} - {nameof(_gameAnchorSurfaces)}]: Anchor {anchor.name} does not have surfaces!");
                }
                return false;
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

                position = surface.Transform.MultiplyPoint3x4(new Vector3(pos.x, pos.y, 0f));
                normal = surface.Transform.MultiplyVector(Vector3.forward);
                return true;
            }

            return false;
        }
    }
}
