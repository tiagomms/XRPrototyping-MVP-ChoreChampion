using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using NaughtyAttributes;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System;

namespace ChoreChampion.XR.MRUtilityKit
{
    public class MRUKSpawnedObject : MonoBehaviour
    {
        protected string _id = Guid.NewGuid().ToString();
        public string ID => _id;
        public MRUKAnchor Anchor { get; private set; }
        public MRUKExtension.Surface Surface { get; private set; }

        public UnityEvent<MRUKSpawnedObject> onInitialized = new();
        public UnityEvent<MRUKSpawnedObject> onDestroyed = new();

        public virtual void Initialize(MRUKAnchor anchor, MRUKExtension.Surface surface)
        {
            Anchor = anchor;
            Surface = surface;
            onInitialized?.Invoke(this);
        }

        /// <summary>
        /// Deletes the object without triggering the onDestroyed event.
        /// Useful for batch operations to avoid collection modification errors.
        /// </summary>
        /// <param name="obj">Object to destroy, or null to destroy this GameObject.</param>
        /// <param name="triggerEvent">Whether to trigger the onDestroyed event.</param>
        public virtual void Delete(GameObject obj = null, bool triggerEvent = true)
        {
            if (triggerEvent)
            {
                onDestroyed?.Invoke(this);
            }
            Destroy(obj != null ? obj : gameObject);
        }

        protected virtual void OnDestroy()
        {
            onInitialized.RemoveAllListeners();
            onDestroyed.RemoveAllListeners();
        }


        public bool Equals(MRUKSpawnedObject other)
        {
            return ID == other.ID; // reference equality, may be fragile
        }

        public override bool Equals(object obj)
        {
            return obj is MRUKSpawnedObject other && Equals(other);
        }
        public override int GetHashCode()
        {
            return ID != null ? ID.GetHashCode() : 0;
        }
    }
}