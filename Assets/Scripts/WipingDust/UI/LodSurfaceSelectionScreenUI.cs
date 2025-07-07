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
    public class LodSurfaceSelectionScreenUI : BaseUI
    {
        [Header("Anchor Buttons")]
        [SerializeField] private PlaceAndStretchSingleObjectOnAnchor tapAnchorMechanism;

        [Header("Next")]
        [SerializeField] private BaseUI nextUiScreen;

        private Dictionary<InteractableUnityEventWrapper, MRUKSpawnedObject> spawnedUiInteractables = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (tapAnchorMechanism != null)
            {
                tapAnchorMechanism.onSpawned.AddListener(InitializeSurfacePokeInteractables);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (tapAnchorMechanism != null)
            {
                tapAnchorMechanism.onSpawned.RemoveListener(InitializeSurfacePokeInteractables);
            }
        }

        private void InitializeSurfacePokeInteractables()
        {
            int childCount = tapAnchorMechanism.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform obj = tapAnchorMechanism.transform.GetChild(i);

                MRUKSpawnedObject spawnedObject = obj.GetComponent<MRUKSpawnedObject>();
                InteractableUnityEventWrapper interactable = obj.GetComponentInChildren<InteractableUnityEventWrapper>();

                if (spawnedObject != null && interactable != null)
                {
                    spawnedUiInteractables.Add(interactable, spawnedObject);
                }
            }

            EnableInteractables();
        }

        private void GameAnchorSelected(MRUKSpawnedObject value)
        {
            // ???: will there be a button to select or go straight to game. right now straight to game.
            DisableInteractables();
            LastOfDustChore.Instance.GameAreaSelected(value.Anchor);
        }

        private void EnableInteractables()
        {
            foreach (var item in spawnedUiInteractables)
            {
                item.Key.WhenSelect.AddListener(() => GameAnchorSelected(item.Value));
            }
        }

        private void DisableInteractables()
        {
            foreach (var item in spawnedUiInteractables)
            {
                item.Key.WhenSelect.RemoveListener(() => GameAnchorSelected(item.Value));
            }
        }



    }
}
