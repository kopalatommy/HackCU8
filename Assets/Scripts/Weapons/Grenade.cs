using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Animation;
using UnnaturalSelection.Audio;

namespace UnnaturalSelection.Weapons
{
    [DisallowMultipleComponent]
    public class Grenade : Equipment
    {
        [SerializeField]
        [Tooltip("The grenade explosive prefab.")]
        private Rigidbody grenade;

        /// <summary>
        /// The Transform reference used to know where the grenade will be instantiated from.
        /// </summary>
        [SerializeField]
        [Tooltip("The Transform reference used to know where the grenade will be instantiated from.")]
        private Transform throwTransformReference;

        /// <summary>
        /// Defines the force that the character will throw the grenade.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the force that the character will throw the grenade.")]
        private float throwForce = 20;

        /// <summary>
        /// Defines the amount of grenades the character will start.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the amount of grenades the character will start.")]
        private int amount = 3;

        /// <summary>
        /// Allow the character to use unlimited grenades.
        /// </summary>
        [SerializeField]
        [Tooltip("Allow the character to use unlimited grenades.")]
        private bool infiniteGrenades;

        /// <summary>
        /// Defines the maximum number of grenades the character can carry.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the maximum number of grenades the character can carry.")]
        private int maxAmount = 3;

        /// <summary>
        /// Defines the delay in seconds for the character throw the grenade.
        /// (For some grenades it is necessary to remove the protection pin before throwing, use this field to adjust the necessary time for such action.)
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the delay in seconds for the character throw the grenade. " +
                 "(For some grenades it is necessary to remove the protection pin before throwing, use this field to adjust the necessary time for such action.)")]
        private float delayToInstantiate = 0.14f;

        /// <summary>
        /// The animator reference.
        /// </summary>
        [SerializeField]
        [Tooltip("The animator reference.")]
        private Animator animator;

        /// <summary>
        /// Animation of pulling the grenade pin.
        /// </summary>
        [SerializeField]
        [Tooltip("Animation of pulling the grenade pin.")]
        private string pullAnimation;

        /// <summary>
        /// Sound of pulling the grenade pin.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound of pulling the grenade pin.")]
        private AudioClip pullSound;

        /// <summary>
        /// Defines the volume of the grenade pin pulling sound.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the volume of the grenade pin pulling sound.")]
        private float pullVolume = 0.2f;

        /// <summary>
        /// Animation of throwing the grenade.
        /// </summary>
        [SerializeField]
        [Tooltip("Animation of throwing the grenade.")]
        private string throwAnimation;

        /// <summary>
        /// The sound of throwing the grenade.
        /// </summary>
        [SerializeField]
        [Tooltip("The sound of throwing the grenade.")]
        private AudioClip throwSound;

        /// <summary>
        /// The volume of the grenade throwing sound.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of the grenade throwing sound.")]
        private float throwVolume = 0.2f;

        private bool isInitialized;

        private WaitForSeconds pullDuration;
        private WaitForSeconds instantiateDelay;
        private AudioEmitter playerBodySource;

        #region PROPERTIES

        /// <summary>
        /// The duration in seconds of the animations of pulling a grenade pin.
        /// </summary>
        public override float UsageDuration
        {
            get
            {
                if (!animator)
                    return 0;

                float duration = 0;

                if (pullAnimation.Length > 0)
                {
                    duration += animator.GetAnimationClip(pullAnimation).length;
                }

                if (throwAnimation.Length > 0)
                {
                    duration += Mathf.Max(animator.GetAnimationClip(throwAnimation).length, delayToInstantiate);
                }

                return duration;
            }
        }

        /// <summary>
        /// The current amount of grenades the character has.
        /// </summary>
        public int Amount => infiniteGrenades ? 99 : amount;

        /// <summary>
        /// Can the character carry more grenades?
        /// </summary>
        public bool CanRefill => amount < maxAmount;

        #endregion

        /// <summary>
        /// Initializes the object.
        /// </summary>
        public override void Init()
        {
            if (isInitialized)
                return;

            pullDuration = pullAnimation.Length > 0 ? new WaitForSeconds(animator.GetAnimationClip(pullAnimation).length) : new WaitForSeconds(0);
            instantiateDelay = new WaitForSeconds(delayToInstantiate);

            playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);

            DisableShadowCasting();
        }

        /// <summary>
        /// Uses a unit of the item and instantiates a grenade.
        /// </summary>
        public override void Use()
        {
            if (grenade && throwTransformReference && animator)
                StartCoroutine(ThrowGrenade());
        }

        /// <summary>
        /// Play the animation and instantiates a grenade.
        /// </summary>
        private IEnumerator ThrowGrenade()
        {
            if (pullAnimation.Length > 0)
            {
                animator.CrossFadeInFixedTime(pullAnimation, 0.1f);
                playerBodySource.ForcePlay(pullSound, pullVolume);
                yield return pullDuration;
            }

            if (throwAnimation.Length > 0)
            {
                animator.CrossFadeInFixedTime(throwAnimation, 0.1f);
                playerBodySource.ForcePlay(throwSound, throwVolume);
                yield return instantiateDelay;
            }

            InstantiateGrenade();
        }

        /// <summary>
        /// Throw a grenade using the parameters.
        /// </summary>
        private void InstantiateGrenade()
        {
            if (!grenade)
                return;

            if (!infiniteGrenades)
                amount--;

            Rigidbody clone = Instantiate(grenade, throwTransformReference.position, throwTransformReference.rotation);
            clone.velocity = clone.transform.TransformDirection(Vector3.forward) * throwForce;
        }

        /// <summary>
        /// Refill the grenades.
        /// </summary>
        public override void Refill()
        {
            amount = maxAmount;
        }
    }
}
