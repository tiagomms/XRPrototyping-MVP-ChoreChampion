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

using NaughtyAttributes;
using UnityEngine;

namespace Utils
{
    public class SimpleBillboard : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("If true, the object will rotate to face the camera immediately on Start.")]
        protected bool toStartRotated = false;

        [SerializeField]
        [Tooltip("If false, disables the Update rotation behavior.")]
        public bool DoUpdate = true;

        [ShowIf("DoUpdate"), Header("Rotation Settings")]
        [SerializeField, Range(0f, 1f)] protected float rotationSpeed = 0.1f;

        protected Camera _mainCamera;
        protected Quaternion _targetRotation;

        protected virtual void Start()
        {
            _mainCamera = Camera.main;
            _targetRotation = transform.rotation;

            // If toStartRotated is true, rotate immediately to face the camera
            if (toStartRotated)
            {
                Vector3 direction = GetDirection();
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        protected virtual void Update()
        {
            // If DoUpdate is false, skip Update logic
            if (!DoUpdate)
            {
                return;
            }

            Vector3 direction = GetDirection();
            // If the direction is too small, don't rotate (avoids errors)
            if (direction.sqrMagnitude < 0.001f)
                return;

            LookTowards(direction.normalized);
        }

        protected virtual Vector3 GetDirection()
        {
            return transform.position - _mainCamera.transform.position;
        }

        protected void LookTowards(Vector3 direction)
        {
            _targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed);
        }
    }
}
