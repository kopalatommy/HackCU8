using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Animation;
using UnnaturalSelection.Audio;
using UnnaturalSelection.Character;

namespace UnnaturalSelection.Weapons
{
    [DisallowMultipleComponent]
    public class FirstAidKit : Equipment
    {
        [SerializeField]
        [Tooltip("The character HealthController reference.")]
        private HealthController healthController;

        [SerializeField]
        [Tooltip("Defines how much vitality will be restored per use.")]
        private float healAmount = 100;

        /// <summary>
        /// Defines the delay in seconds to apply the healing effect.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the delay in seconds to apply the healing effect.")]
        private float delayToHeal = 1.3f;

        [SerializeField]
        [Tooltip("Allow the character to receive an additional stamina bonus after being healed.")]
        private bool staminaBonus;

        [SerializeField]
        [Tooltip("Defines how long the effect will last.")]
        private float staminaBonusDuration = 10;

        [SerializeField]
        [Tooltip("Should the character injuries be healed?")]
        private bool healInjuries;

        [SerializeField]
        [Tooltip("Defines the amount of syringes the character will start.")]
        private int amount = 3;

        [SerializeField]
        [Tooltip("Allow the character to use unlimited shots.")]
        private bool infiniteShots;

        [SerializeField]
        [Tooltip("Defines the maximum number of syringes the character can carry.")]
        private int maxAmount = 3;

        [SerializeField]
        [Tooltip("The animator reference.")]
        private Animator animator;

        [SerializeField]
        [Tooltip("The name of the animation that will be played when the character is healing.")]
        private string healAnimation = "Heal";

        [SerializeField]
        [Tooltip("The audio that will be played when the character is using the first aid kit.")]
        private AudioClip healSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the sound volume when the character is using the first aid kit.")]
        private float healVolume = 0.3f;

        private bool isInitialized;

        private WaitForSeconds healDuration;
        private AudioEmitter playerBodySource;

        #region PROPERTIES

        /// <summary>
        /// The duration in seconds of the animation of using the first aid kit.
        /// </summary>
        public override float UsageDuration
        {
            get
            {
                if (!animator)
                    return 0;

                if (this.healAnimation.Length == 0)
                    return 0;

                AnimationClip healAnimation = animator.GetAnimationClip(this.healAnimation);
                return healAnimation.length > delayToHeal ? healAnimation.length : delayToHeal;
            }
        }

        /// <summary>
        /// The current amount of syringes the character has.
        /// </summary>
        public int Amount => infiniteShots ? 99 : amount;

        /// <summary>
        /// Can the character carry more adrenaline?
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

            AnimationClip healAnimation = animator.GetAnimationClip(this.healAnimation);
            if (healAnimation)
                healDuration = new WaitForSeconds(healAnimation.length > delayToHeal ? healAnimation.length - delayToHeal : delayToHeal);
            else
            {
                Debug.LogError("Heal animation not found!");
            }

            DisableShadowCasting();

            isInitialized = true;
        }

        /// <summary>
        /// Uses a unit of the item and apply the effects to the character.
        /// </summary>
        public override void Use()
        {
            if (animator)
                StartCoroutine(AdrenalineShot());
        }

        /// <summary>
        /// Play the animation, regenerate the character's vitality and apply a temporary speed bonus.
        /// </summary>
        private IEnumerator AdrenalineShot()
        {
            if (healAnimation.Length > 0)
                animator.CrossFadeInFixedTime(healAnimation, 0.1f);

            if (playerBodySource == null)
                playerBodySource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);

            playerBodySource.ForcePlay(healSound, healVolume);

            yield return new WaitForSeconds(delayToHeal);
            healthController.Heal(healAmount, healInjuries, staminaBonus && staminaBonusDuration > 0, staminaBonusDuration);

            if (!infiniteShots)
                amount--;

            yield return healDuration;
        }

        /// <summary>
        /// Refill the adrenaline's syringes.
        /// </summary>
        public override void Refill()
        {
            amount = maxAmount;
        }
    }
}
