using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace ChoreChampion.XR.MRUtilityKit
{
    /// <summary>
    /// Extended Anchor prefab spawner - with Game Anchor selection and Toggle MRUK Anchor visibility
    /// </summary>
    public class ExtendedAnchorPrefabSpawner : AnchorPrefabSpawner
    {

        [SerializeField, Tooltip("Start with Anchor Prefabs Visible")]
        private bool areAnchorsVisibleOnStart;

        [Header("Debug")]

        [SerializeField, Tooltip("For Hardcoded Game Anchor - set closest anchor from one of these labels")]
        public MRUKAnchor.SceneLabels EligibleGameAnchorLabels = ~(MRUKAnchor.SceneLabels)0;

        [SerializeField, Tooltip("For Hardcoded Game Anchor - provide player position to track closest anchor.")]
        private Transform cameraTransform;


        private bool _areAnchorsVisible = false;

        public UnityEvent onCompleteSpawnPrefabs;

        // NOTE: to deprecate whenever possible
        public UnityEvent<MRUKAnchor> onSelectGameAnchor;
        [NonSerialized] public MRUKAnchor GameAnchor;

        protected override void Start()
        {
            if (MRUK.Instance is null)
            {
                return;
            }
            _areAnchorsVisible = areAnchorsVisibleOnStart;

            MRUK.Instance.RegisterSceneLoadedCallback(() =>
            {
                if (SpawnOnStart == MRUK.RoomFilter.None)
                {
                    return;
                }

                switch (SpawnOnStart)
                {
                    case MRUK.RoomFilter.CurrentRoomOnly:
                        SpawnPrefabs(MRUK.Instance.GetCurrentRoom());
                        ToggleMrukAnchorsVisibility();
                        onCompleteSpawnPrefabs.Invoke();
                        break;
                    case MRUK.RoomFilter.AllRooms:
                        SpawnPrefabs();
                        ToggleMrukAnchorsVisibility();
                        onCompleteSpawnPrefabs.Invoke();
                        break;
                    case MRUK.RoomFilter.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        
        [Button]
        public void SetDefaultGameAnchor()
        {
            // TODO: Right now I will set up the code from the closest anchor of type X to test, if not null
            SetGameAnchor(MRUKExtension.GetClosestAnchorBasedOnSurfacePosition(EligibleGameAnchorLabels, cameraTransform.position));
        }

        /// <summary>
        /// Deprecate whenever possible
        /// </summary>
        /// <param name="newGameAnchor"></param>
        public void SetGameAnchor(MRUKAnchor newGameAnchor)
        {
            GameAnchor = newGameAnchor;
            onSelectGameAnchor.Invoke(GameAnchor);
        }

        /// <summary>
        /// Toggle MRUK Anchors to make them visible - may be important for debugging
        /// </summary>
        [Button]
        public void ToggleMrukAnchorsVisibility()
        {
            SetMrukAnchorsVisibility(_areAnchorsVisible);
            _areAnchorsVisible = !_areAnchorsVisible; // toggle value

        }

        public void SetMrukAnchorsVisibility(bool isVisible)
        {
            foreach (var item in AnchorPrefabSpawnerObjects)
            {
                item.Value.GetComponentInChildren<MeshRenderer>().enabled = isVisible;
            }
        }


    }
}