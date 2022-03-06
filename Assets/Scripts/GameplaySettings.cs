using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameplaySettings", order = 1)]
    public class GameplaySettings : ScriptableObject
    {
        [SerializeField]
        [Range(0.1f, 10)]
        [Tooltip("Defines the overall mouse sensitivity.")]
        private float overallMouseSensitivity = 1;

        [SerializeField]
        [Tooltip("Defines whether the camera’s horizontal movement must be opposite to the mouse movement.")]
        private bool invertHorizontalAxis;

        [SerializeField]
        [Tooltip("Defines whether the camera’s vertical movement must be opposite to the mouse movement.")]
        private bool invertVerticalAxis;

        [SerializeField]
        [Range(50, 90)]
        [Tooltip("Defines the camera FOV in hip-fire mode.")]
        private float fieldOfView = 75;

        public float OverallMouseSensitivity
        {
            get => overallMouseSensitivity;
            set => overallMouseSensitivity = Mathf.Clamp(value, 0.1f, 10);
        }

        public bool InvertHorizontalAxis
        {
            get => invertHorizontalAxis;
            set => invertHorizontalAxis = value;
        }

        public bool InvertVerticalAxis
        {
            get => invertVerticalAxis;
            set => invertVerticalAxis = value;
        }

        public float FieldOfView
        {
            get => fieldOfView;
            set => fieldOfView = Mathf.Clamp(value, 50, 90);
        }
    }
}
