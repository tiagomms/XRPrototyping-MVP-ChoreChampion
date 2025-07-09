
using System.Linq;
using ChoreChampion.XR.MRUtilityKit;
using Chores;
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
        }

        public void Initialize(MRUKSpawnedObject spawnedObject)
        {
            Agent.enabled = false; // right now disabled due to bugs with scene navigation
            mrukSpawnedObject = spawnedObject;
            //Debug.Log($"{nameof(LodSlime)} Initialized");
        }
    }
}