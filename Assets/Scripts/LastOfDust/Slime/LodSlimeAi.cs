using System;
using ChoreChampion.XR.MRUtilityKit;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace LastOfDust
{
    public class LodSlimeAi : KawaiiSlimeAi
    {
        private BaseWipingObject _wipingObj;
        private LodSlime _lodSlime;
        [SerializeField] private MRUKSpawnedObject _mrukSpawnedObject;
        private bool _hasBeenKilled;

        /// <summary>
        /// Minimum Y scale when flattened.
        /// </summary>
        [SerializeField] private float MinYScale = 0.1f;
        /// <summary>
        /// Maximum XZ scale when spread.
        /// </summary>
        [SerializeField] private float MaxXZScale = 2.0f;
        /// <summary>
        /// Default scale (100%).
        /// </summary>
        private Vector3 defaultScale;
        /// <summary>
        /// Duration for scale animations.
        /// </summary>
        [SerializeField]
        private float scaleAnimDuration = 0.5f;

        public UnityEvent onDeadAnimationEnded;

        // NOTE: lazy approach - please do doublecheck
        /*
        void OnValidate()
        {
            if (SmileBody == null)
            {
                SmileBody = transform.GetChild(1).gameObject;
            }
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }
        }
        */

        /// <summary>
        /// Unity Awake: Store default scale and subscribe to events.
        /// </summary>
        protected void OnEnable()
        {
            _wipingObj = GetComponent<BaseWipingObject>();
            _lodSlime = GetComponent<LodSlime>();
            _mrukSpawnedObject = GetComponentInParent<MRUKSpawnedObject>();

            defaultScale = transform.localScale;
        }

        protected override void Start()
        {
            base.Start();
            // no need to remove listeners because I am deleting the BaseDustParticle
            _wipingObj.onDustParticleHit.AddListener(AnimateHitScale);
            _wipingObj.onDustParticleKilled.AddListener(AnimateKillAndDestroy);
        }

        /// <summary>
        /// Animates the scale of the cube based on remaining life points.
        /// </summary>
        private void AnimateHitScale()
        {
            float t = GetLifePercentage();
            Vector3 targetScale = CalculateNewScale(t);

            Damaged(GetDamageType(t));

            transform.DOScale(targetScale, scaleAnimDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Life percentage</param>
        /// <returns></returns>
        private Vector3 CalculateNewScale(float t)
        {
            // Interpolate Y from 1.0 to MinYScale, XZ from 1.0 to MaxXZScale
            float yScale = Mathf.Lerp(1.0f, MinYScale, t);
            float xzScale = Mathf.Lerp(1.0f, MaxXZScale, t);
            Vector3 targetScale = new Vector3(defaultScale.x * xzScale, defaultScale.y * yScale, defaultScale.z * xzScale);
            return targetScale;
        }

        /// <summary>
        /// Based on life percentage, set damage type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private int GetDamageType(float t)
        {
            if (t >= 0.5f)
            {
                return 0;
            }
            else if (t > 0f)
            {
                return 1;
            }
            return 2;
        }

        private float GetLifePercentage()
        {
            // 1 is subtracted to both because because on the last hit the thing is killed
            int maxLife = _wipingObj.MaxLifePoints - 1;
            int currentLife = _wipingObj.LifePoints - 1;
            float t = 1.0f - (float)currentLife / maxLife;
            return t;
        }

        /// <summary>
        /// Animates the cube shrinking to zero and destroys the GameObject.
        /// </summary>
        private void AnimateKillAndDestroy()
        {
            _hasBeenKilled = true;
            Damaged(GetDamageType(0f));
        }

        protected override void OnAnimationDamageEnded()
        {
            // When Animation ended check distance between current position and first position 
            //if it > 1 AI will back to first position 

            // if died - or dmg type 2
            if (_hasBeenKilled) // wipingObj.LifePoints == 0
            {
                //Debug.Log($"Animation Damage Ended - KILLED - mrukSpawnedObject exists: {_mrukSpawnedObject != null}");
                if (_mrukSpawnedObject != null)
                {
                    _mrukSpawnedObject.Delete();
                }
                onDeadAnimationEnded.Invoke();

                animator.speed = 0f; // stop animator let it die
                return;
            }

            base.OnAnimationDamageEnded();
        }


        private void OnDestroy()
        {
            onDeadAnimationEnded.RemoveAllListeners();
        }

        // FIXME: initialize does not work on multiple spawn and random things - it just initialzes on the wrong one
        public void Initialize(MRUKSpawnedObject spawnedObject)
        {
            _mrukSpawnedObject = spawnedObject;
            //Debug.Log($"{nameof(LodSlimeAi)} Initialized");
        }
    }
}