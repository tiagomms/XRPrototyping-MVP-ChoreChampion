
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
        [SerializeField] private BaseWipingObject wipingObj;
        public BaseWipingObject WipingObj => wipingObj;

        
        public NavMeshAgent Agent { get; private set; }
        private GameObject _slimeObj;

        private void Awake()
        {
            _slimeObj = Utils.RandomChildActivator.ActivateRandomChild(transform);
            Agent = _slimeObj.GetComponent<NavMeshAgent>();
            Agent.enabled = false; // only enable on initialize
        }
        private void Start()
        {
            //wipingObj.InitializeColliders(_slimeObj.GetComponentsInChildren<Collider>());
        }

        public override void Initialize(MRUKAnchor anchor, MRUKExtension.Surface surface)
        {
            base.Initialize(anchor, surface);
            Agent.enabled = true;
        }
    }
}