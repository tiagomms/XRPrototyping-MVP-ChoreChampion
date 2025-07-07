using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

namespace ChoreChampion.XR.MRUtilityKit
{
    /// <summary>
    /// Places spawn objects at fixed positions on anchor surfaces with optional parenting.
    /// </summary>
    public class FixedSpawnPrefabsOnAnchorSurfaces : BaseSpawnPrefabsOnAnchorSurfaces
    {
        protected enum SpawnBasedOn
        {
            UserPosition = 0,
            AnchorTransform = 1
        }

        [Space]
        [Header("Fixed Spawn Variables")]
        [SerializeField, Tooltip("Spawn things based on user position or Anchor transform")]
        protected SpawnBasedOn spawnBasedOn = SpawnBasedOn.UserPosition;

        [SerializeField, Tooltip("On spawn based on user position, where it should spawn")]
        protected MRUKExtension.SnapTarget snapTarget = MRUKExtension.SnapTarget.NearestEdge;

        [SerializeField, Tooltip("Based on local position provided, clamp value in axis if needed")]
        protected MRUKExtension.Clamp2DValues clampLocalPositions = MRUKExtension.Clamp2DValues.X;

        /// <summary>
        /// List of fixed local positions to spawn prefabs at. Cycles through this list when spawn amount exceeds list size.
        /// </summary>
        [SerializeField, Tooltip("List of fixed local positions to spawn prefabs at. Cycles through this list when spawn amount exceeds list size.")]
        protected List<Vector2> fixedLocalPositions = new List<Vector2>();

        /// <summary>
        /// If true, shuffles the position list each cycle; if false, uses sequential order.
        /// </summary>
        [SerializeField, Tooltip("If true, shuffles the position list each cycle; if false, uses sequential order.")]
        protected bool shufflePositions = false;

        /// <summary>
        /// Current index in the fixed positions list.
        /// </summary>
        protected int currentPositionIndex = 0;

        /// <summary>
        /// Private random instance for shuffling positions.
        /// </summary>
        protected System.Random random;

        /// <summary>
        /// Incremental offset adjustment per iteration on the normal direction.
        /// </summary>
        [SerializeField, Tooltip("Incremental offset adjustment per iteration on the normal direction.")]
        protected float incrementalOffsetPerIteration = 0.002f;

        protected override void OnValidate()
        {
            base.OnValidate();
            MaxIterations = 10; // since positions are fixed it does not make sense to iterate further
        }

        /// <summary>
        /// Unity Start: Initialize the random instance.
        /// </summary>
        protected override void Start()
        {
            random = new System.Random();
            base.Start();
        }

        /// <summary>
        /// Calculates spawn position and normal using fixed positioning from the list.
        /// </summary>
        /// <param name="surfaceType">Type of surface to spawn on.</param>
        /// <param name="anchor">Anchor to spawn on.</param>
        /// <param name="spawnPosition">Output spawn position.</param>
        /// <param name="spawnNormal">Output spawn normal.</param>
        /// <param name="iteration">Current iteration number for incremental offset calculation.</param>
        /// <returns>True if position was calculated successfully, false otherwise.</returns>
        protected override bool CalculateSpawnPositionAndNormal(MRUK.SurfaceType surfaceType, MRUKAnchor anchor, out Vector3 spawnPosition, out Vector3 spawnNormal, int iteration = 0)
        {
            spawnPosition = Vector3.zero;
            spawnNormal = Vector3.zero;

            if (fixedLocalPositions.Count == 0)
            {
                return false;
            }

            // Get the current position from the list
            Vector2 localPosition = fixedLocalPositions[currentPositionIndex];

            // Move to next position
            currentPositionIndex = (currentPositionIndex + 1) % fixedLocalPositions.Count;

            // Shuffle if needed and we've completed a cycle
            if (shufflePositions && currentPositionIndex == 0)
            {
                ShufflePositions();
            }

            // 
            if (TryGeneratePositionOnSpecificSurfaceAnchor(surfaceType, anchor, out Vector3 pos, out Vector3 normal, iteration, localPosition))
            {
                // Calculate incremental offset based on iteration
                float incrementalOffset = iteration * incrementalOffsetPerIteration;
                spawnPosition = pos + normal * (_baseOffset + incrementalOffset);
                spawnNormal = normal;
                return true;
            }

            return false;
        }

        // In my extensions this will be the bit that changes
        protected virtual bool TryGeneratePositionOnSpecificSurfaceAnchor(MRUK.SurfaceType surfaceType, MRUKAnchor anchor, out Vector3 pos, out Vector3 normal, int iteration, Vector2 localPosition)
        {
            if (spawnBasedOn == SpawnBasedOn.AnchorTransform)
            {
                // This case: Use fixed positioning based on anchor local space
                return GenerateFixedPositionOnSpecificSurfaceAnchor(surfaceType, localPosition, _minRadius, anchor, out pos, out normal);
            }
            else
            {
                return GenerateClosestPositionOnSpecificSurfaceAnchor(surfaceType, localPosition, _minRadius, anchor, out pos, out normal, snapTarget, clampLocalPositions);
            }

        }

        /// <summary>
        /// Shuffles the fixed positions list using Fisher-Yates algorithm.
        /// </summary>
        protected void ShufflePositions()
        {
            for (int i = fixedLocalPositions.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(0, i + 1);
                Vector2 temp = fixedLocalPositions[i];
                fixedLocalPositions[i] = fixedLocalPositions[randomIndex];
                fixedLocalPositions[randomIndex] = temp;
            }
        }
    }
}