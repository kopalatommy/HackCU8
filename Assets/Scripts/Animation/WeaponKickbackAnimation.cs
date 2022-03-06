using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    [System.Serializable]
    public class WeaponKickbackAnimation
    {
        [SerializeField]
        private bool enabled;

        [SerializeField]
        private float upwardForce;

        [SerializeField]
        private Vector2 upwardRandomness;

        [SerializeField]
        private float sidewaysForce;

        [SerializeField]
        private Vector2 sidewaysRandomness;

        [SerializeField]
        private float kickbackForce;

        [SerializeField]
        private Vector2 kickbackRandomness;

        [SerializeField]
        private float verticalRotation;

        [SerializeField]
        private Vector2 verticalRotationRandomness;

        [SerializeField]
        private float horizontalRotation;

        [SerializeField]
        private Vector2 horizontalRotationRandomness;

        [SerializeField]
        private float kickbackDuration;

        [SerializeField]
        private float kickbackSpeed;

        public WeaponKickbackAnimation(bool enabled, float upwardForce, Vector2 upwardRandomness, float sidewaysForce, Vector2 sidewaysRandomness, float kickbackForce, Vector2 kickbackRandomness, float verticalRotation, Vector2 verticalRotationRandomness, float horizontalRotation, Vector2 horizontalRotationRandomness, float kickbackDuration, float kickbackSpeed)
        {
            this.enabled = enabled;
            this.upwardForce = upwardForce;
            this.upwardRandomness = upwardRandomness;
            this.sidewaysForce = sidewaysForce;
            this.sidewaysRandomness = sidewaysRandomness;
            this.kickbackForce = kickbackForce;
            this.kickbackRandomness = kickbackRandomness;
            this.verticalRotation = verticalRotation;
            this.verticalRotationRandomness = verticalRotationRandomness;
            this.horizontalRotation = horizontalRotation;
            this.horizontalRotationRandomness = horizontalRotationRandomness;
            this.kickbackDuration = kickbackDuration;
            this.kickbackSpeed = kickbackSpeed;
        }

        public bool Enabled => enabled;

        public float UpwardForce => upwardForce;

        public Vector2 UpwardRandomness => upwardRandomness;

        public float SidewaysForce => sidewaysForce;

        public Vector2 SidewaysRandomness => sidewaysRandomness;

        public float KickbackForce => kickbackForce;

        public Vector2 KickbackRandomness => kickbackRandomness;

        public float VerticalRotation => verticalRotation;

        public Vector2 VerticalRotationRandomness => verticalRotationRandomness;

        public float HorizontalRotation => horizontalRotation;

        public Vector2 HorizontalRotationRandomness => horizontalRotationRandomness;

        public float KickbackDuration => kickbackDuration;

        public float KickbackSpeed => kickbackSpeed;
    }
}
