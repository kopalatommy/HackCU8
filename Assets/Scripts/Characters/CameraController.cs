using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnnaturalSelection.Character
{
    [System.Serializable]
    public class CameraController
    {
        [SerializeField]
        [Tooltip("Defines how horizontally sensitive the camera will be to mouse movement.")]
        private float yawSensitivity = 3f;

        [SerializeField]
        [Tooltip("Defines how vertically sensitive the camera will be to mouse movement.")]
        private float pitchSensitivity = 3f;

        [SerializeField]
        [Tooltip("Defines how horizontally sensitive the camera will be to mouse movement while aiming.")]
        private float aimingYawSensitivity = 1f;

        [SerializeField]
        [Tooltip("Defines how vertically sensitive the camera will be to mouse movement while aiming.")]
        private float aimingPitchSensitivity = 1f;

        [SerializeField]
        [Tooltip("Limits the camera’s vertical rotation (pitch).")]
        private bool limitPitchRotation = true;

        [SerializeField]
        private Vector2 pitchLimit = new Vector2(-75f, 80);

        [SerializeField]
        [Tooltip("Defines how fast the camera will decelerate.")]
        private float smoothness = 0.3f;

        private float minPitch;
        private float maxPitch;
        private Quaternion characterTargetRot;
        private Quaternion cameraTargetRot;

        private Transform characterReference;
        private Transform cameraReference;

        private InputActionMap inputBindings;
        private InputAction mouseAction;

        public bool Controllable
        {
            get;
            set;
        }

        public float CurrentPitch => cameraTargetRot.x;

        public float CurrentYaw
        {
            get;
            private set;
        }

        public Vector2 CurrentSensitivity
        {
            private set;
            get;
        }

        public void Init(Transform character, Transform camera)
        {
            characterReference = character;
            cameraReference = camera;

            characterTargetRot = character.localRotation;
            cameraTargetRot = camera.localRotation;

            minPitch = pitchLimit.x;
            maxPitch = pitchLimit.y;

            // Input Bindings
            inputBindings = GameplayManager.Instance.GetActionMap("Movement");
            inputBindings.Enable();

            mouseAction = inputBindings.FindAction("Mouse");
        }

        /// <summary>
        /// Forces the character to look at a position in the world.
        /// </summary>
        /// <param name="position">The target position.</param>
        public void LookAt(Vector3 position)
        {
            Vector3 characterDirection = position - characterReference.position;
            characterDirection.y = 0;

            // Forces the character to look at the target position.
            characterTargetRot = Quaternion.Slerp(characterTargetRot, Quaternion.LookRotation(characterDirection), 10 * Time.deltaTime);
            characterReference.localRotation = Quaternion.Slerp(characterReference.localRotation, characterTargetRot, 10 * Time.deltaTime);
        }

        /// <summary>
        /// Defines whether the character camera should use the default pitch settings or override it.
        /// </summary>
        /// <param name="overridePitchLimit">Should the character uses custom pitch values?</param>
        /// <param name="minPitch">Camera minimum pitch.</param>
        /// <param name="maxPitch">Camera maximum pitch.</param>
        public void OverrideCameraPitchLimit(bool overridePitchLimit, float minPitch, float maxPitch)
        {
            this.minPitch = overridePitchLimit ? minPitch : pitchLimit.x;
            this.maxPitch = overridePitchLimit ? maxPitch : pitchLimit.y;
        }

        /// <summary>
        /// Updates the character and camera rotation based on the player input.
        /// </summary>
        /// <param name="isAiming">Is the character aiming?</param>
        public void UpdateRotation(bool isAiming)
        {
            if (!Controllable)
                return;

            // Avoids the mouse looking if the game is effectively paused.
            if (Mathf.Abs(Time.timeScale) < float.Epsilon)
                return;

            CurrentSensitivity = new Vector2(isAiming ? aimingYawSensitivity : yawSensitivity, isAiming ? aimingPitchSensitivity : pitchSensitivity);

            Vector2 mouseDelta = mouseAction.ReadValue<Vector2>();

            float xRot = (GameplayManager.Instance.InvertVerticalAxis ? -mouseDelta.y : mouseDelta.y)
                         * CurrentSensitivity.y * GameplayManager.Instance.OverallMouseSensitivity;

            CurrentYaw = (GameplayManager.Instance.InvertHorizontalAxis ? -mouseDelta.x : mouseDelta.x)
                * CurrentSensitivity.x * GameplayManager.Instance.OverallMouseSensitivity;

            characterTargetRot *= Quaternion.Euler(0f, CurrentYaw, 0f);
            cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (limitPitchRotation)
            {
                cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot, -maxPitch, -minPitch);
            }

            if (smoothness > 0)
            {
                characterReference.localRotation = Quaternion.Slerp(characterReference.localRotation, characterTargetRot, 10 / smoothness * Time.deltaTime);
                cameraReference.localRotation = Quaternion.Slerp(cameraReference.localRotation, cameraTargetRot, 10 / smoothness * Time.deltaTime);
            }
            else
            {
                characterReference.localRotation = characterTargetRot;
                cameraReference.localRotation = cameraTargetRot;
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q, float minimum, float maximum)
        {
            q.x /= q.w;
            q.y = 0;
            q.z = 0;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, minimum, maximum);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
