using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    [System.Serializable]
    public class LerpAnimation
    {
        [SerializeField]
        [Tooltip("Defines the animation’s target (destination) position.")]
        private Vector3 targetPosition;

        [SerializeField]
        [Tooltip("Defines the animation’s target rotation.")]
        private Vector3 targetRotation;

        [SerializeField]
        [Tooltip("Defines how long the animation will take to move from the origin to the destination.")]
        private float duration;

        [SerializeField]
        [Tooltip("Defines how long the animation will take to retreat from the destination to the original position.")]
        private float returnDuration;

        public Vector3 Position
        {
            get;
            set;
        }

        public Vector3 Rotation
        {
            get;
            set;
        }

        public LerpAnimation(Vector3 targetPosition, Vector3 targetRotation, float duration = 0.25f, float returnDuration = 0.25f)
        {
            this.targetPosition = targetPosition;
            this.targetRotation = targetRotation;
            this.duration = duration;
            this.returnDuration = returnDuration;
        }

        public void SetTargets(Vector3 targetPosition, Vector3 targetRotation)
        {
            this.targetPosition = targetPosition;
            this.targetRotation = targetRotation;
        }

        public IEnumerator Play()
        {
            // Make the GameObject move to target slightly.
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                float by = t / duration;
                Position = Vector3.Lerp(Vector3.zero, targetPosition, by);
                Rotation = Vector3.Lerp(Vector3.zero, targetRotation, by);
                yield return null;
            }

            // Make it move back to its origin.
            for (float t = 0; t <= returnDuration; t += Time.deltaTime)
            {
                float by = t / returnDuration;
                Position = Vector3.Lerp(targetPosition, Vector3.zero, by);
                Rotation = Vector3.Lerp(targetRotation, Vector3.zero, by);
                yield return null;
            }

            Position = Vector3.zero;
            Rotation = Vector3.zero;
        }

        public IEnumerator Stop()
        {
            // Make it move back to its origin.
            for (float t = 0; t <= returnDuration; t += Time.deltaTime)
            {
                float by = t / returnDuration;
                Position = Vector3.Lerp(targetPosition, Vector3.zero, by);
                Rotation = Vector3.Lerp(targetRotation, Vector3.zero, by);
                yield return null;
            }

            Position = Vector3.zero;
            Rotation = Vector3.zero;
        }
    }
}
