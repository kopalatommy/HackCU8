using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    [CreateAssetMenu(menuName = "Motion Data", fileName = "ScriptableObjects/Motion Data", order = 201)]
    public class MotionData : ScriptableObject
    {
        [SerializeField]
        private bool animatePosition = true;

        [SerializeField]
        [Tooltip("Determines how fast the animation will be played.")]
        private float positionSpeed = 1;

        [SerializeField]
        [Tooltip("Determines how far the transform can move horizontally.")]
        private float horizontalPositionAmplitude = 0.001f;

        [SerializeField]
        [Tooltip("Determines how far the transform can move vertically.")]
        private float verticalPositionAmplitude = 0.001f;

        [SerializeField]
        [Tooltip("Defines how the vertical movement will behave.")]
        private AnimationCurve verticalPositionAnimationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField]
        [Tooltip("Determines how notably the transform can be rotated on Z-axis.")]
        private float distalAmplitude;

        [SerializeField]
        private bool animateRotation = true;

        [SerializeField]
        [Tooltip("Determines how fast the animation will be played.")]
        private float rotationSpeed = 1;

        [SerializeField]
        [Tooltip("Determines how far the transform can move horizontally.")]
        private float horizontalRotationAmplitude = 1;

        [SerializeField]
        [Tooltip("Determines how far the transform can move vertically.")]
        private float verticalRotationAmplitude = 1;

        [SerializeField]
        [Tooltip("Defines how the vertical movement will behave.")]
        private AnimationCurve verticalRotationAnimationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [SerializeField]
        [Tooltip("Determines how notably the transform can be rotated on Z-axis.")]
        private float tiltAmplitude;

        [SerializeField, Range(-2, 2)]
        [Tooltip("Determines how distinctly the animation will be vertically affected by the direction of the character's movement.")]
        private float velocityInfluence = 1;

        [SerializeField]
        [Tooltip("Determines a position offset for the animation, moving it to a more appropriate position.")]
        private Vector3 positionOffset;

        [SerializeField]
        [Tooltip("Determines a rotation offset for the animation, rotating it to a more appropriate angle.")]
        private Vector3 rotationOffset;

        public bool AnimatePosition => animatePosition;

        public float PositionSpeed => positionSpeed;

        public float HorizontalPositionAmplitude => horizontalPositionAmplitude;

        public float VerticalPositionAmplitude => verticalPositionAmplitude;

        public AnimationCurve VerticalPositionAnimationCurve => verticalPositionAnimationCurve;

        public float DistalAmplitude => distalAmplitude;

        public bool AnimateRotation => animateRotation;

        public float RotationSpeed => rotationSpeed;

        public float HorizontalRotationAmplitude => horizontalRotationAmplitude;

        public float VerticalRotationAmplitude => verticalRotationAmplitude;

        public AnimationCurve VerticalRotationAnimationCurve => verticalRotationAnimationCurve;

        public float TiltAmplitude => tiltAmplitude;

        public float VelocityInfluence => velocityInfluence;

        public Vector3 PositionOffset => positionOffset;

        public Vector3 RotationOffset => rotationOffset;
    }
}
