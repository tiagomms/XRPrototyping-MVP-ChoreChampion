
using System;
using System.Linq;
using ChoreChampion.UI;
using ChoreChampion.XR.MRUtilityKit;
using Chores;
using DG.Tweening;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.AI;

namespace LastOfDust
{
    public class LodSlime : MonoBehaviour
    {
        public BaseWipingObject WipingObj { get; private set; }
        public NavMeshAgent Agent { get; private set; }

        private MRUKSpawnedObject mrukSpawnedObject;

        [SerializeField] private int maxRotationOffset = 45;
        [SerializeField] private AddScoreUIReferences addScoreObj;
        [SerializeField] private Vector3 _addScoreOriginalLocalScale;

        private void Awake()
        {
            WipingObj = GetComponent<BaseWipingObject>();
            Agent = GetComponent<NavMeshAgent>();
            Agent.enabled = false; // only enable on initialize
        }
        private void Start()
        {
            var random = new System.Random();
            // Randomly rotate on the Y axis by up to ±maxRotationOffset degrees from the current rotation
            float randomOffset = random.Next(-maxRotationOffset, maxRotationOffset);
            Quaternion yRotation = Quaternion.Euler(0f, randomOffset, 0f);
            transform.rotation = yRotation * transform.rotation;
            WipingObj.InitializeColliders(GetComponentsInChildren<Collider>());

            WipingObj.onDustParticleKilled.AddListener(AddScore);
            _addScoreOriginalLocalScale = addScoreObj.transform.localScale;
            addScoreObj.gameObject.SetActive(false);
        }

        private void AddScore()
        {
            //addScoreObj.transform.localScale = Vector3.zero;
            addScoreObj.gameObject.SetActive(true);
            addScoreObj.SetPoints(LastOfDustChore.Instance.PointSystem.MonsterKillPoints);
            //addScoreObj.transform.SetParent(null, true);

            //addScoreObj.transform.DOScale(_addScoreOriginalLocalScale, 0.2f).SetEase(Ease.InElastic).OnComplete(() => addScoreObj.transform.SetParent(null));
            
        }

        public void Initialize(MRUKSpawnedObject spawnedObject)
        {
            Agent.enabled = false; // right now disabled due to bugs with scene navigation
            mrukSpawnedObject = spawnedObject;
            //Debug.Log($"{nameof(LodSlime)} Initialized");
        }

        private void OnDestroy()
        {
            WipingObj.onDustParticleKilled.RemoveListener(AddScore);
            Destroy(addScoreObj.gameObject);
        }
    }
}