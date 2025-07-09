
using System.Linq;
using ChoreChampion.XR.MRUtilityKit;
using Chores;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.AI;

namespace LastOfDust
{
    public class LodSlime : MRUKSpawnedObject
    {
        public BaseWipingObject WipingObj {get; private set;}
        public NavMeshAgent Agent { get; private set; }

        [SerializeField] private float maxRotationOffset = 180f;

        private void Awake()
        {
            WipingObj = GetComponent<BaseWipingObject>();
            Agent = GetComponent<NavMeshAgent>();
            Agent.enabled = false; // only enable on initialize
        }
        private void Start()
        {
            // Randomly rotate on the Y axis by up to ±maxRotationOffset degrees from the current rotation
            float randomOffset = Random.Range(-maxRotationOffset, maxRotationOffset);
            Quaternion yRotation = Quaternion.Euler(0f, randomOffset, 0f);
            transform.rotation = yRotation * transform.rotation;

            WipingObj.InitializeColliders(GetComponentsInChildren<Collider>());
        }

        public override void Initialize(MRUKAnchor anchor, MRUKExtension.Surface surface)
        {
            base.Initialize(anchor, surface);
            Agent.enabled = true;
        }
    }
}