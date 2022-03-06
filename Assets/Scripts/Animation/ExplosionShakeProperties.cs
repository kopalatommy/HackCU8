using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    [System.Serializable]
    public class ExplosionShakeProperties
    {
        [SerializeField]
        [Tooltip("Determines the initial direction of the shake caused by the explosion.")]
        private float angle;

        [SerializeField]
        [Tooltip("How far the shake can move the camera.")]
        private float strength;

        [SerializeField]
        [Tooltip("The minimum speed of how fast the camera can move during the shake.")]
        private float minSpeed;

        [SerializeField]
        [Tooltip("The maximum speed of how fast the camera can move during the shake.")]
        private float maxSpeed;

        [SerializeField]
        [Tooltip("How long the explosion 'shake' lasts.")]
        private float duration;

        [SerializeField, Range(0, 1)]
        [Tooltip("A higher value will result in a more randomized direction for the shake animation.")]
        private float noisePercent;

        [SerializeField, Range(0, 1)]
        [Tooltip("Sets how fast the shake intensity will decrease.")]
        private float dampingPercent;

        [SerializeField, Range(0, 1)]
        [Tooltip("The magnitude at which camera rotation is affected during the shake animation.")]
        private float rotationPercent;

        public float Angle => angle;

        public float Strength => strength;

        public float MinSpeed => minSpeed;

        public float MaxSpeed => maxSpeed;

        public float Duration => duration;

        public float NoisePercent => noisePercent;

        public float DampingPercent => dampingPercent;

        public float RotationPercent => rotationPercent;

        public ExplosionShakeProperties(float angle, float strength, float minSpeed, float maxSpeed, float duration, float noisePercent, float dampingPercent, float rotationPercent)
        {
            this.angle = angle;
            this.strength = strength;
            this.maxSpeed = minSpeed;
            this.minSpeed = maxSpeed;
            this.duration = duration;
            this.noisePercent = Mathf.Clamp01(noisePercent);
            this.dampingPercent = Mathf.Clamp01(dampingPercent);
            this.rotationPercent = Mathf.Clamp01(rotationPercent);
        }
    }
}
