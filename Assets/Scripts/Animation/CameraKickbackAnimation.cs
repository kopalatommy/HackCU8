using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    [System.Serializable]
    public class CameraKickbackAnimation
    {
        [SerializeField]
        private bool enabled;

        [SerializeField]
        private float kickback;

        [SerializeField]
        private float maxKickback;

        [SerializeField]
        private Vector2 kickbackRandomness;

        [SerializeField]
        private float horizontalKickback;

        [SerializeField]
        private Vector2 horizontalKickbackRandomness;

        [SerializeField]
        [Range(0, 1)]
        private float kickbackRotation;

        [SerializeField]
        private float kickbackDuration;

        [SerializeField]
        private float kickbackSpeed;

        public CameraKickbackAnimation(bool enabled, float kickback, float maxKickback, Vector2 kickbackRandomness, float horizontalKickback, Vector2 horizontalKickbackRandomness, float kickbackRotation, float kickbackDuration, float kickbackSpeed)
        {
            this.enabled = enabled;
            this.kickback = kickback;
            this.maxKickback = maxKickback;
            this.kickbackDuration = kickbackDuration;
            this.horizontalKickback = horizontalKickback;
            this.horizontalKickbackRandomness = horizontalKickbackRandomness;
            this.kickbackRandomness = kickbackRandomness;
            this.kickbackSpeed = kickbackSpeed;
            this.kickbackRotation = kickbackRotation;
        }

        public bool Enabled => enabled;

        public float Kickback => kickback;

        public float MaxKickback => maxKickback;

        public Vector2 HorizontalKickbackRandomness => horizontalKickbackRandomness;

        public float KickbackDuration => kickbackDuration;

        public float HorizontalKickback => horizontalKickback;

        public Vector2 KickbackRandomness => kickbackRandomness;

        public float KickbackRotation => kickbackRotation;

        public float KickbackSpeed => kickbackSpeed;
    }
}
