using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Character
{
    public enum BodyPart
    {
        FullBody,
        Head,
        Chest,
        Arm,
        Stomach,
        Leg
    }

    public enum TraumaType
    {
        None,
        Limp,
        Tremor,
    }

    [DisallowMultipleComponent]
    public class DamageHandler : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Defines which body part of the character this script represents.")]
        private BodyPart bodyPart = BodyPart.FullBody;

        [SerializeField]
        [Tooltip("Defines whether the character can survive if vitality is equal to 0.")]
        private bool vital = true;

        [SerializeField]
        [Tooltip("Defines how many vitality units this body part has.")]
        private float vitality = 100;

        [SerializeField]
        [Tooltip("Allows the character to get injured, compromising basic functions such as running, jumping or aiming down sights.")]
        private bool allowInjury;

        [SerializeField, Range(0, 100)]
        [Tooltip("Defines the minimum damage required to make the character bleed.")]
        private float bleedThreshold = 35;

        [SerializeField, Range(0, 1)]
        [Tooltip("Bleed Chance is the probability to start losing blood (if the character suffer damage above the threshold) after an arbitrary damage. If a body part is bleeding, the whole structure will be affected, leading the character to death.")]
        private float bleedChance = 0.5f;

        [SerializeField]
        [Tooltip("Defines the threshold between injured and healthy.")]
        private float damageThreshold = 50;

        [SerializeField]
        [Tooltip("Defines the trauma due the injury to the body part.")]
        private TraumaType traumaType = TraumaType.None;

        [SerializeField]
        [Tooltip("Defines how much Vitality will be drain per second.")]
        private float bleedRate = 0.5f;

        [SerializeField]
        [Tooltip("Enables regeneration functionality for this body part.")]
        private bool allowRegeneration;

        [SerializeField]
        [Tooltip("Defines how the delay to start regenerating its vitality, in seconds.")]
        private float startDelay = 5;

        [SerializeField]
        [Tooltip("Defines how many vitality units will regenerate per second.")]
        private float regenerationRate = 7.5f;

        private float nextRegenTime;
        private bool healing;

        private HealthController healthController;

        public float Vitality => vitality;

        public float CurrentVitality
        {
            get;
            private set;
        }

        public bool CanRegenerate => allowRegeneration;

        public bool IsAlive => CurrentVitality > 0;

        public bool Injured
        {
            get;
            private set;
        }

        public bool Bleeding
        {
            get;
            set;
        }

        public bool Vital => vital;

        public BodyPart BodyPart => bodyPart;

        public TraumaType TraumaType => traumaType;

        public void Init(HealthController healthController)
        {
            CurrentVitality = vitality;
            this.healthController = healthController;
        }

        private void Update()
        {
            if (allowRegeneration && CurrentVitality > 0 && CurrentVitality < vitality && nextRegenTime < Time.time)
            {
                CurrentVitality = Mathf.MoveTowards(CurrentVitality, vitality, Time.deltaTime * regenerationRate);
            }

            if (!Bleeding)
                return;

            if (bleedRate > 0)
                CurrentVitality = Mathf.MoveTowards(CurrentVitality, 0, Time.deltaTime * bleedRate);
        }

        private void ApplyDamage(float damage)
        {
            CurrentVitality = Mathf.Max(CurrentVitality - damage, 0);
            nextRegenTime = Time.time + startDelay;
            healing = false;

            if (allowInjury && !Injured && CurrentVitality <= damageThreshold && !allowRegeneration)
                Injured = true;
        }

        public void ApplyBleedEffect(float damage)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) <= bleedChance && bleedThreshold <= damage && !allowRegeneration)
                Bleeding = true;
        }

        public void Damage(float damage)
        {
            if (damage > 0)
                ApplyDamage(damage);
        }

        public void Damage(float damage, Vector3 targetPosition, Vector3 hitPosition)
        {
            if (Math.Abs(damage) < Mathf.Epsilon)
                return;

            ApplyDamage(damage);
            healthController.DamageEffect(targetPosition);
            healthController.GenericDamageResponse();
            healthController.BulletHitEffect();
        }

        public void ProjectileDamage(float damage, Vector3 targetPosition, Vector3 hitPosition, float penetrationPower)
        {
            Damage(damage, targetPosition, hitPosition);
            ApplyBleedEffect(damage);

            healthController.HitDamageResponse();
            healthController.BulletHitEffect();
        }

        public void ExplosionDamage(float damage, Vector3 targetPosition, Vector3 hitPosition)
        {
            ApplyBleedEffect(damage);

            if (Math.Abs(damage) < Mathf.Epsilon)
                return;

            ApplyDamage(damage);
            healthController.DamageEffect(targetPosition);
            healthController.GenericDamageResponse();
        }

        public void DeafnessEffect(float intensity)
        {
            if (intensity > 0.5f)
                healthController.ExplosionEffect(intensity);
        }

        public void Heal(float healthAmount, float healDuration = 2)
        {
            healing = true;
            Bleeding = false;
            Injured = false;
            StartCoroutine(HealProgressively(healthAmount, healDuration));
        }

        private IEnumerator HealProgressively(float healthAmount, float duration = 1)
        {
            float targetLife = Mathf.Min(vitality, CurrentVitality + healthAmount);

            for (float t = 0; t <= duration && healing; t += Time.deltaTime)
            {
                CurrentVitality = Mathf.Lerp(CurrentVitality, targetLife, t / duration);

                yield return new WaitForFixedUpdate();
            }
            healing = false;
        }
    }
}
