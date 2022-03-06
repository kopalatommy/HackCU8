using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using UnnaturalSelection.Character;
using UnnaturalSelection.Character.CameraControls;
using UnnaturalSelection.Animation;
using UnnaturalSelection.Environment;
using UnnaturalSelection.Audio;

namespace UnnaturalSelection.Weapons
{
    [DisallowMultipleComponent]
    public class Gun : MonoBehaviour, IWeapon
    {
        [SerializeField]
        [Tooltip("GunData Asset is a container responsible for defining individual weapon characteristics.")]
        private GunData gunData;

        [SerializeField]
        [Tooltip("Defines the reference to the inventory system.")]
        private WeaponManager inventoryManager;

        [SerializeField]
        [Tooltip("Defines the reference to the character’s Main Camera transform.")]
        private Transform cameraTransformReference;

        [SerializeField]
        [Tooltip("Defines the reference to the First Person Character Controller.")]
        private MovementController fPController;

        [SerializeField]
        [Tooltip("Defines the reference to the Camera Animator.")]
        private CameraAnimator cameraAnimationsController;

        [SerializeField]
        private WeaponSwing weaponSwing = new WeaponSwing();

        [SerializeField]
        private MotionAnimation motionAnimation = new MotionAnimation();

        [SerializeField]
        private WeaponKickbackAnimation weaponKickbackAnimation;

        [SerializeField]
        private WeaponKickbackAnimation aimingWeaponKickbackAnimation;

        [SerializeField]
        private CameraKickbackAnimation cameraKickbackAnimation;

        [SerializeField]
        private CameraKickbackAnimation aimingCameraKickbackAnimation;

        [SerializeField]
        private GunAnimator gunAnimator = new GunAnimator();

        [SerializeField]
        private GunEffects gunEffects = new GunEffects();

        private bool gunIsActive;
        private bool isAiming;
        private bool isReloading;
        private bool isAttacking;

        private WaitForSeconds reloadDuration;
        private WaitForSeconds completeReloadDuration;

        private WaitForSeconds startReloadDuration;
        private WaitForSeconds insertInChamberDuration;
        private WaitForSeconds insertDuration;
        private WaitForSeconds stopReloadDuration;

        private float fireInterval;
        private float nextFireTime;
        private float nextReloadTime;
        private float nextSwitchModeTime;
        private float nextInteractTime;
        private float accuracy;

        private Camera _camera;
        private float isShooting;
        private Vector3 nextShootDirection;

        private InputActionMap inputBindings;
        private InputAction fireAction;
        private InputAction aimAction;
        private InputAction reloadAction;
        private InputAction meleeAction;
        private InputAction fireModeAction;

        #region EDITOR

        /// <summary>
        /// Returns the ReloadMode used by this gun.
        /// </summary>
        public GunData.ReloadMode ReloadType => gunData != null ? gunData.ReloadType : GunData.ReloadMode.Magazines;

        /// <summary>
        /// Returns true if this gun has secondary firing mode, false otherwise.
        /// </summary>
        public bool HasSecondaryMode => gunData != null && gunData.SecondaryFireMode != GunData.FireMode.None;

        /// <summary>
        /// Returns true if this gun has a chamber, false otherwise.
        /// </summary>
        public bool HasChamber => gunData != null && gunData.HasChamber;

        /// <summary>
        /// Returns the name of the weapon displayed on the Inspector tab.
        /// </summary>
        public string InspectorName => gunData != null ? gunData.GunName : "No Name";

        #endregion

        #region GUN PROPERTIES

        /// <summary>
        /// Returns true if this weapon is ready to be replaced, false otherwise.
        /// </summary>
        public virtual bool CanSwitch
        {
            get
            {
                if (!fPController)
                    return false;

                return gunIsActive && nextSwitchModeTime < Time.time && !isAttacking && nextInteractTime < Time.time && nextFireTime < Time.time;
            }
        }

        /// <summary>
        /// Returns true if the character is not performing an action that prevents him from using items, false otherwise.
        /// </summary>
        public virtual bool CanUseEquipment
        {
            get
            {
                if (!fPController)
                    return false;

                return gunIsActive && !IsAiming && !isReloading && nextReloadTime < Time.time && fPController.State != MotionState.Running
                && nextFireTime < Time.time && nextSwitchModeTime < Time.time && !isAttacking && nextInteractTime < Time.time;
            }
        }

        /// <summary>
        /// Returns true if the character is performing an action that prevents him from vaulting, false otherwise.
        /// </summary>
        public virtual bool IsBusy
        {
            get
            {
                if (!fPController)
                    return true;

                return !gunIsActive || IsAiming || isReloading || nextReloadTime > Time.time || nextFireTime > Time.time
                || nextSwitchModeTime > Time.time || isAttacking || nextInteractTime > Time.time;
            }
        }

        /// <summary>
        /// Returns true if the character is aiming with this weapon, false otherwise.
        /// </summary>
        public virtual bool IsAiming => gunAnimator.IsAiming;

        /// <summary>
        /// Returns the maximum number of rounds a magazine can hold.
        /// </summary>
        public int RoundsPerMagazine
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current number of rounds that are in the magazine coupled to the gun.
        /// </summary>
        public int CurrentRounds
        {
            get;
            protected set;
        }

        /// <summary>
        /// 
        /// </summary>
        public GunData GunData => gunData;

        /// <summary>
        /// Return the weapon identifier.
        /// </summary>
        public int Identifier => gunData != null ? gunData.GetInstanceID() : -1;

        /// <summary>
        /// Return the name of the gun.
        /// </summary>
        public string GunName => gunData != null ? gunData.GunName : "No Name";

        /// <summary>
        /// Returns the current accuracy of the gun.
        /// </summary>
        public float Accuracy => accuracy;

        /// <summary>
        /// Returns the selected fire mode on the gun.
        /// </summary>
        public GunData.FireMode FireMode
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns the viewmodel (GameObject) of this gun.
        /// </summary>
        public GameObject Viewmodel => gameObject;

        /// <summary>
        /// Returns the dropped object when swapping the gun.
        /// </summary>
        public GameObject DroppablePrefab => gunData != null ? gunData.DroppablePrefab : null;

        /// <summary>
        /// Returns the weight of the gun.
        /// </summary>
        public float Weight => gunData != null ? gunData.Weight : 0;

        /// <summary>
        /// Returns the length of the gun.
        /// </summary>
        public float Size => gunData != null ? gunData.Size : 0;

        /// <summary>
        /// 
        /// </summary>
        public float HideAnimationLength => gunAnimator.HideAnimationLength;

        public float DrawAnimationLenght => gunAnimator.DrawAnimationLength;

        public float InteractAnimationLength => gunAnimator.InteractAnimationLength;
        public float InteractDelay => gunAnimator.InteractDelay;

        public bool Reloading => isReloading || nextReloadTime > Time.time;
        public bool Firing => nextFireTime > Time.time;
        public bool MeleeAttacking => isAttacking;
        public bool Interacting => nextInteractTime > Time.time;

        public bool Idle => !Reloading && !Firing && !MeleeAttacking && !Interacting;

        public bool OutOfAmmo => CurrentRounds == 0;

        #endregion

        protected virtual void Awake()
        {
            if (!gunData)
            {
                throw new Exception("Gun Controller was not assigned");
            }

            if (!cameraTransformReference)
            {
                throw new Exception("Camera Transform Reference was not assigned");
            }

            if (!cameraAnimationsController)
            {
                throw new Exception("Camera Animations Controller was not assigned");
            }

            if (!fPController)
            {
                throw new Exception("FPController was not assigned");
            }

            if (!inventoryManager)
            {
                throw new Exception("Inventory Manager was not assigned");
            }
        }

        private void Start()
        {
            FireMode = gunData.PrimaryFireMode;
            fireInterval = gunData.PrimaryRateOfFire;
            RoundsPerMagazine = gunData.RoundsPerMagazine;

            if (motionAnimation.Lean)
                motionAnimation.LeanAmount = 0;

            switch (gunData.ReloadType)
            {
                case GunData.ReloadMode.Magazines:
                    reloadDuration = new WaitForSeconds(gunAnimator.ReloadAnimationLength);
                    completeReloadDuration = new WaitForSeconds(gunAnimator.FullReloadAnimationLength);
                    break;
                case GunData.ReloadMode.BulletByBullet:
                    startReloadDuration = new WaitForSeconds(gunAnimator.StartReloadAnimationLength);
                    insertInChamberDuration = new WaitForSeconds(gunAnimator.InsertInChamberAnimationLength / 2);
                    insertDuration = new WaitForSeconds(gunAnimator.InsertAnimationLength / 2);
                    stopReloadDuration = new WaitForSeconds(gunAnimator.StopReloadAnimationLength);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            gunEffects.Init();
            InitSwing(transform);
            DisableShadowCasting();

            // Input Bindings
            inputBindings = GameplayManager.Instance.GetActionMap("Weapons");
            inputBindings.Enable();

            fireAction = inputBindings.FindAction("Fire");
            aimAction = inputBindings.FindAction("Aim");
            reloadAction = inputBindings.FindAction("Reload");
            meleeAction = inputBindings.FindAction("Melee");
            fireModeAction = inputBindings.FindAction("Fire Mode");

            fPController.preJumpEvent += PrepareForJump;
            fPController.jumpEvent += WeaponJump;
            fPController.landingEvent += WeaponLanding;
            fPController.vaultEvent += Vault;
            fPController.gettingUpEvent += GettingUp;
        }

        private void Update()
        {
            if (gunIsActive)
            {
                cameraAnimationsController.HoldBreath = gunAnimator.CanHoldBreath;
                fPController.IsAiming = gunAnimator.IsAiming;
                fPController.ReadyToVault = !IsBusy;
                isShooting = Mathf.MoveTowards(isShooting, 0, Time.deltaTime);
                gunAnimator.SetCrouchStatus(fPController.IsCrouched);
                motionAnimation.StabiliseWeaponRecoil();

                if (fPController.IsControllable)
                    HandleInput();
            }

            if (isAiming)
            {
                gunAnimator.Aim(true);
            }
            else
            {
                bool canSprint = fPController.State == MotionState.Running && !isReloading && nextReloadTime < Time.time
                                 && nextSwitchModeTime < Time.time && nextFireTime < Time.time && gunIsActive && !isAttacking
                                 && nextInteractTime < Time.time && !fPController.IsSliding;

                gunAnimator.Sprint(canSprint, fPController.IsSliding);
            }

            accuracy = Mathf.Clamp(Mathf.MoveTowards(accuracy, GetCurrentAccuracy(), Time.deltaTime *
                                    (isShooting > 0 ? gunData.DecreaseRateByShooting : gunData.DecreaseRateByWalking)),
                                    gunData.BaseAccuracy, gunData.AIMAccuracy);

            bool canLean = !isAttacking && fPController.State != MotionState.Running && nextInteractTime < Time.time
                                        && cameraAnimationsController != null && motionAnimation.Lean;

            motionAnimation.LeanAnimation(canLean ? cameraAnimationsController.LeanDirection : 0);

            weaponSwing.Swing(transform.parent, fPController);
            motionAnimation.MovementAnimation(fPController);
            motionAnimation.BreathingAnimation(fPController.IsAiming ? 0 : 1);
        }

        private float GetCurrentAccuracy()
        {
            if (gunAnimator.IsAiming)
            {
                if (isShooting > 0)
                    return gunData.HIPAccuracy;

                return fPController.State != MotionState.Idle ? gunData.HIPAccuracy : gunData.AIMAccuracy;
            }
            if (isShooting > 0)
                return gunData.BaseAccuracy;

            return fPController.State != MotionState.Idle ? gunData.BaseAccuracy : gunData.HIPAccuracy;
        }

        public void InitializeMagazineAsDefault()
        {
            CurrentRounds = gunData.HasChamber ? gunData.RoundsPerMagazine + 1 : gunData.RoundsPerMagazine;
        }

        public void Select()
        {
            if (!gunAnimator.Initialized)
            {
                _camera = cameraTransformReference.GetComponent<Camera>();
                gunAnimator.Init(transform, _camera);
            }

            gunAnimator.Draw();
            StartCoroutine(Draw());
        }

        private IEnumerator Draw()
        {
            yield return new WaitForSeconds(gunAnimator.DrawAnimationLength);
            gunIsActive = true;
        }

        public void Deselect()
        {
            gunIsActive = false;
            isAiming = false;
            isReloading = false;
            nextReloadTime = 0;
            fPController.IsAiming = false;
            fPController.ReadyToVault = false;
            isShooting = 0;
            gunAnimator.Hide();
        }

        public void DeselectImmediately()
        {
            gunIsActive = false;
            isAiming = false;
            isReloading = false;
            nextReloadTime = 0;
            fPController.IsAiming = false;
            fPController.ReadyToVault = false;
            isShooting = 0;
        }

        public void SetCurrentRounds(int currentRounds)
        {
            CurrentRounds = currentRounds;
        }

        private void HandleInput()
        {
            // Restrictions:
            // Is firing = m_NextFireTime > Time.time
            // Is reloading = m_IsReloading || m_NextReloadTime > Time.time
            // Is empty = CurrentRounds == 0
            // Is running = m_FPController.State == MotionState.Running
            // Is attacking = m_Attacking
            // Is switching mode = m_NextSwitchModeTime > Time.time
            // Is interacting = m_NextInteractTime > Time.time
            // Can reload = Magazines > 0

            bool canShoot = !isReloading && nextReloadTime < Time.time && nextFireTime < Time.time && CurrentRounds >= 0
                            && (fPController.State != MotionState.Running || fPController.IsSliding) && !isAttacking
                            && nextSwitchModeTime < Time.time && nextInteractTime < Time.time;

            if (canShoot)
            {
                if (FireMode == GunData.FireMode.FullAuto || FireMode == GunData.FireMode.ShotgunAuto)
                {
                    if (fireAction.ReadValue<float>() > 0)
                    {
                        if (CurrentRounds == 0 && inventoryManager.GetAmmo(gunData.AmmoType) > 0)
                        {
                            Reload();
                        }
                        else
                        {
                            PullTheTrigger();
                        }
                    }
                }
                else if (FireMode == GunData.FireMode.Single || FireMode == GunData.FireMode.ShotgunSingle || FireMode == GunData.FireMode.Burst)
                {
                    if (fireAction.triggered)
                    {
                        if (CurrentRounds == 0 && inventoryManager.GetAmmo(gunData.AmmoType) > 0)
                        {
                            Reload();
                        }
                        else
                        {
                            PullTheTrigger();
                        }
                    }
                }
            }

            if (gunData.ReloadType == GunData.ReloadMode.BulletByBullet && isReloading && nextReloadTime < Time.time && CurrentRounds > (gunData.HasChamber ? 1 : 0))
            {
                if (fireAction.triggered)
                {
                    isReloading = false;
                    StartCoroutine(StopReload());
                }
            }

            bool canAim = !isReloading && nextReloadTime < Time.time && fPController.State != MotionState.Running && !isAttacking && nextInteractTime < Time.time;

            if (canAim)
            {
                if (aimAction.triggered && !isAiming)
                {
                    isAiming = !isAiming;
                }
                else if (aimAction.triggered && isAiming && gunAnimator.IsAiming)
                {
                    isAiming = !isAiming;
                }
            }
            else
            {
                isAiming = false;
            }

            bool canReload = !isReloading && nextReloadTime < Time.time && CurrentRounds < (gunData.HasChamber ? RoundsPerMagazine + 1 : RoundsPerMagazine) && inventoryManager.GetAmmo(gunData.AmmoType) > 0
                             && !isAttacking && nextSwitchModeTime < Time.time && nextInteractTime < Time.time && nextFireTime < Time.time;

            if (canReload)
            {
                if (reloadAction.triggered)
                {
                    Reload();
                }
            }

            bool canAttack = !isAttacking && !isReloading && nextReloadTime < Time.time && fPController.State != MotionState.Running && !IsAiming
                             && nextFireTime < Time.time && gunAnimator.CanMeleeAttack && nextSwitchModeTime < Time.time && nextInteractTime < Time.time;

            if (canAttack)
            {
                if (meleeAction.triggered)
                {
                    StartCoroutine(MeleeAttack());
                }
            }

            bool canChangeFireMode = HasSecondaryMode && !isAttacking && !isReloading
                                     && nextReloadTime < Time.time && fPController.State != MotionState.Running && nextSwitchModeTime < Time.time
                                     && nextInteractTime < Time.time;

            if (canChangeFireMode)
            {
                if (fireModeAction.triggered)
                {
                    if (FireMode == gunData.PrimaryFireMode)
                    {
                        nextSwitchModeTime = Time.time + gunAnimator.SwitchModeAnimationLength;
                        gunAnimator.SwitchMode();

                        FireMode = gunData.SecondaryFireMode;
                        fireInterval = gunData.SecondaryRateOfFire;
                    }
                    else
                    {
                        nextSwitchModeTime = Time.time + gunAnimator.SwitchModeAnimationLength;
                        gunAnimator.SwitchMode();

                        FireMode = gunData.PrimaryFireMode;
                        fireInterval = gunData.PrimaryRateOfFire;
                    }
                }
            }
        }

        private void PullTheTrigger()
        {
            if (CurrentRounds > 0 && inventoryManager.GetAmmo(gunData.AmmoType) >= 0)
            {
                if (FireMode == GunData.FireMode.FullAuto || FireMode == GunData.FireMode.Single)
                {
                    nextFireTime = Time.time + fireInterval;
                    CurrentRounds--;

                    nextShootDirection = GetBulletSpread();
                    Shot();

                    isShooting = 0.1f;

                    gunAnimator.Shot(CurrentRounds == 0);
                    gunEffects.Play();

                    motionAnimation.WeaponRecoilAnimation(IsAiming ? aimingWeaponKickbackAnimation : weaponKickbackAnimation);

                    if (cameraAnimationsController)
                    {
                        cameraAnimationsController.ApplyRecoil(IsAiming ? aimingCameraKickbackAnimation : cameraKickbackAnimation);
                    }
                }
                else if (FireMode == GunData.FireMode.ShotgunAuto || FireMode == GunData.FireMode.ShotgunSingle)
                {
                    nextFireTime = Time.time + fireInterval;
                    CurrentRounds--;

                    for (int i = 0; i < gunData.BulletsPerShoot; i++)
                    {
                        nextShootDirection = GetBulletSpread();
                        Shot();
                    }

                    isShooting = 0.1f;

                    gunAnimator.Shot(CurrentRounds == 0);
                    gunEffects.Play();

                    motionAnimation.WeaponRecoilAnimation(IsAiming ? aimingWeaponKickbackAnimation : weaponKickbackAnimation);

                    if (cameraAnimationsController)
                    {
                        cameraAnimationsController.ApplyRecoil(IsAiming ? aimingCameraKickbackAnimation : cameraKickbackAnimation);
                    }

                }
                else if (FireMode == GunData.FireMode.Burst)
                {
                    nextFireTime = Time.time + fireInterval * (gunData.BulletsPerBurst + 1);
                    StartCoroutine(Burst());
                }
            }
            else
            {
                nextFireTime = Time.time + 0.25f;
                gunAnimator.OutOfAmmo();
            }
        }

        private IEnumerator Burst()
        {
            for (int i = 0; i < gunData.BulletsPerBurst; i++)
            {
                if (CurrentRounds == 0)
                    break;

                nextShootDirection = GetBulletSpread();
                CurrentRounds--;
                Shot();

                isShooting = 0.1f;

                gunAnimator.Shot(CurrentRounds == 0);
                gunEffects.Play();

                motionAnimation.WeaponRecoilAnimation(IsAiming ? aimingWeaponKickbackAnimation : weaponKickbackAnimation);

                if (cameraAnimationsController)
                {
                    cameraAnimationsController.ApplyRecoil(IsAiming ? aimingCameraKickbackAnimation : cameraKickbackAnimation);
                }
                yield return new WaitForSeconds(fireInterval);
            }
        }

        private void Shot()
        {
            Vector3 direction = cameraTransformReference.TransformDirection(nextShootDirection);
            Vector3 origin = cameraTransformReference.transform.position;

            Ray ray = new Ray(origin, direction);

            float tracerDuration = gunEffects.TracerDuration;

            if (Physics.Raycast(ray, out RaycastHit hitInfo, gunData.Range, gunData.AffectedLayers, QueryTriggerInteraction.Collide))
            {
                float damage = gunData.CalculateDamage(hitInfo.distance);

                //Debug.DrawLine(origin, hitInfo.point, Color.green, 10);

                SurfaceIdentifier surf = hitInfo.collider.GetSurface();
                bool hasSplintered = false;

                if (surf)
                {
                    bool canPenetrate = gunData.AmmoType.CanPenetrate && gunData.AmmoType.PenetrationPower > 0 && surf.CanPenetrate(hitInfo.triangleIndex);
                    float density = surf.Density(hitInfo.triangleIndex);

                    bool hasRicocheted = gunData.AmmoType.Ricochet && (!canPenetrate || density >= gunData.AmmoType.RicochetDensityThreshold) && gunData.AmmoType.RicochetChance > 0 && Random.Range(0.0f, 1.0f) <= gunData.AmmoType.RicochetChance;
                    hasSplintered = gunData.AmmoType.Fragmentation && canPenetrate && gunData.AmmoType.FragmentationChance > 0 && Random.Range(0.0f, 1.0f) <= gunData.AmmoType.FragmentationChance;

                    // Generates a bullet mark
                    BulletDecalsManager.Instance.CreateBulletDecal(surf, hitInfo);

                    if (hasRicocheted && Vector3.Angle(direction, hitInfo.normal) - 90 <= gunData.AmmoType.MaxIncidentAngle)
                    {
                        Vector3 reflection = direction - 2 * Vector3.Dot(direction, hitInfo.normal) * hitInfo.normal;
                        Vector3 ricochetDirection = reflection;

                        if (gunData.AmmoType.TrajectoryDeflection > 0)
                        {
                            float minDeflection = 0.5f - gunData.AmmoType.TrajectoryDeflection * 0.5f;
                            float maxDeflection = 1 - minDeflection;

                            ricochetDirection = new Vector3
                            {
                                x = reflection.x * Random.Range(Mathf.Min(minDeflection, maxDeflection), Mathf.Max(minDeflection, maxDeflection)),
                                y = reflection.y * Random.Range(Mathf.Min(minDeflection, maxDeflection), Mathf.Max(minDeflection, maxDeflection)),
                                z = reflection.z * Random.Range(Mathf.Min(minDeflection, maxDeflection), Mathf.Max(minDeflection, maxDeflection)),
                            };
                        }

                        float ricochetPower = Random.Range(0.01f, 0.25f);
                        float ricochetRange = (gunData.Range - hitInfo.distance) * ricochetPower;
                        Ricochet(hitInfo.point, ricochetDirection, gunData.AmmoType.PenetrationPower * ricochetPower, ricochetRange, damage * ricochetPower);

                        SurfaceType surfaceType = surf.GetSurfaceType(hitInfo.point, hitInfo.triangleIndex);
                        if (surfaceType)
                        {
                            AudioClip ricochetSound = surfaceType.GetRandomRicochetSound();
                            float ricochetVolume = surfaceType.BulletImpactVolume;

                            AudioManager.Instance.PlayClipAtPoint(ricochetSound, hitInfo.point, 3, 15, ricochetVolume);
                        }
                    }
                    else
                    {
                        if (hasSplintered && density >= gunData.AmmoType.FragmentationDensityThreshold)
                        {
                            int fragments = Random.Range(1, gunData.AmmoType.MaxFragments + 1);
                            for (int i = 0; i < fragments; i++)
                            {
                                Vector3 newDirection;
                                if (gunData.AmmoType.FragmentScattering.sqrMagnitude > 0)
                                {
                                    newDirection = new Vector3
                                    {
                                        x = direction.x * Random.Range(gunData.AmmoType.FragmentScattering[0], gunData.AmmoType.FragmentScattering[1]),
                                        y = direction.y * Random.Range(gunData.AmmoType.FragmentScattering[0], gunData.AmmoType.FragmentScattering[1]),
                                        z = 1
                                    };

                                    newDirection = cameraTransformReference.TransformDirection(newDirection);
                                }
                                else
                                {
                                    newDirection = direction;
                                }

                                float fragmentPower = Random.Range(0.01f, 0.25f);
                                float fragmentDamage = damage * fragmentPower;
                                float fragmentRange = (gunData.Range - hitInfo.distance) * fragmentPower;
                                float fragmentPenetrationPower = gunData.AmmoType.PenetrationPower * fragmentPower;

                                if (gunData.AmmoType.PenetrationPower > 0)
                                {
                                    Penetrate(hitInfo, newDirection, surf, fragmentPenetrationPower, fragmentRange, fragmentDamage);
                                }
                            }
                        }

                        if (canPenetrate)
                        {
                            Penetrate(hitInfo, direction, surf, gunData.AmmoType.PenetrationPower, gunData.Range - hitInfo.distance, damage);
                        }
                    }
                }

                if (hitInfo.transform.root != transform.root)
                {
                    IProjectileDamageable damageableTarget = hitInfo.collider.GetComponent<IProjectileDamageable>();
                    damageableTarget?.ProjectileDamage(hasSplintered ? damage * gunData.AmmoType.FragmentationDamageMultiplier : damage, transform.root.position, hitInfo.point, gunData.AmmoType.PenetrationPower);
                }

                // If hit a rigid body applies force to push.
                Rigidbody rigidBody = hitInfo.collider.attachedRigidbody;
                if (rigidBody)
                {
                    float impactForce = gunData.AmmoType.CalculateImpactForce(hitInfo.distance);
                    rigidBody.AddForce(direction * impactForce, ForceMode.Impulse);
                }

                tracerDuration = hitInfo.distance / gunEffects.TracerSpeed;
            }

            if (tracerDuration > 0.05f)
                gunEffects.CreateTracer(transform, direction, tracerDuration);
        }

        private void Ricochet(Vector3 origin, Vector3 direction, float penetrationPower, float range, float damage)
        {
            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, range, gunData.AffectedLayers, QueryTriggerInteraction.Collide))
            {
                SurfaceIdentifier surf = hitInfo.collider.GetSurface();

                //Debug.DrawLine(origin, hitInfo.point, Color.red, 10);

                if (surf)
                {
                    BulletDecalsManager.Instance.CreateBulletDecal(surf, hitInfo);
                }

                IProjectileDamageable damageableTarget = hitInfo.collider.GetComponent<IProjectileDamageable>();
                damageableTarget?.ProjectileDamage(damage, transform.root.position, hitInfo.point, penetrationPower);

                // If hit a rigidbody applies force to push.
                Rigidbody rigidBody = hitInfo.collider.attachedRigidbody;
                if (rigidBody)
                {
                    float impactForce = gunData.AmmoType.CalculateImpactForce(hitInfo.distance);
                    rigidBody.AddForce(direction * impactForce, ForceMode.Impulse);
                }
            }
        }

        private void Penetrate(RaycastHit lastHitInfo, Vector3 direction, SurfaceIdentifier surf, float penetrationPower, float range, float damage)
        {
            SurfaceIdentifier newSurf = surf;
            float remainingPower = penetrationPower;

            while (newSurf && remainingPower > 0 && newSurf.CanPenetrate(lastHitInfo.triangleIndex))
            {
                const float distanceEpsilon = 0.01f;
                Ray ray = new Ray(lastHitInfo.point + direction * distanceEpsilon, direction);

                int affectedObjectID = lastHitInfo.collider.GetInstanceID();

                if (Physics.Raycast(ray, out RaycastHit hitInfo, range, gunData.AffectedLayers, QueryTriggerInteraction.Collide))
                {
                    // Get the surface type of the object.
                    newSurf = hitInfo.collider.GetSurface();

                    // Exit hole
                    Ray exitRay = new Ray(hitInfo.point, direction * -1);

                    if (Physics.Raycast(exitRay, out RaycastHit exitInfo, hitInfo.distance + distanceEpsilon, gunData.AffectedLayers, QueryTriggerInteraction.Collide))
                    {
                        surf = exitInfo.collider.GetSurface();
                        float density = surf.Density(lastHitInfo.triangleIndex);
                        float distanceTraveled = Vector3.Distance(lastHitInfo.point, exitInfo.point) * density;

                        // Does the bullet gets through?
                        if (penetrationPower > distanceTraveled)
                        {
                            //Debug.DrawLine(lastHitInfo.point, hitInfo.point, Color.blue, 10);

                            BulletDecalsManager.Instance.CreateBulletDecal(newSurf, hitInfo);

                            if (affectedObjectID == exitInfo.collider.GetInstanceID())
                            {
                                BulletDecalsManager.Instance.CreateBulletDecal(surf, exitInfo);
                            }

                            // Make sure you don't hit yourself
                            if (hitInfo.transform.root != transform.root)
                            {
                                IProjectileDamageable damageableTarget = hitInfo.collider.GetComponent<IProjectileDamageable>();
                                damageableTarget?.ProjectileDamage(damage * (distanceTraveled / gunData.AmmoType.PenetrationPower), transform.root.position, exitInfo.point, gunData.AmmoType.PenetrationPower - distanceTraveled);
                            }

                            // If hit a rigidbody applies force to push.
                            Rigidbody rigidBody = hitInfo.collider.attachedRigidbody;
                            if (rigidBody)
                            {
                                float impactForce = gunData.AmmoType.CalculateImpactForce(hitInfo.distance);
                                rigidBody.AddForce(direction * impactForce, ForceMode.Impulse);
                            }

                            if (gunData.AmmoType.Refraction.sqrMagnitude > 0)
                            {
                                float densityInfluence = density * gunData.AmmoType.DensityInfluence;
                                Vector3 newDirection = new Vector3
                                {
                                    x = Random.Range(gunData.AmmoType.Refraction[0], gunData.AmmoType.Refraction[1]) * densityInfluence,
                                    y = Random.Range(gunData.AmmoType.Refraction[0], gunData.AmmoType.Refraction[1]) * densityInfluence,
                                    z = 1
                                };
                                direction = cameraTransformReference.TransformDirection(newDirection);
                            }

                            remainingPower = penetrationPower - distanceTraveled;
                            lastHitInfo = hitInfo;
                            penetrationPower = remainingPower;
                            range -= hitInfo.distance;
                            continue;
                        }
                    }
                }
                else
                {
                    // Exit hole
                    float maxDistance = penetrationPower / newSurf.Density(lastHitInfo.triangleIndex);

                    Ray exitRay = new Ray(lastHitInfo.point + direction * (maxDistance - distanceEpsilon), direction * -1);
                    if (Physics.Raycast(exitRay, out RaycastHit exitInfo, maxDistance, gunData.AffectedLayers, QueryTriggerInteraction.Collide))
                    {
                        if (affectedObjectID == exitInfo.collider.GetInstanceID())
                            BulletDecalsManager.Instance.CreateBulletDecal(newSurf, exitInfo);
                    }
                }

                break;
            }
        }

        private Vector3 GetBulletSpread()
        {
            if (Mathf.Abs(accuracy - 1) < Mathf.Epsilon)
            {
                return new Vector3(0, 0, 1);
            }
            else
            {
                Vector2 randomPointInScreen = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * ((1 - accuracy) * (gunData.MaximumSpread / 10));
                return new Vector3(randomPointInScreen.x, randomPointInScreen.y, 1);
            }
        }

        private IEnumerator MeleeAttack()
        {
            isAttacking = true;
            gunAnimator.Melee();
            yield return new WaitForSeconds(gunAnimator.MeleeDelay);

            Vector3 direction = cameraTransformReference.TransformDirection(Vector3.forward);
            Vector3 origin = cameraTransformReference.transform.position;
            float range = gunData.Size * 0.5f + fPController.Radius;

            Ray ray = new Ray(origin, direction);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, range, gunData.AffectedLayers, QueryTriggerInteraction.Collide))
            {
                gunAnimator.Hit(hitInfo.point);

                if (hitInfo.transform.root != transform.root)
                {
                    IProjectileDamageable damageableTarget = hitInfo.collider.GetComponent<IProjectileDamageable>();
                    damageableTarget?.ProjectileDamage(gunData.MeleeDamage, transform.root.position, hitInfo.point, 0);
                }

                // If hit a rigidbody applies force to push.
                Rigidbody rigidBody = hitInfo.collider.attachedRigidbody;
                if (rigidBody)
                {
                    rigidBody.AddForce(direction * gunData.MeleeForce, ForceMode.Impulse);
                }
            }

            yield return new WaitForSeconds(gunAnimator.MeleeAnimationLength - gunAnimator.MeleeDelay);
            isAttacking = false;
        }

        private void Reload()
        {
            if (gunData.ReloadType == GunData.ReloadMode.Magazines)
            {
                StartCoroutine(ReloadMagazines());

                if (CurrentRounds == 0 && gunEffects.FullReloadDrop)
                {
                    Invoke(nameof(DropMagazinePrefab), gunEffects.FullDropDelay);
                }
                else if (CurrentRounds > 0 && gunEffects.TacticalReloadDrop)
                {
                    Invoke(nameof(DropMagazinePrefab), gunEffects.TacticalDropDelay);
                }
            }
            else if (gunData.ReloadType == GunData.ReloadMode.BulletByBullet)
            {
                StartCoroutine(ReloadBulletByBullet());
            }
        }

        private IEnumerator ReloadMagazines()
        {
            isReloading = true;

            gunAnimator.Reload(CurrentRounds > 0);

            yield return CurrentRounds == 0 ? completeReloadDuration : reloadDuration;

            if (gunIsActive && isReloading)
            {
                if (CurrentRounds > 0)
                {
                    int amount = (gunData.HasChamber ? RoundsPerMagazine + 1 : RoundsPerMagazine) - CurrentRounds;
                    CurrentRounds += inventoryManager.RequestAmmunition(gunData.AmmoType, amount);
                }
                else
                {
                    CurrentRounds += inventoryManager.RequestAmmunition(gunData.AmmoType, RoundsPerMagazine);
                }
            }

            isReloading = false;
        }

        private IEnumerator ReloadBulletByBullet()
        {
            isReloading = true;

            gunAnimator.StartReload(CurrentRounds > 0);

            if (CurrentRounds == 0)
            {
                yield return insertInChamberDuration;
                CurrentRounds += inventoryManager.RequestAmmunition(gunData.AmmoType, 1);
                yield return insertInChamberDuration;
            }
            else
            {
                yield return startReloadDuration;
            }

            while (gunIsActive && CurrentRounds < (gunData.HasChamber ? RoundsPerMagazine + 1 : RoundsPerMagazine) && inventoryManager.GetAmmo(gunData.AmmoType) > 0 && isReloading)
            {
                gunAnimator.Insert();
                yield return insertDuration;

                if (gunIsActive && isReloading)
                {
                    CurrentRounds += inventoryManager.RequestAmmunition(gunData.AmmoType, 1);
                }
                yield return insertDuration;
            }

            if (gunIsActive && isReloading)
            {
                StartCoroutine(StopReload());
            }
        }

        private IEnumerator StopReload()
        {
            gunAnimator.StopReload();
            isReloading = false;
            nextReloadTime = gunAnimator.StopReloadAnimationLength + Time.time;
            yield return stopReloadDuration;
        }

        private void DropMagazinePrefab()
        {
            gunEffects.DropMagazine(fPController.GetComponent<Collider>());
        }

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

        private void PrepareForJump()
        {
            if (gunIsActive)
                StartCoroutine(motionAnimation.BraceForJumpAnimation.Play());
        }

        private void WeaponJump()
        {
            if (gunIsActive)
                StartCoroutine(motionAnimation.JumpAnimation.Play());
        }

        private void WeaponLanding(float fallDamage)
        {
            if (gunIsActive)
                StartCoroutine(motionAnimation.LandingAnimation.Play());
        }

        public void DisableShadowCasting()
        {
            // For each object that has a renderer inside the weapon gameObject
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.sharedMaterial.EnableKeyword("_VIEWMODEL");
            }
        }

        public virtual void Interact()
        {
            nextInteractTime = Time.time + Mathf.Max(InteractAnimationLength, InteractDelay);
            gunAnimator.Interact();
        }

        private void Vault()
        {
            if (gunIsActive)
                gunAnimator.Vault();
        }

        private void GettingUp()
        {
            if (gunIsActive && !IsBusy)
                gunAnimator.Vault();
        }
    }
}
