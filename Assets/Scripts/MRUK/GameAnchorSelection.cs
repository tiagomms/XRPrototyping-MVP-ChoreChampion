using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.Util;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Meta.XR.MRUtilityKit
{
    /// <summary>
    /// Allows for fast generation of valid (inside the room, outside furniture bounds) random positions for content spawning.
    /// Optional method to pin directly to surfaces.
    /// This class leverages the <see cref="MRUKRoom.GenerateRandomPositionInRoom"/> and <see cref="MRUKRoom.GenerateRandomPositionOnSurface"/> methods
    /// to provide a simple interface for spawning content in the room.
    /// </summary>
    public class GameAnchorSelection : AnchorPrefabSpawner
    {
        [NonSerialized] public MRUKAnchor GameAnchor;
        /// <summary>
        /// Anchor prefab spawner will help on selecting the correct anchor where we start our game
        /// </summary>

        /// <summary>
        /// When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.
        /// ???: due to limitations of MRUK I will need to use this anyway - there is no MRUKRoom method to GetRandomPositionInAnchor
        /// </summary>
        [SerializeField, Tooltip("Eligible Game Anchor Labels. From these we may select our game anchor")]
        public MRUKAnchor.SceneLabels EligibleGameAnchorLabels = ~(MRUKAnchor.SceneLabels)0;

        [Header("Debug")]
        [SerializeField, Tooltip("To help on setting a hardcoded Game Anchor based on player position.")]
        private Transform cameraTransform;
        private bool _areAnchorsVisible = false;

        public UnityEvent<MRUKAnchor> onSelectGameAnchor;
        // We want to initialize the spawner in the current room, but we don't want to spawn anything yet.
        // So we disable the anchor prefab spawner.
        private void Awake()
        {
            // TODO: this will not use anchor prefab spawner, instead we will have a ray that allow us to select what is the game area 
            // The anchor prefab spawner may still exist to help users know which volume/plane they want to clean
            // this works because MRUK.Instance.RegisterSceneLoadedCallback event triggers even if room has been initialized a long time ago
            
            // so it makes sure it does not spawn prefabs on start
            this.SpawnOnStart = MRUK.RoomFilter.None;
        }

        [Button]
        public void SetDefaultGameAnchor()
        {
            // TODO: Right now I will set up the code from the closest anchor of type X to test, if not null
            GameAnchor = GetClosestAnchorBasedOnSurfacePosition(EligibleGameAnchorLabels, cameraTransform.position);

            onSelectGameAnchor.Invoke(GameAnchor);
        }

        /// <summary>
        /// Toggle MRUK Anchors to make them visible - may be important for debugging
        /// </summary>
        [Button]
        public void ToggleMrukAnchors()
        {
            // ???: unsure this part is needed - to select may be important
            if (MRUK.Instance && MRUK.Instance.IsInitialized)
            {
                _areAnchorsVisible = !_areAnchorsVisible;

                if (_areAnchorsVisible)
                {
                    SpawnPrefabs(MRUK.Instance.GetCurrentRoom());
                }
                else 
                {
                    ClearPrefabs();
                }
            }
        }

        public static MRUKAnchor GetClosestAnchorBasedOnSurfacePosition(MRUKAnchor.SceneLabels? labels, Vector3 worldPos)
        {
            LabelFilter labelFilterBasedOnAnchorLabel = new(labels, null);
            MRUK.Instance.GetCurrentRoom().TryGetClosestSurfacePosition(worldPos, out Vector3 surfacePosition, out MRUKAnchor anchor, labelFilterBasedOnAnchorLabel);
            return anchor;
        }

    }
}