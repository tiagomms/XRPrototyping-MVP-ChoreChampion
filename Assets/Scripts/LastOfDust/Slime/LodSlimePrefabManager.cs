using ChoreChampion.XR.MRUtilityKit;
using DG.Tweening;
using LastOfDust;
using Meta.XR.MRUtilityKit;
using NaughtyAttributes;
using UnityEngine;
using Utils;

namespace LastOfDust
{
    public class LodSlimePrefabManager : MRUKSpawnedObject
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public LodSlime CurrentSlime;
        public LodSlimeAi CurrentSlimeAi;
        //public BaseWipingObject WipingObj;
        [SerializeField] private float prefabDeathAnimDuration = 0.5f;
        public override void Initialize(MRUKAnchor anchor, MRUKExtension.Surface surface)
        {
            GameObject selectedChildObj = RandomChildActivator.ActivateRandomChild(transform);
            CurrentSlime = selectedChildObj.GetComponent<LodSlime>();
            CurrentSlimeAi = selectedChildObj.GetComponent<LodSlimeAi>();

            CurrentSlime.Initialize(this);
            CurrentSlimeAi.Initialize(this);

            base.Initialize(anchor, surface);
        }

        [Button]
        public override void Delete(GameObject obj = null, bool triggerEvent = true)
        {
            //Debug.Log($"LodSlimePrefabManager DELETE Zoom out");
            transform.DOScale(Vector3.zero, prefabDeathAnimDuration)
                                    .SetEase(Ease.InBack)
                                    .OnComplete(() => base.Delete(obj, triggerEvent));
        }
    }
}