/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChoreChampion.XR.MRUtilityKit
{
    /// <summary>
    /// Allows for fast generation of valid random positions for content spawning on anchor surfaces.
    /// This class leverages the <see cref="MRUKRoom.GenerateRandomPositionInRoom"/> and <see cref="MRUKRoom.GenerateRandomPositionOnSurface"/> methods
    /// to provide a simple interface for spawning content in the room.
    /// </summary>
    public class RandomSpawnPrefabsOnAnchorSurfaces : BaseSpawnPrefabsOnAnchorSurfaces
    {
        /// <summary>
        /// Calculates spawn position and normal using random positioning on anchor surfaces.
        /// </summary>
        /// <param name="surfaceType">Type of surface to spawn on.</param>
        /// <param name="anchor">Anchor to spawn on.</param>
        /// <param name="spawnPosition">Output spawn position.</param>
        /// <param name="spawnNormal">Output spawn normal.</param>
        /// <param name="iteration">Current iteration number for incremental offset calculation. (ignored in this one)</param>
        /// <returns>True if position was calculated successfully, false otherwise.</returns>
        protected override bool CalculateSpawnPositionAndNormal(MRUK.SurfaceType surfaceType, MRUKAnchor anchor, out Vector3 spawnPosition, out Vector3 spawnNormal, int iteration = 0)
        {         
            spawnPosition = Vector3.zero;
            spawnNormal = Vector3.zero;

            if (GenerateRandomPositionOnSpecificSurfaceAnchor(surfaceType, _minRadius, anchor, out var pos, out var normal))
            {
                spawnPosition = pos + normal * _baseOffset;
                spawnNormal = normal;
                return true;
            }
            return false;
        }
    }
}
