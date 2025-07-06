using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    using BoundingBox = SentisInferenceUiManager.BoundingBox;
    /// <summary>
    /// Slightly different detection manager - it's responsibility is to place prefabs automatically
    /// </summary>
    public class DetectionPrefabManager : MonoBehaviour
    {

        [System.Serializable]
        public class PlacedPrefabData
        {
            public int ModelLabelId;
            public GameObject overlayObject;
            public Vector3 targetPosition;
            public int framesUnmatched;
            public int framesMatched;
        }

        public GameObject spawnPrefab;


        [Header("Prefab Placement Settings")]
        public float matchThreshold = 0.3f; // meters
        public float minMovementThreshold = 0.05f; // don't update if below this
        public float yOffset = 0.15f; // placing prefab y meters above
        public float lerpSpeed = 5f;
        public int maxFramesUnmatched = 3; // minimum consecutive frames before destroying overlay (if already showing)
        public int minFramesMatched = 1; // minimum consecutive frames before showing overlay - I am placing 1 because I think it does not work
        public float spawnScaleDuration = 0.3f; // spawn animation duration
        public float destroyScaleDuration = 0.2f;

        private List<PlacedPrefabData> activeOverlays = new();

        /// <summary>
        /// Because I couldn't keep track of things in the previous project, I tried to develop this fake way
        /// of keeping track things (if label of type n is in the proximities then it should be same)
        /// </summary>
        /// <param name="detectedBoxes"></param>
        public void UpdatePrefabs(List<BoundingBox> detectedBoxes)
        {
            // Step 1: Mark all overlays as unmatched
            foreach (var overlay in activeOverlays)
                overlay.framesUnmatched++;

            foreach (var box in detectedBoxes)
            {
                if (!box.WorldPos.HasValue)
                    continue;

                Vector3 worldPos = box.WorldPos.Value + Vector3.up * yOffset;

                // Step 2: Try to find closest matching overlay of same type
                PlacedPrefabData match = null;
                float bestDist = float.MaxValue;

                foreach (var overlay in activeOverlays)
                {
                    // idea here is: if it the model label id is different, then it is a piece of different clothes
                    if (overlay.ModelLabelId != box.Id) continue;

                    float dist = Vector3.Distance(overlay.overlayObject.transform.position, worldPos);
                    if (dist < matchThreshold && dist < bestDist)
                    {
                        bestDist = dist;
                        match = overlay;
                    }
                }

                if (match != null)
                {
                    // Only update if distance is meaningfully different
                    float delta = Vector3.Distance(match.targetPosition, worldPos);
                    if (delta > minMovementThreshold)
                    {
                        match.targetPosition = worldPos;
                    }
                    match.framesUnmatched = 0;
                    match.framesMatched++;

                    GameObject overlayObj = match.overlayObject;
                    // Only show overlay if minimum consecutive frames are reached
                    if (match.framesMatched >= minFramesMatched && !match.overlayObject.activeSelf)
                    {
                        ShowPrefabAndDoScaleUpAnimation(overlayObj);
                    }

                    // update label
                    UpdateLabel(overlayObj, box);
                    continue;
                }

                // Step 3: No match found — create a new overlay (but keep it hidden initially)
                GameObject obj = Instantiate(spawnPrefab, worldPos, Quaternion.identity);
                obj.transform.localScale = Vector3.zero;
                obj.SetActive(false); // Start hidden

                // NOTE: this is very dumb - but in case multiple frame matching does not work 
                //  then we have this protection.
                if (minFramesMatched == 1)
                {
                    ShowPrefabAndDoScaleUpAnimation(obj);
                }

                // update label if any
                UpdateLabel(obj, box);

                activeOverlays.Add(new PlacedPrefabData
                {
                    ModelLabelId = box.Id,
                    overlayObject = obj,
                    targetPosition = worldPos,
                    framesUnmatched = 0,
                    framesMatched = 1, // First frame detected
                });
            }

            // Step 4: Update and clean overlays
            for (int i = activeOverlays.Count - 1; i >= 0; i--)
            {
                var overlay = activeOverlays[i];

                // Smooth movement toward target position
                overlay.overlayObject.transform.position = Vector3.Lerp(
                    overlay.overlayObject.transform.position,
                    overlay.targetPosition,
                    Time.deltaTime * lerpSpeed
                );

                // Cleanup if unmatched too long
                if (overlay.framesUnmatched > maxFramesUnmatched)
                {
                    GameObject toDestroy = overlay.overlayObject;
                    activeOverlays.RemoveAt(i);

                    toDestroy.transform.DOScale(Vector3.zero, destroyScaleDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(toDestroy));
                }
            }
        }

        private void ShowPrefabAndDoScaleUpAnimation(GameObject overlayObj)
        {
            overlayObj.SetActive(true);
            overlayObj.transform.localScale = Vector3.zero;
            overlayObj.transform.DOScale(Vector3.one, spawnScaleDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Unsure you want this, at least for logging might be good
        /// </summary>
        /// <param name="overlayObject"></param>
        /// <param name="box"></param>
        private void UpdateLabel(GameObject overlayObject, BoundingBox box)
        {
            // TODO: improve this implementation it is terrible x)
            Text text = overlayObject.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = $"{box.UILabel}";
                return;
            }

            TMP_Text tmpText = overlayObject.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = $"{box.UILabel}";
                return;
            }
        }
    }
}
