using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.Util;
using Sirenix.OdinInspector;
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
    public class GameAnchorSelection : MonoBehaviour
    {
        [NonSerialized] public MRUKAnchor GameAnchor;
        /// <summary>
        /// Anchor prefab spawner will help on selecting the correct anchor where we start our game
        /// </summary>
        [Tooltip("Volume/Surface Anchor MRUK displayer")]
        [SerializeField] private AnchorPrefabSpawner _anchorPrefabSpawner;

        /// <summary>
        /// When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.
        /// ???: due to limitations of MRUK I will need to use this anyway - there is no MRUKRoom method to GetRandomPositionInAnchor
        /// </summary>
        [SerializeField, Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
        public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

        [Header("Debug")]
        [SerializeField, Tooltip("To help on setting a hardcoded Game Anchor based on player position.")]
        private Transform cameraTransform;

        public UnityEvent<MRUKAnchor> onSelectGameAnchor;
        // We want to initialize the spawner in the current room, but we don't want to spawn anything yet.
        // So we disable the anchor prefab spawner.
        private void Awake()
        {
            // TODO: this will not use anchor prefab spawner, instead we will have a ray that allow us to select what is the game area 
            // The anchor prefab spawner may still exist to help users know which volume/plane they want to clean
            // this works because MRUK.Instance.RegisterSceneLoadedCallback event triggers even if room has been initialized a long time ago
            _anchorPrefabSpawner.gameObject.SetActive(false);
        }


        [Button]
        public void SetDefaultGameAnchor()
        {
            // TODO: Right now I will set up the code from the closest anchor of type X to test, if not null
            SetHardcodedGameAnchor();

            onSelectGameAnchor.Invoke(GameAnchor);
        }

        /// <summary>
        /// Initialize the spawner in the current room.
        /// </summary>
        [Button]
        public void ToggleMrukAnchors()
        {
            // ???: unsure this part is needed - to select may be important
            if (MRUK.Instance && MRUK.Instance.IsInitialized)
            {
                bool toggleAnchorPrefabSpawner = _anchorPrefabSpawner.gameObject.activeSelf;
                toggleAnchorPrefabSpawner = !toggleAnchorPrefabSpawner;
                _anchorPrefabSpawner.gameObject.SetActive(toggleAnchorPrefabSpawner);
            }
        }

        private void SetHardcodedGameAnchor()
        {
            LabelFilter labelFilterBasedOnAnchorLabel = new(Labels, null);
            MRUK.Instance.GetCurrentRoom().TryGetClosestSurfacePosition(cameraTransform.position, out Vector3 surfacePosition, out GameAnchor, labelFilterBasedOnAnchorLabel);
        }

    }
}