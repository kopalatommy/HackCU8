using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Animation;
using UnnaturalSelection.Audio;
using Random = UnityEngine.Random;

namespace UnnaturalSelection.Weapons
{
    [System.Serializable]
    public class GunAnimator : IWeaponAnimator
    {
        public enum AnimationType
        {
            Sequential,
            Random
        }

        [SerializeField]
        [Tooltip("Enables running animation.")]
        private bool runAnimation;

        [SerializeField]
        [Tooltip("Position of the transform when the character starts running")]
        private Vector3 runningPosition;

        [SerializeField]
        [Tooltip("Rotation of the transform when the character starts running.")]
        private Vector3 runningRotation;

        [SerializeField]
        [Tooltip("Speed of interpolation between HIP and Running position/rotation.")]
        private float runningSpeed = 10;

        [SerializeField]
        [Tooltip("Enables slide animation.")]
        private bool slidingAnimation;

        [SerializeField]
        [Tooltip("Position of the transform when the character starts sliding.")]
        private Vector3 slidingPosition;

        [SerializeField]
        [Tooltip("Rotation of the transform when the character starts sliding.")]
        private Vector3 slidingRotation;

        [SerializeField]
        [Tooltip("Speed of interpolation between HIP and Sliding position/rotation.")]
        private float slidingSpeed = 10;

        [SerializeField]
        [Tooltip("Enables aiming animation.")]
        private bool aimAnimation;

        [SerializeField]
        [Tooltip("Transform aiming position.")]
        private Vector3 aimingPosition;

        [SerializeField]
        [Tooltip("Transform aiming rotation.")]
        private Vector3 aimingRotation;

        [SerializeField]
        [Tooltip("Sound played when aiming the gun.")]
        private AudioClip aimInSound;

        [SerializeField]
        [Tooltip("Sound played when stop aiming the gun.")]
        private AudioClip aimOutSound;

        [SerializeField]
        [Tooltip("Enables camera FOV zoom animation.")]
        private bool zoomAnimation;

        [SerializeField]
        [Range(1, 179)]
        [Tooltip("Camera field of view while aiming.")]
        private float aimFOV = 50;

        [SerializeField]
        [Tooltip("Camera zoom speed.")]
        private float aimingSpeed = 10;

        [SerializeField]
        [Tooltip("Defines whether the character can hold his breath to stabilize the aiming.")]
        private bool holdBreath;

        [SerializeField]
        [Tooltip("Standing Position is the default position of the weapon.")]
        private Vector3 standingPosition;

        [SerializeField]
        [Tooltip("Standing Rotation is the default rotation of the weapon.")]
        private Vector3 standingRotation;

        [SerializeField]
        [Tooltip("Crouch Position will define the weapon position when the current state of the character is Crouch.")]
        private Vector3 crouchPosition;

        [SerializeField]
        [Tooltip("Crouch Rotation will define the weapon rotation when the current state of the character is Crouch.")]
        private Vector3 crouchRotation;

        [SerializeField]
        [Tooltip("Determines how fast the weapon can change from standing to crouching.")]
        private float crouchingSpeed = 5;

        [SerializeField]
        [Tooltip("Animator's reference.")]
        private Animator animator;

        [SerializeField]
        [Tooltip("The name of the animator parameter that defines the playing speed.")]
        private string speedParameter = "Speed";

        private int speedParameterHash;

        [SerializeField]
        [Tooltip("Enables Draw animation.")]
        private bool draw;

        [SerializeField]
        [Tooltip("The name of the draw animation.")]
        private string drawAnimation = "Draw";

        [SerializeField]
        [Tooltip("Execution speed of the draw animation.")]
        private float drawSpeed = 1;

        [SerializeField]
        [Tooltip("The sound played when drawing the weapon.")]
        private AudioClip drawSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of the sound of drawing the weapon.")]
        private float drawVolume = 0.25f;

        [SerializeField]
        [Tooltip("Enables the animation of hiding the weapon.")]
        private bool hide;

        [SerializeField]
        [Tooltip("The name of the hide the weapon animation.")]
        private string hideAnimation = "Hide";

        [SerializeField]
        [Tooltip("Execution speed of the hiding animation.")]
        private float hideSpeed = 1;

        [SerializeField]
        [Tooltip("The sound of hiding the weapon.")]
        private AudioClip hideSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of the sound of hiding the weapon.")]
        private float hideVolume = 0.25f;

        [SerializeField]
        [Tooltip("Enables shooting animation.")]
        private bool fire;

        [SerializeField]
        [Tooltip("List of shooting animations.")]
        private List<string> fireAnimationList = new List<string>();

        [SerializeField]
        [Tooltip("List of shooting animations while aiming.")]
        private List<string> aimedFireAnimationList = new List<string>();

        [SerializeField]
        [Tooltip("Defines the order of execution of the animations.")]
        private AnimationType fireAnimationType = AnimationType.Sequential;

        [SerializeField]
        [Tooltip("List of shooting sounds.")]
        private List<AudioClip> fireSoundList = new List<AudioClip>();

        [SerializeField]
        [Tooltip("Replaces last shot animation to simulate the slide stop animation.")]
        private bool overrideLastFire;

        [SerializeField]
        [Tooltip("Shooting and slide stop animation.")]
        private string lastFireAnimation = "Last Fire";

        [SerializeField]
        [Tooltip("Defines the speed at which shooting animations are executed.")]
        private float fireSpeed = 1;

        [SerializeField]
        [Tooltip("Defines the speed at which aimed shooting animations are executed.")]
        private float aimedFireSpeed = 1;

        [SerializeField]
        [Tooltip("Sound played when the gun is unloaded.")]
        private AudioClip outOfAmmoSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The shooting sound volume.")]
        private float fireVolume = 0.5f;

        [SerializeField]
        [Tooltip("Allows the firing sound to be additive, creating a new audio source instance at each shot, otherwise the gun will reset the audio source with each new shot.")]
        private bool additiveSound = true;

        private int lastIndex;

        [SerializeField]
        [Tooltip("Enables reload animation.")]
        private bool reload;

        [SerializeField]
        [Tooltip("Name of the tactical reload animation.")]
        private string reloadAnimation = "Reload";

        [SerializeField]
        [Tooltip("Reload animation speed.")]
        private float reloadSpeed = 1;

        [SerializeField]
        [Tooltip("Reload animation sound.")]
        private AudioClip reloadSound;

        [SerializeField]
        [Tooltip("Name of the full reload animation.")]
        private string reloadEmptyAnimation = "FullReload";

        [SerializeField]
        private float reloadEmptySpeed = 1;

        [SerializeField]
        [Tooltip("Full reload animation sound.")]
        private AudioClip reloadEmptySound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Reload sound volume.")]
        private float reloadVolume = 0.25f;

        [SerializeField]
        [Tooltip("Animation played at the start of the reload. (Used to position gun to receive bullets)")]
        private string startReloadAnimation = "Start Reload";

        /// <summary>
        /// Speed of the reload start animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Speed of the reload start animation.")]
        private float startReloadSpeed = 1;

        /// <summary>
        /// Sound played at the start of the reload.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound played at the start of the reload.")]
        private AudioClip startReloadSound;

        /// <summary>
        /// Volume of the sound played at the start of the reload.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Volume of the sound played at the start of the reload.")]
        private float startReloadVolume = 0.25f;

        /// <summary>
        /// Animation of inserting a bullet in the chamber to start reloading.
        /// </summary>
        [SerializeField]
        [Tooltip("Animation of inserting a bullet in the chamber to start reloading.")]
        private string insertInChamberAnimation = "Insert Chamber";

        /// <summary>
        /// Speed of the animation of inserting a bullet in the chamber.
        /// </summary>
        [SerializeField]
        [Tooltip("Speed of the animation of inserting a bullet in the chamber.")]
        private float insertInChamberSpeed = 1;

        /// <summary>
        /// Sound of the animation of inserting a bullet in the chamber.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound of the animation of inserting a bullet in the chamber.")]
        private AudioClip insertInChamberSound;

        /// <summary>
        /// Volume of the sound of inserting a bullet in the chamber.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Volume of the sound of inserting a bullet in the chamber.")]
        private float insertInChamberVolume = 0.25f;

        /// <summary>
        /// Bullet per Bullet reloading animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Bullet per Bullet reloading animation.")]
        private string insertAnimation = "Insert Reload";

        /// <summary>
        /// Bullet per Bullet reloading animation speed.
        /// </summary>
        [SerializeField]
        [Tooltip("Bullet per Bullet reloading animation speed.")]
        private float insertSpeed = 1;

        /// <summary>
        /// Insert bullet in chamber sound.
        /// </summary>
        [SerializeField]
        [Tooltip("Insert bullet in chamber sound.")]
        private AudioClip insertSound;

        /// <summary>
        /// Insert bullet in chamber volume.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Insert bullet in chamber volume.")]
        private float insertVolume = 0.25f;

        /// <summary>
        /// Reload finalization animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Reload finalization animation.")]
        private string stopReloadAnimation = "Stop Reload";

        /// <summary>
        /// Reload finalization speed.
        /// </summary>
        [SerializeField]
        [Tooltip("Reload finalization speed.")]
        private float stopReloadSpeed = 1;

        /// <summary>
        /// Reload finalization sound.
        /// </summary>
        [SerializeField]
        [Tooltip("Reload finalization sound.")]
        private AudioClip stopReloadSound;

        /// <summary>
        /// Reload finalization sound volume.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Reload finalization sound volume.")]
        private float stopReloadVolume = 0.25f;

        /// <summary>
        /// Enables melee animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables melee animation.")]
        private bool melee;

        /// <summary>
        /// Name of the melee animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of the melee animation.")]
        private string meleeAnimation = "Melee";

        /// <summary>
        /// Speed of melee animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Speed of melee animation.")]
        private float meleeSpeed = 1;

        /// <summary>
        /// The time required to send the hit signal to the object the character is fighting with.
        /// </summary>
        [SerializeField]
        [Tooltip("The time required to send the hit signal to the object the character is fighting with.")]
        private float m_MeleeDelay = 0.1f;

        /// <summary>
        /// Sound of melee animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound of melee animation.")]
        private AudioClip m_MeleeSound;

        /// <summary>
        /// Volume of the sound of melee animation.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Volume of the sound of melee animation.")]
        private float m_MeleeVolume = 0.2f;

        /// <summary>
        /// List of hit sounds. (Played when the character hits something with the attacks)
        /// </summary>
        [SerializeField]
        [Tooltip("List of hit sounds. (Played when the character hits something with the attacks)")]
        private List<AudioClip> m_HitSoundList = new List<AudioClip>();

        /// <summary>
        /// The volume of the hit sound.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        private float m_HitVolume = 0.3f;

        /// <summary>
        /// Enables switch shooting mode animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables switch shooting mode animation.")]
        private bool switchMode;

        /// <summary>
        /// Name of the switch shooting mode animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of the switch shooting mode animation.")]
        private string switchModeAnimation = "SwitchMode";

        /// <summary>
        /// Speed of the switch shooting mode animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Speed of the switch shooting mode animation.")]
        private float switchModeSpeed = 1;

        /// <summary>
        /// Sound of the switch shooting mode animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound of the switch shooting mode animation.")]
        private AudioClip switchModeSound;

        /// <summary>
        /// Volume sound of the switch shooting mode animation.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Volume sound of the switch shooting mode animation.")]
        private float switchModeVolume = 0.2f;

        /// <summary>
        /// Enables interaction animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables interaction animation.")]
        private bool interact;

        /// <summary>
        /// Name of the interaction animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of the interaction animation.")]
        private string interactAnimation = "Interact";

        /// <summary>
        /// Speed of the interaction animation.
        /// </summary>
        [SerializeField]
        [Tooltip("Speed of the interaction animation.")]
        private float interactSpeed = 1;

        /// <summary>
        /// The time required to send the activation signal to the object the character is interacting with.
        /// </summary>
        [SerializeField]
        [Tooltip("The time required to send the activation signal to the object the character is interacting with.")]
        private float interactDelay = 0.25f;

        /// <summary>
        /// Sound played when interacting with an object.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound played when interacting with an object.")]
        private AudioClip interactSound;

        /// <summary>
        /// The volume of interaction sound.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of interaction sound.")]
        private float interactVolume = 0.2f;

        /// <summary>
        /// Enables vault animation.
        /// </summary>
        [SerializeField]
        private bool vault;

        /// <summary>
        /// The vault animation name.
        /// </summary>
        [SerializeField]
        [Tooltip("The vault animation name.")]
        private string vaultAnimation = "Vault";

        /// <summary>
        /// The vault animation speed.
        /// </summary>
        [SerializeField]
        [Tooltip("The vault animation speed.")]
        private float vaultSpeed = 1;

        /// <summary>
        /// The sound of vaulting.
        /// </summary>
        [SerializeField]
        [Tooltip("The sound of vaulting.")]
        private AudioClip vaultSound;

        /// <summary>
        /// The volume of the sound of vaulting.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of the sound of vaulting.")]
        private float vaultVolume = 0.2f;

        private Camera _camera;
        private Transform targetTransform;

        private Vector3 targetPos;
        private Quaternion targetRot;

        private Vector3 hIPPosition;
        private Vector3 hipRotation;
        private float returningSpeed;

        private bool hasPlayedAimIn;
        private bool hasPlayedAimOut;

        private AudioEmitter playerBodySource;
        private AudioEmitter playerWeaponSource;
        private AudioEmitter playerWeaponGenericSource;

        #region PROPERTIES

        /// <summary>
        /// Returns true if the animator has already been initialized, false otherwise.
        /// </summary>
        public bool Initialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the weapon is in the aiming position/rotation, false otherwise.
        /// </summary>
        public bool IsAiming
        {
            get
            {
                if (!aimAnimation)
                    return false;

                if ((aimingPosition - hIPPosition).sqrMagnitude < 0.001f && (aimingRotation - hipRotation).sqrMagnitude < 0.001f && zoomAnimation && Mathf.Abs(aimFOV - GameplayManager.Instance.FieldOfView) < 0.001f)
                {
                    return false;
                }

                bool position = (aimingPosition - targetTransform.localPosition).sqrMagnitude < 0.0001f;
                bool rotation = (aimingRotation - targetTransform.localRotation.eulerAngles).sqrMagnitude < 0.001f;
                bool zoom = Mathf.Abs(aimFOV - _camera.fieldOfView) < 0.1f;

                return (position && rotation) || (zoom && Math.Abs(GameplayManager.Instance.FieldOfView - aimFOV) > 0.001f);
            }
        }

        /// <summary>
        /// Returns true if holding the breath is enable for this gun, false otherwise.
        /// </summary>
        public bool CanHoldBreath => holdBreath;

        /// <summary>
        /// Returns true if melee is enable for this gun, false otherwise.
        /// </summary>
        public bool CanMeleeAttack => melee;

        /// <summary>
        /// The duration in seconds of the draw animation.
        /// </summary>
        public float DrawAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!draw)
                    return 0;

                return drawAnimation.Length == 0 ? 0 : animator.GetAnimationClip(drawAnimation).length / drawSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the hiding animation.
        /// </summary>
        public float HideAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!hide)
                    return 0;

                return hideAnimation.Length == 0 ? 0 : animator.GetAnimationClip(hideAnimation).length / hideSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the reload animation.
        /// </summary>
        public float ReloadAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!reload)
                    return 0;

                return reloadAnimation.Length == 0 ? 0 : animator.GetAnimationClip(reloadAnimation).length / reloadSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the full reload animation.
        /// </summary>
        public float FullReloadAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!reload)
                    return 0;

                return reloadEmptyAnimation.Length == 0 ? 0 : animator.GetAnimationClip(reloadEmptyAnimation).length / reloadEmptySpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the start reload animation.
        /// </summary>
        public float StartReloadAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!reload)
                    return 0;

                return startReloadAnimation.Length == 0 ? 0 : animator.GetAnimationClip(startReloadAnimation).length / startReloadSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the insert in chamber animation.
        /// </summary>
        public float InsertInChamberAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!reload)
                    return 0;

                return insertInChamberAnimation.Length == 0 ? 0 : animator.GetAnimationClip(insertInChamberAnimation).length / insertInChamberSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the inserting bullets animation.
        /// </summary>
        public float InsertAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!reload)
                    return 0;

                return insertAnimation.Length == 0 ? 0 : animator.GetAnimationClip(insertAnimation).length / insertSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the finishing reload animation.
        /// </summary>
        public float StopReloadAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!reload)
                    return 0;

                return stopReloadAnimation.Length == 0 ? 0 : animator.GetAnimationClip(stopReloadAnimation).length / stopReloadSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the melee animation.
        /// </summary>
        public float MeleeAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!melee)
                    return 0;

                return meleeAnimation.Length == 0 ? 0 : animator.GetAnimationClip(meleeAnimation).length / meleeSpeed;
            }
        }

        /// <summary>
        /// The time required to send the hit signal to the object the character is fighting with.
        /// </summary>
        public float MeleeDelay
        {
            get
            {
                if (!melee)
                    return 0;

                return m_MeleeDelay > MeleeAnimationLength ? MeleeAnimationLength : m_MeleeDelay;
            }
        }

        /// <summary>
        /// The duration in seconds of the switch shooting mode animation.
        /// </summary>
        public float SwitchModeAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!switchMode)
                    return 0;

                return switchModeAnimation.Length == 0 ? 0 : animator.GetAnimationClip(switchModeAnimation).length / switchModeSpeed;
            }
        }

        /// <summary>
        /// The duration in seconds of the interact animation.
        /// </summary>
        public float InteractAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!interact)
                    return 0;

                return interactAnimation.Length == 0 ? 0 : animator.GetAnimationClip(interactAnimation).length / interactSpeed;
            }
        }

        /// <summary>
        /// The time required to send the activation signal to the object the character is interacting with.
        /// </summary>
        public float InteractDelay
        {
            get
            {
                if (!interact)
                    return 0;

                return interactDelay;
            }
        }

        #endregion

        #region WEAPON CUSTOMIZATION

        internal void UpdateAiming(Vector3 aimingPosition, Vector3 aimingRotation, bool zoomAnimation = false, float aimFOV = 50)
        {
            this.aimingPosition = aimingPosition;
            this.aimingRotation = aimingRotation;
            this.zoomAnimation = zoomAnimation;
            this.aimFOV = aimFOV;
        }

        internal void UpdateFireSound(AudioClip[] fireSoundList)
        {
            this.fireSoundList.Clear();
            this.fireSoundList.AddRange(fireSoundList);
        }

        #endregion

        /// <summary>
        /// Initializes the component.
        /// </summary>
        /// <param name="targetTransform">The weapon's transform.</param>
        /// <param name="camera">The character's camera.</param>
        public void Init(Transform targetTransform, Camera camera)
        {
            _camera = camera;
            this.targetTransform = targetTransform;

            if (speedParameterHash == 0)
                speedParameterHash = Animator.StringToHash(speedParameter);

            // Audio Sources
            playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", this.targetTransform.transform.root, spatialBlend: 0);
            playerWeaponGenericSource = AudioManager.Instance.RegisterSource("[AudioEmitter] WeaponGeneric", this.targetTransform.transform.parent, spatialBlend: 0);

            // ReSharper disable once Unity.InefficientPropertyAccess
            playerWeaponSource = AudioManager.Instance.RegisterSource("[AudioEmitter] Weapon", this.targetTransform.transform.parent, AudioCategory.SFx, 10, 25, 0);

            Initialized = true;
        }

        public void SetCrouchStatus(bool isCrouched)
        {
            if (isCrouched)
            {
                hIPPosition = crouchPosition;
                hipRotation = crouchRotation;
                returningSpeed = crouchingSpeed;
            }
            else
            {
                hIPPosition = standingPosition;
                hipRotation = standingRotation;
                returningSpeed = runningSpeed;
            }
        }

        /// <summary>
        /// Calculates the aiming position/rotation or return to the origin position.
        /// </summary>
        /// <param name="isAiming">Is the character aiming?</param>
        public void Aim(bool isAiming)
        {
            if (isAiming && aimAnimation)
            {
                if (!IsAiming && !hasPlayedAimIn)
                {
                    hasPlayedAimIn = true;
                    hasPlayedAimOut = false;
                    playerBodySource.Play(aimInSound, 0.1f);
                }

                if (aimAnimation)
                {
                    targetPos = Vector3.Lerp(targetPos, aimingPosition, Time.deltaTime * aimingSpeed);
                    targetRot = Quaternion.Slerp(targetRot, Quaternion.Euler(aimingRotation), Time.deltaTime * aimingSpeed);

                    if (zoomAnimation)
                        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, aimFOV, Time.deltaTime * aimingSpeed);
                }
            }
            else //Stop Sprint Animation
            {
                targetPos = Vector3.Lerp(targetPos, hIPPosition, Time.deltaTime * returningSpeed);
                targetRot = Quaternion.Slerp(targetRot, Quaternion.Euler(hipRotation), Time.deltaTime * returningSpeed);
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * aimingSpeed);
            }

            PerformAnimation();
        }

        /// <summary>
        /// Calculates the target position/rotation based on the character state or return to the origin position.
        /// </summary>
        /// <param name="isRunning">Is the character running?</param>
        /// <param name="isSliding">Is the character sliding?</param>
        public void Sprint(bool isRunning, bool isSliding)
        {
            if (runAnimation && isRunning)
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * aimingSpeed);
                targetPos = Vector3.Lerp(targetPos, runningPosition, Time.deltaTime * runningSpeed);
                targetRot = Quaternion.Slerp(targetRot, Quaternion.Euler(runningRotation), Time.deltaTime * runningSpeed);
            }
            else if (slidingAnimation && isSliding)
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * aimingSpeed);
                targetPos = Vector3.Lerp(targetPos, slidingPosition, Time.deltaTime * slidingSpeed);
                targetRot = Quaternion.Slerp(targetRot, Quaternion.Euler(slidingRotation), Time.deltaTime * slidingSpeed);
            }
            else //Stop Aiming Animation
            {
                targetPos = Vector3.Lerp(targetPos, hIPPosition, Time.deltaTime * returningSpeed);
                targetRot = Quaternion.Slerp(targetRot, Quaternion.Euler(hipRotation), Time.deltaTime * returningSpeed);
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * aimingSpeed);

                if (IsAiming && aimAnimation && !hasPlayedAimOut)
                {
                    hasPlayedAimOut = true;
                    hasPlayedAimIn = false;
                    playerBodySource.Play(aimOutSound, 0.1f);
                }

            }

            PerformAnimation();
        }

        /// <summary>
        /// Defines the position of the gun as the position/rotation calculated in the previous frame.
        /// </summary>
        private void PerformAnimation()
        {
            targetTransform.localPosition = targetPos;
            targetTransform.localRotation = targetRot;
        }

        /// <summary>
        /// Plays the draw animation.
        /// </summary>
        public void Draw()
        {
            if (!animator)
                return;

            if (!draw)
                return;

            if (drawAnimation.Length == 0)
                return;

            if (speedParameterHash == 0)
                speedParameterHash = Animator.StringToHash(speedParameter);

            animator.SetFloat(speedParameterHash, drawSpeed);

            animator.Play(drawAnimation);

            if (playerBodySource == null)
                playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", targetTransform.transform.root, spatialBlend: 0);

            if (drawSound)
                playerBodySource.ForcePlay(drawSound, drawVolume);
        }

        /// <summary>
        /// Plays the hiding animation.
        /// </summary>
        public void Hide()
        {
            if (!animator)
                return;

            if (!hide)
                return;

            if (hideAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, hideSpeed);

            animator.CrossFadeInFixedTime(hideAnimation, 0.1f);
            playerWeaponSource.Stop();
            playerWeaponGenericSource.Stop();

            if (hideSound)
                playerBodySource.ForcePlay(hideSound, hideVolume);
        }

        /// <summary>
        /// Plays the shooting animation.
        /// </summary>
        /// <param name="lastRound">Is the last round in the magazine?</param>
        internal void Shot(bool lastRound)
        {
            if (!animator)
                return;

            if (!fire)
                return;

            if (overrideLastFire && lastRound)
            {
                animator.SetFloat(speedParameterHash, fireSpeed);

                if (lastFireAnimation.Length > 0)
                    animator.CrossFadeInFixedTime(lastFireAnimation, 0.1f);
            }
            else
            {
                string currentAnim;

                if (IsAiming)
                {
                    if (aimedFireAnimationList.Count > 0)
                    {
                        animator.SetFloat(speedParameterHash, aimedFireSpeed);

                        switch (fireAnimationType)
                        {
                            case AnimationType.Sequential:
                                {
                                    if (lastIndex == aimedFireAnimationList.Count || lastIndex > aimedFireAnimationList.Count)
                                        lastIndex = 0;

                                    currentAnim = aimedFireAnimationList[lastIndex++];

                                    if (currentAnim.Length > 0)
                                        animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                                    break;
                                }
                            case AnimationType.Random:

                                currentAnim = aimedFireAnimationList[Random.Range(0, aimedFireAnimationList.Count)];

                                if (currentAnim.Length > 0)
                                    animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                else
                {
                    if (fireAnimationList.Count > 0)
                    {
                        animator.SetFloat(speedParameterHash, fireSpeed);

                        switch (fireAnimationType)
                        {
                            case AnimationType.Sequential:
                                {
                                    if (lastIndex == fireAnimationList.Count || lastIndex > fireAnimationList.Count)
                                        lastIndex = 0;

                                    currentAnim = fireAnimationList[lastIndex++];

                                    if (currentAnim.Length > 0)
                                        animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                                    break;
                                }
                            case AnimationType.Random:

                                currentAnim = fireAnimationList[Random.Range(0, fireAnimationList.Count)];

                                if (currentAnim.Length > 0)
                                    animator.CrossFadeInFixedTime(currentAnim, 0.1f);

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }

            if (fireSoundList.Count > 0)
            {
                if (fireSoundList.Count > 1)
                {
                    int i = Random.Range(1, fireSoundList.Count);
                    AudioClip a = fireSoundList[i];

                    fireSoundList[i] = fireSoundList[0];
                    fireSoundList[0] = a;

                    if (additiveSound)
                        AudioManager.Instance.PlayClipAtPoint(a, _camera.transform.position, 10, 25, fireVolume, 0);
                    else
                        playerWeaponSource.ForcePlay(a, fireVolume);

                }
                else
                {
                    if (additiveSound)
                        AudioManager.Instance.PlayClipAtPoint(fireSoundList[0], _camera.transform.position, 10, 25, fireVolume, 0);
                    else
                        playerWeaponSource.ForcePlay(fireSoundList[0], fireVolume);
                }
            }
        }

        /// <summary>
        /// Plays the dry fire sound.
        /// </summary>
        internal void OutOfAmmo()
        {
            if (outOfAmmoSound)
                playerBodySource.ForcePlay(outOfAmmoSound, fireVolume);
        }

        /// <summary>
        /// Plays the reload magazine animation.
        /// </summary>
        /// <param name="roundInChamber">Is there a bullet in the chamber?</param>
        internal void Reload(bool roundInChamber)
        {
            if (!animator)
                return;

            if (!reload)
                return;

            if (!roundInChamber)
            {
                if (reloadEmptyAnimation.Length == 0)
                    return;

                animator.SetFloat(speedParameterHash, reloadEmptySpeed);

                animator.CrossFadeInFixedTime(reloadEmptyAnimation, 0.1f);

                if (reloadEmptySound)
                    playerWeaponGenericSource.ForcePlay(reloadEmptySound, reloadVolume);
            }
            else
            {
                if (reloadAnimation.Length == 0)
                    return;

                animator.SetFloat(speedParameterHash, reloadSpeed);

                animator.CrossFadeInFixedTime(reloadAnimation, 0.1f);

                if (reloadSound)
                    playerWeaponGenericSource.ForcePlay(reloadSound, reloadVolume);
            }
        }

        /// <summary>
        /// Plays the start reload animation.
        /// </summary>
        /// <param name="roundInChamber">Is there a bullet in the chamber?</param>
        internal void StartReload(bool roundInChamber)
        {
            if (!animator)
                return;

            if (!reload)
                return;

            if (!roundInChamber)
            {
                if (insertInChamberAnimation.Length == 0)
                    return;

                animator.SetFloat(speedParameterHash, insertInChamberSpeed);

                animator.CrossFadeInFixedTime(insertInChamberAnimation, 0.1f);
                playerWeaponSource.Stop();

                if (insertInChamberSound)
                    playerWeaponGenericSource.ForcePlay(insertInChamberSound, insertInChamberVolume);
            }
            else
            {
                if (startReloadAnimation.Length == 0)
                    return;

                animator.SetFloat(speedParameterHash, startReloadSpeed);

                animator.CrossFadeInFixedTime(startReloadAnimation, 0.1f);
                playerWeaponSource.Stop();

                if (startReloadSound)
                    playerWeaponGenericSource.ForcePlay(startReloadSound, startReloadVolume);
            }
        }

        /// <summary>
        /// Plays the insert bullet in chamber animation.
        /// </summary>
        internal void Insert()
        {
            if (!animator)
                return;

            if (!reload)
                return;

            if (insertAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, insertSpeed);

            animator.CrossFadeInFixedTime(insertAnimation, 0.1f);

            if (insertSound)
                playerWeaponGenericSource.ForcePlay(insertSound, insertVolume);
        }

        /// <summary>
        /// Finalizes the reload animation.
        /// </summary>
        internal void StopReload()
        {
            if (!animator)
                return;

            if (!reload)
                return;

            if (stopReloadAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, stopReloadSpeed);

            animator.CrossFadeInFixedTime(stopReloadAnimation, 0.1f);

            if (stopReloadSound)
                playerWeaponGenericSource.ForcePlay(stopReloadSound, stopReloadVolume);
        }

        /// <summary>
        /// Plays a melee attack animation.
        /// </summary>
        internal void Melee()
        {
            if (!animator)
                return;

            if (!melee)
                return;

            if (meleeAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, meleeSpeed);

            animator.CrossFadeInFixedTime(meleeAnimation, 0.1f);

            if (m_MeleeSound)
                playerBodySource.ForcePlay(m_MeleeSound, m_MeleeVolume);
        }

        /// <summary>
        /// Plays an impact sound when hitting a target.
        /// </summary>
        /// <param name="position">Hit position.</param>
        public void Hit(Vector3 position)
        {
            if (m_HitSoundList.Count > 0)
                AudioManager.Instance.PlayClipAtPoint(m_HitSoundList[Random.Range(0, m_HitSoundList.Count)], position, 3, 10, m_HitVolume);
        }

        /// <summary>
        /// Plays the switch fire mode animation.
        /// </summary>
        internal void SwitchMode()
        {
            if (!animator)
                return;

            if (!switchMode)
                return;

            if (switchModeAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, switchModeSpeed);

            animator.CrossFadeInFixedTime(switchModeAnimation, 0.1f);

            if (switchModeSound)
                playerBodySource.ForcePlay(switchModeSound, switchModeVolume);
        }

        /// <summary>
        /// Plays the interact animation.
        /// </summary>
        public void Interact()
        {
            if (!animator)
                return;

            if (!interact)
                return;

            if (interactAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, interactSpeed);

            animator.CrossFadeInFixedTime(interactAnimation, 0.1f);

            if (interactSound)
                playerBodySource.ForcePlay(interactSound, interactVolume);
        }

        /// <summary>
        /// Plays the vaulting animation.
        /// </summary>
        public void Vault()
        {
            if (!animator)
                return;

            if (!vault)
                return;

            if (vaultAnimation.Length == 0)
                return;

            animator.SetFloat(speedParameterHash, vaultSpeed);

            animator.CrossFadeInFixedTime(vaultAnimation, 0.1f);

            if (vaultSound)
                playerBodySource.ForcePlay(vaultSound, vaultVolume);
        }
    }
}
