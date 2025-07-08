using Chores;
using UnityEngine;
using UnityEngine.UI;
using UI;
using ChoreChampion.XR.MRUtilityKit;
using System;
using Oculus.Interaction;
using System.Collections.Generic;
using NaughtyAttributes;
using Meta.WitAi.CallbackHandlers;

namespace LastOfDust.UI
{
    public class Lod03SurfaceSelectionScreenUI : BaseUI
    {
        [Header("Anchor Buttons")]
        [SerializeField] private PlaceAndStretchSingleObjectOnAnchor tapAnchorMechanism;

        [Header("Next")]
        [SerializeField] private BaseUI nextUiScreen;

        private Dictionary<InteractableUnityEventWrapper, MRUKSpawnedObject> _spawnedUiInteractables = new();

        protected override void Awake()
        {
            base.Awake();
            tapAnchorMechanism.onSpawned.AddListener(InitializeSurfacePokeInteractables);
        }

        protected virtual void OnDestroy()
        {
            tapAnchorMechanism.onSpawned.RemoveListener(InitializeSurfacePokeInteractables);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Check if spawning has already occurred when this UI is enabled
            if (tapAnchorMechanism.HasSpawned())
            {
                InitializeSurfacePokeInteractables();
            }
        }

        private void InitializeSurfacePokeInteractables()
        {
            // create list of interactables
            int childCount = tapAnchorMechanism.GetSpawnedObjectCount();
            for (int i = 0; i < childCount; i++)
            {
                Transform obj = tapAnchorMechanism.transform.GetChild(i);

                MRUKSpawnedObject spawnedObject = obj.GetComponent<MRUKSpawnedObject>();
                InteractableUnityEventWrapper interactable = obj.GetComponentInChildren<InteractableUnityEventWrapper>();

                if (spawnedObject != null && interactable != null)
                {
                    _spawnedUiInteractables.Add(interactable, spawnedObject);
                }
            }

            EnableInteractables();
        }

        private void GameAnchorSelected(MRUKSpawnedObject value)
        {
            // ???: will there be a button to select or go straight to game. right now straight to game.
            DisableInteractables();
            LastOfDustChore.Instance.GameAreaSelected(value.Anchor);
            tapAnchorMechanism.ClearSpawnedObjects();
            // TODO: 3...2...1... Initialize game
            BaseUIManager.Instance.HideCurrentPanel();

            LastOfDustChore.Instance.StartChore(true);
        }

        private void EnableInteractables()
        {
            foreach (var item in _spawnedUiInteractables)
            {
                item.Key.WhenSelect.AddListener(() => GameAnchorSelected(item.Value));
            }
        }

        private void DisableInteractables()
        {
            foreach (var item in _spawnedUiInteractables)
            {
                item.Key.WhenSelect.RemoveListener(() => GameAnchorSelected(item.Value));
            }
        }

        public override void GoBack()
        {
            LastOfDustChore.Instance.Reset();
            base.GoBack();
        }

    }
}
