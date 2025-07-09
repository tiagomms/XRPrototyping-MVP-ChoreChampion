
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

        private void Awake()
        {
            WipingObj = GetComponent<BaseWipingObject>();
            Agent = GetComponent<NavMeshAgent>();
            Agent.enabled = false; // only enable on initialize
        }
        private void Start()
        {
            WipingObj.InitializeColliders(GetComponentsInChildren<Collider>());
        }

        public override void Initialize(MRUKAnchor anchor, MRUKExtension.Surface surface)
        {
            base.Initialize(anchor, surface);
            Agent.enabled = true;
        }
    }
}