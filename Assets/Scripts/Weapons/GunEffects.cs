using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    [System.Serializable]
    public class GunEffects
    {
        [SerializeField]
        [Tooltip("Enables muzzle blast particles.")]
        private bool muzzleFlash;

        [SerializeField]
        [Tooltip("The muzzle blast particle.")]
        private ParticleSystem muzzleParticle;

        [SerializeField]
        private bool tracer;

        [SerializeField]
        [Tooltip("The tracer prefab.")]
        private Rigidbody tracerPrefab;

        [SerializeField]
        [Tooltip("Duration of the tracer.")]
        private float tracerDuration = 1f;

        [SerializeField]
        [Tooltip("The projectile speed.")]
        private float tracerSpeed = 450;

        [SerializeField]
        [Tooltip("Position of origin of the tracer. (Position it will be instantiated)")]
        private Transform tracerOrigin;

        [SerializeField]
        [Tooltip("Enables shell ejection when firing.")]
        private bool shell;

        [SerializeField]
        [Tooltip("Bullet shell emitter.")]
        private ParticleSystem shellParticle;

        [SerializeField]
        private Vector2 shellSpeed = new Vector2(1, 3);

        [SerializeField]
        [Tooltip("Delay in seconds for the shell to be ejected.")]
        private float startDelay;

        // Current cartridge particle
        private ParticleSystem[] cachedParticle;

        [SerializeField]
        [Tooltip("Allows the character to drop a magazine by reloading the gun.")]
        private bool magazineDrop;

        [SerializeField]
        [Tooltip("The magazine prefab to be instantiated.")]
        private Rigidbody magazinePrefab;

        [SerializeField]
        [Tooltip("Position that the magazine will eject.")]
        private Transform dropOrigin;

        [SerializeField]
        [Tooltip("Allows magazines to be ejected when tactically reloading.")]
        private bool tacticalReloadDrop = true;

        [SerializeField]
        [Tooltip("Allows magazines to be ejected when full reloading.")]
        private bool fullReloadDrop = true;

        [SerializeField]
        [Tooltip("Delay in seconds to eject the magazine when tactically reloading. (To match the animation)")]
        private float tacticalDropDelay;

        [SerializeField]
        [Tooltip("Delay in seconds to eject the magazine when full reloading. (To match the animation)")]
        private float fullDropDelay;

        [SerializeField]
        [Tooltip("Maximum number of magazines that can be instantiated by this gun.")]
        private int maxMagazinesPrefabs = 5;

        /// <summary>
        /// It returns the velocity of the projectile launched by that weapon.
        /// </summary>
        public float TracerSpeed => tracerSpeed;

        /// <summary>
        /// Returns the duration of the tracer launched by that weapon.
        /// </summary>
        public float TracerDuration => tracerDuration;

        /// <summary>
        /// Returns true if this gun ejects magazines when tactically reloading, false otherwise.
        /// </summary>
        public bool TacticalReloadDrop => tacticalReloadDrop;

        /// <summary>
        /// Returns the delay in seconds to eject magazines when tactically reloading.
        /// </summary>
        public float TacticalDropDelay => tacticalDropDelay;

        /// <summary>
        /// Returns true if this gun ejects magazines when full reloading, false otherwise.
        /// </summary>
        public bool FullReloadDrop => fullReloadDrop;

        /// <summary>
        /// Returns the delay in seconds to eject magazines when full reloading.
        /// </summary>
        public float FullDropDelay => fullDropDelay;

        private int m_LastMagazine;
        private List<GameObject> m_MagazineList = new List<GameObject>();

        /// <summary>
        /// Updates muzzle blast particle.
        /// </summary>
        /// <param name="muzzleParticle">The new muzzle blast particle</param>
        internal void UpdateMuzzleBlastParticle(ParticleSystem muzzleParticle)
        {
            this.muzzleParticle = muzzleParticle;
        }

        public void Init()
        {
            if (!shell || !shellParticle)
                return;

            if (cachedParticle == null || cachedParticle.Length == 0)
                cachedParticle = shellParticle.GetComponentsInChildren<ParticleSystem>();
        }

        /// <summary>
        /// Plays the effects emitted at the instant the gun fires.
        /// </summary>
        public void Play()
        {
            if (muzzleFlash && muzzleParticle)
            {
                muzzleParticle.Play();
            }

            if (!shell || !shellParticle)
                return;

            ParticleSystem.MainModule mainModule = shellParticle.main;
            mainModule.startSpeed = Random.Range(shellSpeed.x, shellSpeed.y);
            mainModule.startDelay = startDelay;

            if (cachedParticle.Length > 0)
            {
                for (int i = 0, l = cachedParticle.Length; i < l; i++)
                {
                    ParticleSystem.MainModule childrenModule = cachedParticle[i].main;
                    childrenModule.startDelay = startDelay;
                }
            }

            shellParticle.Play();
        }

        /// <summary>
        /// Ejects a magazine after reloading the weapon.
        /// </summary>
        /// <param name="character">Collider to be ignored the magazine.</param>
        public void DropMagazine(Collider character)
        {
            if (!magazineDrop)
                return;

            if (!magazinePrefab || !dropOrigin)
                return;

            // Object pooling
            if (m_MagazineList.Count == maxMagazinesPrefabs)
            {
                int magazine = m_LastMagazine++ % maxMagazinesPrefabs;
                m_MagazineList[magazine].transform.position = dropOrigin.position;
                m_MagazineList[magazine].transform.rotation = dropOrigin.rotation;
                m_MagazineList[magazine].GetComponent<Rigidbody>().velocity = Physics.gravity;
            }
            else
            {
                Rigidbody magazine = Object.Instantiate(magazinePrefab, dropOrigin.position, dropOrigin.rotation);
                magazine.velocity = Physics.gravity;

                Physics.IgnoreCollision(magazine.GetComponent<Collider>(), character, true);
                m_MagazineList.Add(magazine.gameObject);
            }
        }

        /// <summary>
        /// Creates a projectile tracer and instantiates with the direction of shot.
        /// </summary>
        /// <param name="origin">Position where the tracer will come from.</param>
        /// <param name="direction">Direction where the tracer goes.</param>
        /// <param name="duration">Tracer lifespan.</param>
        public void CreateTracer(Transform origin, Vector3 direction, float duration)
        {
            if (!this.tracer)
                return;

            if (!tracerPrefab || !origin)
                return;

            Rigidbody tracer = Object.Instantiate(tracerPrefab, tracerOrigin.position, origin.rotation);
            tracer.velocity = direction * tracerSpeed;
            Object.Destroy(tracer.gameObject, duration);
        }
    }
}
