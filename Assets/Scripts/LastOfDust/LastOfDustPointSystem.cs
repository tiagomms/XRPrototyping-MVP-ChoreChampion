using UnityEngine;
using LastOfDust.UI;
using ChoreChampion.XR.MRUtilityKit;
using System.Xml.Serialization;
using Meta.XR.MRUtilityKit;
using UnityEngine.Events;
using System;
using UI;

namespace Chores
{
    public class LastOfDustPointSystem : MonoBehaviour
    {
        [Header("Points")]
        [SerializeField] private float monsterKillPoints = 1f;
        public float MonsterKillPoints => monsterKillPoints;
        [SerializeField] private float surfaceCleanPoints = 10f;
        public float SurfaceCleanPoints => surfaceCleanPoints;
        [SerializeField] private float anchorCleanPoints = 15f;
        public float AnchorCleanPoints => anchorCleanPoints;
        [SerializeField] private float roomCleanPoints = 30f;
        public float RoomCleanPoints => roomCleanPoints;
        
        [Header("Spawners")]
        [SerializeField] private RandomSpawnPrefabsOnAnchorSurfaces randomMonsterSpawner;

        public UnityEvent<float> onScore;

        private void OnEnable()
        {
            randomMonsterSpawner.onSpawnedObjectKilled.AddListener(OnMonsterKill);
            randomMonsterSpawner.onSurfaceCleaned.AddListener(OnSurfaceCleaned);
            randomMonsterSpawner.onAnchorCleaned.AddListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.AddListener(OnRoomCleaned);
        }

        private void OnDisable()
        {
            randomMonsterSpawner.onSpawnedObjectKilled.RemoveListener(OnMonsterKill);
            randomMonsterSpawner.onSurfaceCleaned.RemoveListener(OnSurfaceCleaned);
            randomMonsterSpawner.onAnchorCleaned.RemoveListener(OnAnchorCleaned);
            randomMonsterSpawner.onRoomCleaned.RemoveListener(OnRoomCleaned);
        }

        private void IncrementScore(float value)
        {
            LastOfDustChore.Instance.AddScore(value);
            onScore?.Invoke(value);
        }
        
        #region Events
        private void OnMonsterKill()
        {
            IncrementScore(monsterKillPoints);
        }


        private void OnSurfaceCleaned()
        {
            IncrementScore(surfaceCleanPoints);
        }

        private void OnAnchorCleaned()
        {
            IncrementScore(anchorCleanPoints);
        }

        private void OnRoomCleaned()
        {
            IncrementScore(roomCleanPoints);
            
        }

        #endregion


    }
}