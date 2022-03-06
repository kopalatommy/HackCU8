using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnnaturalSelection.Animation;
using UnnaturalSelection.Audio;

namespace UnnaturalSelection.Character.CameraControls
{
    [DisallowMultipleComponent]
    public class CameraAnimator : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Defines the reference to the First Person Character Controller script.")]
        protected MovementController fPController;

        [SerializeField]
        [Tooltip("Defines the reference to the Health Controller script.")]
        protected HealthController healthController;

        [SerializeField]
        protected MotionAnimation motionAnimation = new MotionAnimation();

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the volume of Hold Breath Sound and Exhale Sound.")]
        protected float holdBreathVolume = 0.3f;

        private InputActionMap inputBindings;
        private InputAction steadyAimAction;
        private InputAction leaningAction;

        private float nextHoldBreathTime;
        private float holdBreathDuration;

        private IEnumerator currentShakeCoroutine;
        private AudioEmitter playerGenericSource;

        public bool HoldBreath
        {
            protected get;
            set;
        }

        public int LeanDirection
        {
            get;
            private set;
        }

        protected virtual void Start()
        {
            // Events callback
            fPController.preJumpEvent += CameraBraceForJump;
            fPController.jumpEvent += CameraJump;
            fPController.landingEvent += CameraLanding;
            fPController.vaultEvent += Vault;
            fPController.gettingUpEvent += Vault;

            healthController.ExplosionEvent += GrenadeExplosion;
            healthController.HitEvent += Hit;

            // Input Bindings
            inputBindings = GameplayManager.Instance.GetActionMap("Movement");
            inputBindings.Enable();

            steadyAimAction = GameplayManager.Instance.GetActionMap("Weapons").FindAction("Steady Aim");
            leaningAction = inputBindings.FindAction("Lean");

            // AudioSources
            playerGenericSource = AudioManager.Instance.RegisterSource("[AudioEmitter] Generic", transform.root);
        }

        protected virtual void Update()
        {
            motionAnimation.MovementAnimation(fPController);
            motionAnimation.StabiliseCameraRecoil();

            // Leaning animations
            if (!motionAnimation.Lean)
                return;

            if (fPController.State != MotionState.Flying && fPController.State != MotionState.Running && fPController.State != MotionState.Climbing)
            {
                if (leaningAction.ReadValue<float>() < 0)
                {
                    LeanDirection = CanLean(Vector3.left) ? -1 : 0;
                }
                else if (leaningAction.ReadValue<float>() > 0)
                {
                    LeanDirection = CanLean(Vector3.right) ? 1 : 0;
                }
                else
                {
                    LeanDirection = 0;
                }

                motionAnimation.LeanAnimation(LeanDirection);
            }
            else
            {
                LeanDirection = 0;
                motionAnimation.LeanAnimation(0);
            }
        }

        /// <summary>
        /// Casts a ray to evaluate if the character can lean to a determinate direction.
        /// </summary>
        /// <param name="direction">The desired direction.</param>
        private bool CanLean(Vector3 direction)
        {
            Ray ray = new Ray(fPController.transform.position, fPController.transform.TransformDirection(direction));
            return !Physics.SphereCast(ray, motionAnimation.LeanAmount, out _, motionAnimation.LeanAmount * 2, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Event method that simulates the effect of preparing to jump on the motion animation component.
        /// </summary>
        protected virtual void CameraBraceForJump()
        {
            StartCoroutine(motionAnimation.BraceForJumpAnimation.Play());
        }

        /// <summary>
        /// Event method that simulates the effect of jump on the motion animation component.
        /// </summary>
        protected virtual void CameraJump()
        {
            StartCoroutine(motionAnimation.JumpAnimation.Play());
        }

        /// <summary>
        /// Event method that simulates the effect of landing on the motion animation component.
        /// </summary>
        protected virtual void CameraLanding(float fallDamage)
        {
            StartCoroutine(motionAnimation.LandingAnimation.Play());
        }

        /// <summary>
        /// Updates the camera recoil settings and play a new recoil animation.
        /// </summary>
        public virtual void ApplyRecoil(CameraKickbackAnimation cameraKickbackAnimation)
        {
            // Update camera recoil properties
            motionAnimation.CameraRecoilAnimation(cameraKickbackAnimation);
        }

        /// <summary>
        /// Event method that simulates the effect of explosion on the motion animation component.
        /// </summary>
        protected virtual void GrenadeExplosion()
        {
            if (currentShakeCoroutine != null)
            {
                StopCoroutine(currentShakeCoroutine);
            }

            currentShakeCoroutine = motionAnimation.Shake(motionAnimation.ExplosionShake);
            StartCoroutine(currentShakeCoroutine);
        }

        /// <summary>
        /// Event method that simulates the effect of character hit by a projectile on the motion animation component.
        /// </summary>
        protected virtual void Hit()
        {
            StartCoroutine(motionAnimation.HitAnimation());
        }

        /// <summary>
        /// Event method that simulates the effect of vaulting on the motion animation component.
        /// </summary>
        protected virtual void Vault()
        {
            StartCoroutine(motionAnimation.VaultAnimation.Play());
        }
    }
}
