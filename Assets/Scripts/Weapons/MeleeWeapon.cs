using System.Collections;
using UnityEngine;
using UnnaturalSelection.Character;
using UnityEngine.InputSystem;
using System;
using UnnaturalSelection.Animation;

namespace UnnaturalSelection.Weapons
{
    public class MeleeWeapon : MonoBehaviour, IWeapon
    {
        [SerializeField]
        [Tooltip("Defines the reference to the character’s Main Camera transform.")]
        private Transform cameraTransformReference;

        [SerializeField]
        [Tooltip("Defines the reference to the First Person Character Controller.")]
        private MovementController fPController;

        [SerializeField]
        [Tooltip("Defines the maximum range of attacks.")]
        private float range = 1.5f;

        [SerializeField]
        [Tooltip("Defines the frequency of attacks.")]
        private float attackRate = 0.35f;

        [SerializeField]
        [Tooltip("Defines the force applied by each attack.")]
        private float force = 20;

        [SerializeField]
        private Vector2 damage = new Vector2(15, 30);

        [SerializeField]
        [Tooltip("The Layers affected by a attack.")]
        private LayerMask affectedLayers = 1;

        [SerializeField] private WeaponSwing weaponSwing = new WeaponSwing();

        [SerializeField] private MotionAnimation motionAnimation = new MotionAnimation();

        [SerializeField] private ArmsAnimator armsAnimator = new ArmsAnimator();

        private Camera _camera;
        private bool armsActive;
        private float nexAttackTime;
        private float nextInteractTime;

        private InputActionMap inputBindings;
        private InputAction fireAction;
        private InputAction aimAction;

        public int Identifier => GetInstanceID();

        public GameObject Viewmodel => gameObject;

        public bool CanSwitch => armsActive && fPController.State != MotionState.Running && nexAttackTime < Time.time;

        public bool CanUseEquipment => armsActive && fPController.State != MotionState.Running && nexAttackTime < Time.time;

        public bool IsBusy => !armsActive || nexAttackTime > Time.time;

        public float Size => range;

        public float HideAnimationLength => armsAnimator.HideAnimationLength;

        public float InteractAnimationLength => armsAnimator.InteractAnimationLength;

        public float InteractDelay => armsAnimator.InteractDelay;

        public bool MeleeAttacking => nexAttackTime > Time.time;

        public bool Interacting => nextInteractTime > Time.time;

        public bool Idle => !MeleeAttacking && !MeleeAttacking && !Interacting;

        private void Awake()
        {
            if (!cameraTransformReference)
            {
                throw new Exception("Camera Transform Reference was not assigned");
            }

            if (!fPController)
            {
                throw new Exception("FPController was not assigned");
            }
        }

        private void Start()
        {
            _camera = cameraTransformReference.GetComponent<Camera>();

            InitSwing(transform);
            DisableShadowCasting();

            // Input Bindings
            inputBindings = GameplayManager.Instance.GetActionMap("Weapons");
            inputBindings.Enable();

            fireAction = inputBindings.FindAction("Fire");
            aimAction = inputBindings.FindAction("Aim");

            // Events callbacks
            fPController.preJumpEvent += WeaponJump;
            fPController.landingEvent += WeaponLanding;
            fPController.vaultEvent += Vault;
            fPController.gettingUpEvent += Vault;
        }

        private void Update()
        {
            if (armsActive)
            {
                if (fPController.IsControllable)
                    HandleInput();

                fPController.ReadyToVault = !IsBusy;
                armsAnimator.SetSpeed(fPController.State == MotionState.Running);
            }

            weaponSwing.Swing(transform.parent, fPController);
            motionAnimation.MovementAnimation(fPController);
            motionAnimation.BreathingAnimation(fPController.IsAiming ? 0 : 1);
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * 10);
        }

        /// <summary>
        /// Draw the weapon.
        /// </summary>
        public void Select()
        {
            // Animator
            if (!armsAnimator.Initialized)
                armsAnimator.Init(fPController);

            armsAnimator.Draw();
            StartCoroutine(Draw());
        }

        /// <summary>
        /// Deselect the weapon.
        /// </summary>
        public void Deselect()
        {
            armsActive = false;
            fPController.ReadyToVault = false;
            armsAnimator.Hide();
        }

        /// <summary>
        /// Method used to verify the actions the player wants to execute.
        /// </summary>
        private void HandleInput()
        {
            bool canAttack = fPController.State != MotionState.Running && nexAttackTime < Time.time && nextInteractTime < Time.time;

            if (canAttack)
            {
                if (fireAction.triggered)
                {
                    armsAnimator.LeftAttack();
                    StartCoroutine(Attack());
                }
                else if (aimAction.triggered)
                {
                    armsAnimator.RightAttack();
                    StartCoroutine(Attack());
                }
            }
        }

        /// <summary>
        /// Applies force and damage to objects hit by attacks.
        /// </summary>
        private IEnumerator Attack()
        {
            nexAttackTime = Time.time + attackRate;

            // Wait 0.1 seconds before applying damage/force.
            yield return new WaitForSeconds(0.1f);

            Vector3 direction = cameraTransformReference.TransformDirection(Vector3.forward);
            Vector3 origin = cameraTransformReference.transform.position;

            Ray ray = new Ray(origin, direction);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, range, affectedLayers, QueryTriggerInteraction.Collide))
            {
                armsAnimator.Hit(hitInfo.point);

                // If hit a rigidbody applies force to push.
                Rigidbody rigidBody = hitInfo.collider.GetComponent<Rigidbody>();
                if (rigidBody)
                {
                    rigidBody.AddForce(direction * force, ForceMode.Impulse);
                }

                if (hitInfo.transform.root != transform.root)
                {
                    IProjectileDamageable damageableTarget = hitInfo.collider.GetComponent<IProjectileDamageable>();
                    damageableTarget?.ProjectileDamage(UnityEngine.Random.Range(damage.x, damage.y), transform.root.position, hitInfo.point, 0);
                }
            }
        }

        /// <summary>
        /// Wait until the weapon is selected and then enable its features.
        /// </summary>
        private IEnumerator Draw()
        {
            yield return new WaitForSeconds(armsAnimator.DrawAnimationLength);
            armsActive = true;
        }

        #region ANIMATIONS

        /// <summary>
        /// Start the WeaponSwing component and move the weapon to the transform that will be animated.
        /// </summary>
        /// <param name="weaponSwing">The weapon transform.</param>
        private void InitSwing(Transform weaponSwing)
        {
            if (!weaponSwing.parent.name.Equals("WeaponSwing"))
            {
                Transform parent = weaponSwing.parent.Find("WeaponSwing");

                if (parent != null)
                {
                    weaponSwing.parent = parent;
                }
                else
                {
                    GameObject weaponController = new GameObject("WeaponSwing");
                    weaponController.transform.SetParent(weaponSwing.parent, false);
                    weaponSwing.parent = weaponController.transform;
                }
            }
            this.weaponSwing.Init(weaponSwing.parent, motionAnimation.ScaleFactor);
        }

        /// <summary>
        /// Event method that simulates the effect of jump on the motion animation component.
        /// </summary>
        private void WeaponJump()
        {
            if (armsActive)
                StartCoroutine(motionAnimation.JumpAnimation.Play());
        }

        /// <summary>
        /// Event method that simulates the effect of landing on the motion animation component.
        /// </summary>
        private void WeaponLanding(float fallDamage)
        {
            if (armsActive)
                StartCoroutine(motionAnimation.LandingAnimation.Play());
        }

        #endregion

        /// <summary>
        /// Deactivates the shadows created by the weapon.
        /// </summary>
        public void DisableShadowCasting()
        {
            // For each object that has a renderer inside the weapon gameObject
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        /// <summary>
        /// Plays the interaction animation.
        /// </summary>
        public void Interact()
        {
            nextInteractTime = Time.time + Mathf.Max(InteractAnimationLength, InteractDelay);
            armsAnimator.Interact();
        }

        public void SetCurrentRounds(int currentRounds)
        {
            // This method will be removed in future updates.
            // Yeah it's a bad implementation and will definitely be changed :)
        }

        /// <summary>
        /// Event method that simulates the effect of vaulting on the motion animation component.
        /// </summary>
        private void Vault()
        {
            if (armsActive)
                armsAnimator.Vault();
        }
    }
}
