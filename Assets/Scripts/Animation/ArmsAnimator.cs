using System;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Audio;
using UnnaturalSelection.Character;
using Random = UnityEngine.Random;

namespace UnnaturalSelection.Animation
{
    public enum AnimationType
    {
        Sequential,
        Random
    }
    public class ArmsAnimator : IWeaponAnimator
    {
        [SerializeField]
        [Tooltip("Animator's reference.")]
        private Animator animator;

        [SerializeField]
        [Tooltip("The name of the animator parameter that defines the playing speed.")]
        private string velocityParameter = "Velocity";

        private int velocityParameterHash;

        [SerializeField]
        [Tooltip("Enables Draw animation.")]
        private bool draw;

        [SerializeField]
        [Tooltip("The name of the draw animation.")]
        private string drawAnimation = "Draw";

        [SerializeField]
        [Tooltip("The sound played when drawing the weapon.")]
        private AudioClip drawSound;

        [SerializeField, Range(0, 1)]
        [Tooltip("The volume of the sound of drawing the weapon.")]
        private float drawVolume = 0.25f;

        [SerializeField]
        [Tooltip("Enables the animation of hiding the weapon.")]
        private bool hide;

        [SerializeField]
        [Tooltip("The name of the hide the weapon animation.")]
        private string hideAnimation = "Hide";

        [SerializeField]
        private AudioClip hideSound;

        [SerializeField, Range(0, 1)]
        [Tooltip("The volume of the sound of hiding the weapon.")]
        private float hideVolume = 0.25f;

        [SerializeField]
        [Tooltip("Enables attack animations.")]
        private bool attack;

        [SerializeField]
        [Tooltip("List of right-handed attack animations.")]
        private List<string> rightAttackAnimationList = new List<string>();

        [SerializeField]
        [Tooltip("List of left-handed attack animations.")]
        private List<string> leftAttackAnimationList = new List<string>();

        [SerializeField]
        [Tooltip("Defines the order of execution of the animations.")]
        private AnimationType attackAnimationType = AnimationType.Sequential;

        [SerializeField]
        [Tooltip("The list of attack sounds.")]
        private List<AudioClip> attackSoundList = new List<AudioClip>();

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of the attack sound.")]
        private float attackVolume = 0.5f;

        [SerializeField]
        [Tooltip("List of hit sounds. (Played when the character hits something with the attacks)")]
        private List<AudioClip> hitSoundList = new List<AudioClip>();

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of the hit sound.")]
        private float hitVolume = 0.3f;

        private int lastIndex;

        [SerializeField]
        [Tooltip("Enables interaction animation.")]
        private bool interact;

        [SerializeField]
        [Tooltip("Name of the interaction animation.")]
        private string interactAnimation = "Interact";

        [SerializeField]
        [Tooltip("The time required to send the activation signal to the object the character is interacting with.")]
        private float interactDelay = 0.1f;

        [SerializeField]
        [Tooltip("Sound played when interacting with an object.")]
        private AudioClip interactSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of interaction sound.")]
        private float interactVolume = 0.2f;

        [SerializeField]
        [Tooltip("Enables vault animation.")]
        private bool vault;

        [SerializeField]
        [Tooltip("The vault animation name.")]
        private string vaultAnimation = "Vault";

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

        private float m_Velocity;
        private MovementController fPController;
        private AudioEmitter playerBodySource;

        public bool Initialized
        {
            get;
            private set;
        }

        public float DrawAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!draw)
                    return 0;

                if (drawAnimation.Length == 0)
                    return 0;

                return animator.GetAnimationClip(drawAnimation).length;
            }
        }

        public float HideAnimationLength
        {
            get
            {
                if (!animator)
                    return 0;

                if (!hide)
                    return 0;

                if (hideAnimation.Length == 0)
                    return 0;

                return animator.GetAnimationClip(hideAnimation).length;
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

                if (interactAnimation.Length == 0)
                    return 0;

                return interactAnimation.Length == 0 ? 0 : animator.GetAnimationClip(interactAnimation).length;

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

        /// <summary>
        /// Initializes the component.
        /// </summary>
        internal void Init(MovementController FPController)
        {
            fPController = FPController;
            velocityParameterHash = Animator.StringToHash(velocityParameter);

            // Audio Source
            playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", fPController.transform.root, spatialBlend: 0);
            Initialized = true;
        }

        /// <summary>
        /// Defines the animations execution speed.
        /// </summary>
        /// <param name="running">Is the character running?</param>
        internal void SetSpeed(bool running)
        {
            m_Velocity = Mathf.MoveTowards(m_Velocity, fPController.IsSliding ? 0 : running ? 1 : 0, Time.deltaTime * 5);
            animator.SetFloat(velocityParameterHash, m_Velocity);

            // Updates animator speed to smoothly change between walking and running animations
            animator.speed = Mathf.Max(fPController.CurrentTargetForce / (fPController.State == MotionState.Running ? 10 : fPController.CurrentTargetForce), 0.8f);
        }

        /// <summary>
        /// Executes an attack using the left arm.
        /// </summary>
        internal void LeftAttack()
        {
            if (!animator)
                return;

            if (!attack)
                return;

            // Normalizes the playing speed.
            animator.speed = 1;

            // Choose an animation from the list according to the defined selection method.
            if (leftAttackAnimationList.Count > 0)
            {
                switch (attackAnimationType)
                {
                    case AnimationType.Sequential:
                        {
                            if (lastIndex == leftAttackAnimationList.Count || lastIndex > leftAttackAnimationList.Count)
                                lastIndex = 0;

                            string currentAnim = leftAttackAnimationList[lastIndex++];

                            if (currentAnim.Length > 0)
                                animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                            break;
                        }
                    case AnimationType.Random:
                        {
                            string currentAnim = leftAttackAnimationList[Random.Range(0, leftAttackAnimationList.Count)];

                            if (currentAnim.Length > 0)
                                animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Attack sound.
            if (attackSoundList.Count > 0)
            {
                if (attackSoundList.Count > 1)
                {
                    int i = Random.Range(1, attackSoundList.Count);
                    AudioClip a = attackSoundList[i];

                    attackSoundList[i] = attackSoundList[0];
                    attackSoundList[0] = a;

                    playerBodySource.ForcePlay(a, attackVolume);
                }
                else
                {
                    playerBodySource.ForcePlay(attackSoundList[0], attackVolume);
                }
            }
        }

        /// <summary>
        /// Executes an attack using the right arm.
        /// </summary>
        internal void RightAttack()
        {
            if (!animator)
                return;

            if (!attack)
                return;

            // Normalizes the playing speed.
            animator.speed = 1;

            // Choose an animation from the list according to the defined selection method.
            if (rightAttackAnimationList.Count > 0)
            {
                switch (attackAnimationType)
                {
                    case AnimationType.Sequential:
                        {
                            if (lastIndex == rightAttackAnimationList.Count || lastIndex > rightAttackAnimationList.Count)
                                lastIndex = 0;

                            string currentAnim = rightAttackAnimationList[lastIndex++];

                            if (currentAnim.Length > 0)
                                animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                            break;
                        }
                    case AnimationType.Random:
                        {
                            string currentAnim = rightAttackAnimationList[Random.Range(0, rightAttackAnimationList.Count)];

                            if (currentAnim.Length > 0)
                                animator.CrossFadeInFixedTime(currentAnim, 0.1f);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Attack sound.
            if (attackSoundList.Count > 0)
            {
                if (attackSoundList.Count > 1)
                {
                    int i = Random.Range(1, attackSoundList.Count);
                    AudioClip a = attackSoundList[i];

                    attackSoundList[i] = attackSoundList[0];
                    attackSoundList[0] = a;

                    playerBodySource.ForcePlay(a, attackVolume);
                }
                else
                {
                    playerBodySource.ForcePlay(attackSoundList[0], attackVolume);
                }
            }
        }

        /// <summary>
        /// Plays an impact sound when hitting a target.
        /// </summary>
        /// <param name="position">Hit position.</param>
        public void Hit(Vector3 position)
        {
            if (hitSoundList.Count > 0)
                AudioManager.Instance.PlayClipAtPoint(hitSoundList[Random.Range(0, hitSoundList.Count)], position, 3, 10, hitVolume);
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

            // Normalizes the playing speed.
            animator.speed = 1;

            animator.Play(drawAnimation);
            playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", fPController.transform.root, spatialBlend: 0);
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

            // Normalizes the playing speed.
            animator.speed = 1;

            animator.CrossFadeInFixedTime(hideAnimation, 0.1f);
            playerBodySource.Stop();
            playerBodySource.ForcePlay(hideSound, hideVolume);
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

            // Normalizes the playing speed.
            animator.speed = 1;

            animator.CrossFadeInFixedTime(interactAnimation, 0.1f);
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

            // Normalizes the playing speed.
            animator.speed = 1;

            animator.CrossFadeInFixedTime(vaultAnimation, 0.1f);
            playerBodySource.ForcePlay(vaultSound, vaultVolume);
        }
    }
}
