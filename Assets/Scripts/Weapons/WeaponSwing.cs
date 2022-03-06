using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnnaturalSelection.Character;

namespace UnnaturalSelection.Weapons
{
    public enum SwingTarget
    {
        Fist,
        Weapon
    }

    [System.Serializable]
    public class WeaponSwing
    {
        [SerializeField]
        [Tooltip("Enables the swinging feature in the weapon.")]
        private bool swing;

        [SerializeField]
        private Vector2 tiltAngle = new Vector2(5, 10);

        [SerializeField]
        private Vector2 swingAngle = new Vector2(4, 8);

        [SerializeField, Range(0, 1)]
        [Tooltip("Determines how fast the animation will be played.")]
        private float speed = 0.5f;

        [SerializeField]
        [Tooltip("Boosts the animation effect by allowing rotation on all axes.")]
        private bool m_AnimateAllAxes;

        [SerializeField]
        private Vector2 tiltBoost = new Vector2(2.5f, 5);

        [SerializeField, Range(0, 2)]
        [Tooltip("Defines the magnitude of the weapon shaking animation when the tremor effect is active.")]
        private float tremorAmount = 1;

        [SerializeField]
        [Tooltip("Defines the style of animation depending on the pivot.")]
        private SwingTarget swingTarget = SwingTarget.Weapon;

        private float scaleFactor = 1;
        private Vector3 targetPos;
        private Quaternion targetRot;

        private InputActionMap inputBindings;
        private InputAction mouseAction;

        internal void Init(Transform weaponSwing, float scaleFactor)
        {
            this.scaleFactor = scaleFactor;
            targetPos = weaponSwing.localPosition;
            targetRot = weaponSwing.localRotation;

            inputBindings = GameplayManager.Instance.GetActionMap("Movement");
            inputBindings.Enable();

            mouseAction = inputBindings.FindAction("Mouse");
        }

        internal void Swing(Transform weaponSwing, MovementController FPController)
        {
            if (Mathf.Abs(Time.timeScale) < float.Epsilon)
                return;

            if (swing)
            {
                Vector2 mouseDelta = mouseAction.ReadValue<Vector2>();

                // Calculates the swing angle by the mouse movement
                float yRot = Mathf.Clamp(mouseDelta.x * -swingAngle.x * GameplayManager.Instance.OverallMouseSensitivity, -swingAngle.y, swingAngle.y);
                float xRot = Mathf.Clamp(mouseDelta.y * -swingAngle.x * GameplayManager.Instance.OverallMouseSensitivity, -swingAngle.y, swingAngle.y);

                // Calculates the tilt angle by sideways movement
                float zRot = FPController.Velocity.sqrMagnitude > 1 && !FPController.IsSliding
                    ? Mathf.Clamp(FPController.MoveInput.x * -tiltAngle.x, -tiltAngle.y, tiltAngle.y) : 0;

                float zRotBoost = Mathf.Clamp(m_AnimateAllAxes ? mouseDelta.x * GameplayManager.Instance.OverallMouseSensitivity * -tiltBoost.x : 0, -tiltBoost.y, tiltBoost.y);

                // Simulates the tremor effect (shaking effect)
                if (FPController.TremorTrauma)
                {
                    yRot += UnityEngine.Random.Range(-1.0f, 1.0f) * tremorAmount;
                    xRot += UnityEngine.Random.Range(-1.0f, 1.0f) * tremorAmount;
                }

                if (swingTarget == SwingTarget.Fist)
                {
                    targetRot = Quaternion.Euler(xRot, yRot, zRot + zRotBoost);
                    targetPos = new Vector3(-yRot / 100 + (zRot + zRotBoost) / 500, xRot / 100, 0);

                    if (FPController.IsAiming)
                        targetPos /= 2;
                }
                else
                {
                    targetRot = Quaternion.Euler(-xRot, yRot, zRot + zRotBoost);
                    targetPos = new Vector3((zRot + zRotBoost) / 500, 0, 0);
                }
            }
            else
            {
                targetRot = Quaternion.identity;
                targetPos = Vector3.zero;
            }

            weaponSwing.localPosition = Vector3.Lerp(weaponSwing.localPosition, targetPos * scaleFactor, Time.deltaTime * speed * 10);
            weaponSwing.localRotation = Quaternion.Slerp(weaponSwing.localRotation, targetRot, Time.deltaTime * speed * 10);
        }
    }
}
