using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnnaturalSelection.Audio;

namespace UnnaturalSelection.Character
{
    [DisallowMultipleComponent]
    public class HealthController : MonoBehaviour
    {
        [SerializeField]
        protected List<DamageHandler> bodyParts = new List<DamageHandler>();

        [SerializeField]
        [Tooltip("Sound played when the character is healing or affected by a bonus.")]
        protected AudioClip healSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Heal Sound volume when the character is healing or affected by a bonus.")]
        protected float healVolume = 0.3f;

        [SerializeField]
        [Tooltip("Sound played when the character is heavily damaged.")]
        protected AudioClip coughSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Cough Sound volume when the character is heavily damaged.")]
        protected float coughVolume = 0.5f;

        [SerializeField]
        [Tooltip("Sound played when the player suffer a heavy fall damage.")]
        protected AudioClip breakLegsSound;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Break Legs Sound volume when the player suffer a heavy fall damage.")]
        protected float breakLegsVolume = 0.3f;

        [SerializeField]
        protected List<AudioClip> hitSounds = new List<AudioClip>();

        [SerializeField]
        protected List<AudioClip> damageSounds = new List<AudioClip>();

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the Hit Sound volume when the character is hit by an enemy.")]
        protected float hitVolume = 0.3f;

        [SerializeField]
        [Tooltip("Sound played when the player is hit by a near explosion, simulating a temporary deafness effect.")]
        protected AudioClip explosionNoise;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the Explosion Noise volume when the player is hit by a near explosion, simulating a temporary deafness effect.")]
        protected float explosionNoiseVolume = 0.3f;

        [SerializeField]
        protected Vector2 deafnessDuration = new Vector2(2, 5);

        [SerializeField]
        [Tooltip("Default Audio Mixer snapshot.")]
        protected AudioMixerSnapshot normalSnapshot;

        [SerializeField]
        [Tooltip("Snapshot used to simulate echoing and temporary deafness.")]
        protected AudioMixerSnapshot stunnedSnapshot;

        [SerializeField]
        [Tooltip("The dead character prefab. (Instantiated when the character dies).")]
        protected GameObject deadCharacter;

        public event Action<Vector3> DamageEvent;

        public event Action ExplosionEvent;

        public event Action HitEvent;

        private BloodSplashEffect bloodSplashEffect;
        private MovementController fPController;
        private AudioEmitter playerHealthSource;
        private AudioEmitter playerBreathSource;

        private float totalVitality;
        private float currentVitality;
        private float tempVitality;

        public bool Bleeding
        {
            get;
            private set;
        }

        public bool Limping
        {
            get;
            private set;
        }

        public bool Trembling
        {
            get;
            private set;
        }

        public bool Injured
        {
            get;
            private set;
        }

        public virtual bool IsAlive
        {
            get
            {
                if (bodyParts.Count == 0)
                    return true;

                return currentVitality > 0;
            }
        }

        public float HealthPercent
        {
            get
            {
                if (bodyParts.Count == 0)
                    return 1;
                return currentVitality / totalVitality;
            }
        }

        public float EditorHealthPercent
        {
            get
            {
                if (Math.Abs(currentVitality) < Mathf.Epsilon)
                    return 1;
                return currentVitality / totalVitality;
            }
        }

        public void RegisterBodyPart(DamageHandler bodyPart)
        {
            bodyParts.Add(bodyPart);
        }

        protected virtual void Start()
        {
            // References
            fPController = GetComponent<MovementController>();
            bloodSplashEffect = GetComponentInChildren<BloodSplashEffect>();
            fPController.landingEvent += FallDamage;

            // Audio Sources
            playerHealthSource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterHealth", transform.root);
            playerBreathSource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterGeneric", transform.root);

            // Body parts
            for (int i = 0, bodyPartsCount = bodyParts.Count; i < bodyPartsCount; i++)
            {
                if (!bodyParts[i])
                    continue;

                bodyParts[i].Init(this);
                totalVitality += bodyParts[i].Vitality;
            }
            currentVitality = totalVitality;
        }

        protected virtual void Update()
        {
            if (bodyParts.Count < 1)
                return;

            tempVitality = 0;
            for (int i = 0, bodyPartsCount = bodyParts.Count; i < bodyPartsCount; i++)
            {
                if (!bodyParts[i])
                    continue;

                // Sets if the player is dead if any vital body part vitality is equal to 0.
                GameplayManager.Instance.IsDead |= (Mathf.Abs(bodyParts[i].CurrentVitality) < Mathf.Epsilon && bodyParts[i].Vital);

                if (bodyParts[i].Injured)
                {
                    Injured = true;
                    if (bodyParts[i].TraumaType == TraumaType.Limp)
                        Limping = true;
                    if (bodyParts[i].TraumaType == TraumaType.Tremor)
                        Trembling = true;
                }

                if (bodyParts[i].Bleeding)
                {
                    Bleeding = true;
                }

                // Make the character lose blood from the entire structure.
                if (Bleeding && !bodyParts[i].Bleeding)
                {
                    bodyParts[i].Bleeding = true;
                }

                tempVitality += bodyParts[i].CurrentVitality;
            }

            currentVitality = tempVitality;
            GameplayManager.Instance.IsDead |= Mathf.Abs(currentVitality) < Mathf.Epsilon;

            // Sets whether or not the character has broken their legs.
            fPController.LowerBodyDamaged = Limping;
            fPController.TremorTrauma = Trembling;

            if (bloodSplashEffect)
            {
                bloodSplashEffect.BloodAmount = 1 - currentVitality / totalVitality;
            }

            if (GameplayManager.Instance.IsDead && deadCharacter != null)
            {
                Die();
            }
        }

        protected void LateUpdate()
        {
            if (Trembling && Limping && !playerHealthSource.IsPlaying)
                playerHealthSource.Play(coughSound, coughVolume);
        }

        /// <summary>
        /// Instantly regenerates vitality and applies an adrenaline bonus.
        /// </summary>
        /// <param name="healthAmount">Amount of vitality to be healed.</param>
        /// <param name="healInjuries">Should the character injuries be healed?</param>
        /// <param name="bonus">Allow adrenaline bonus?</param>
        /// <param name="bonusDuration">Duration of the adrenaline bonus.</param>
        public virtual void Heal(float healthAmount, bool healInjuries, bool bonus = false, float bonusDuration = 10)
        {
            if (healthAmount > 0 && bodyParts.Count > 0)
            {
                for (int i = 0, bodyPartsCount = bodyParts.Count; i < bodyPartsCount; i++)
                {
                    if (bodyParts[i])
                    {
                        bodyParts[i].Heal(healthAmount / bodyPartsCount); // Heals each body part by the same amount.
                    }
                }

                Invoke(nameof(SetNormalSnapshot), 0);
                playerBreathSource.Stop();
                playerHealthSource.ForcePlay(healSound, healVolume);

                if (healInjuries)
                {
                    Limping = false;
                    Trembling = false;
                    Bleeding = false;
                }
            }
        }

        protected virtual void FallDamage(float damage)
        {
            for (int i = 0, bodyPartsCount = bodyParts.Count; i < bodyPartsCount; i++)
            {
                if (!bodyParts[i])
                    continue;

                if (bodyParts[i].BodyPart == BodyPart.Leg && !bodyParts[i].CanRegenerate)
                {
                    bodyParts[i].Damage(damage);

                    if (bodyParts[i].Injured)
                    {
                        Limping = true;
                        if (!bodyParts[i].CanRegenerate)
                            bodyParts[i].ApplyBleedEffect(damage);

                        playerHealthSource.Stop();
                        AudioManager.Instance.PlayClipAtPoint(breakLegsSound, transform.position, 5, 10, breakLegsVolume);
                    }
                }

                if (bodyParts[i].BodyPart != BodyPart.FullBody)
                    continue;

                bodyParts[i].Damage(damage);
                if (damage > bodyParts[i].Vitality * 0.7f && !bodyParts[i].CanRegenerate)
                {
                    Limping = true;
                    bodyParts[i].ApplyBleedEffect(damage);

                    playerHealthSource.Stop();
                    AudioManager.Instance.PlayClipAtPoint(breakLegsSound, transform.position, 5, 10, breakLegsVolume);
                }
            }
        }

        public virtual void DamageEffect(Vector3 targetPosition)
        {
            DamageEvent?.Invoke(targetPosition);
        }

        public virtual void HitDamageResponse()
        {
            if (hitSounds.Count <= 0)
                return;

            if (hitSounds.Count == 1)
            {
                playerHealthSource.Play(hitSounds[0], hitVolume);
                return;
            }

            int i = UnityEngine.Random.Range(1, hitSounds.Count);
            AudioClip a = hitSounds[i];

            hitSounds[i] = hitSounds[0];
            hitSounds[0] = a;

            playerHealthSource.Play(a, hitVolume);
        }

        public virtual void GenericDamageResponse()
        {
            if (damageSounds.Count <= 0)
                return;

            if (damageSounds.Count == 1)
            {
                playerHealthSource.Play(damageSounds[0], hitVolume);
                return;
            }

            int i = UnityEngine.Random.Range(1, damageSounds.Count);
            AudioClip a = damageSounds[i];

            damageSounds[i] = damageSounds[0];
            damageSounds[0] = a;

            playerHealthSource.Play(a, hitVolume);
        }

        public virtual void BulletHitEffect()
        {
            HitEvent?.Invoke();
        }

        public virtual void ExplosionEffect(float intensity)
        {
            playerHealthSource.ForcePlay(explosionNoise, explosionNoiseVolume);
            stunnedSnapshot.TransitionTo(0.1f);

            float duration = deafnessDuration.x + (deafnessDuration.y - deafnessDuration.x) * intensity;
            Invoke(nameof(SetNormalSnapshot), duration);

            ExplosionEvent?.Invoke();
        }

        protected virtual void SetNormalSnapshot()
        {
            normalSnapshot.TransitionTo(0.3f);
        }

        protected virtual void Die()
        {
            SetNormalSnapshot();
            gameObject.SetActive(false);

            Transform t = transform;
            Instantiate(deadCharacter, t.position, t.rotation);
        }
    }
}
