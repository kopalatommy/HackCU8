using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Audio;

namespace UnnaturalSelection.Weapons
{
    [DisallowMultipleComponent]
    public class Explosive : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Defines the explosion radius of effect.")]
        private float explosionRadius = 15;

        /// <summary>
        /// Defines how strong the objects will be pushed if within the blast radius.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines how strong the objects will be pushed if within the blast radius.")]
        private float explosionForce = 35;

        /// <summary>
        /// Defines whether the explosion should ignore objects within the radius that may block its effects.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the explosion should ignore objects within the radius that may block its effects.")]
        private bool ignoreCover;

        /// <summary>
        /// Defines the maximum damage caused by the explosion.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the maximum damage caused by the explosion.")]
        private float damage = 120;

        /// <summary>
        /// Defines whether the explosive should explode when it touches any surface.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the explosive should explode when it touches any surface.")]
        private bool explodeWhenCollide;

        /// <summary>
        /// Defines the time required in seconds for the object to explode.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the time required in seconds for the object to explode.")]
        private float timeToExplode = 3;

        /// <summary>
        /// The particle instantiated by the explosion.
        /// </summary>
        [SerializeField]
        [Tooltip("The particle instantiated by the explosion.")]
        private GameObject explosionParticle;

        /// <summary>
        /// The explosion sound.
        /// </summary>
        [SerializeField]
        [Tooltip("The explosion sound.")]
        private AudioClip explosionSound;

        /// <summary>
        /// Defines the explosion sound volume.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the explosion sound volume.")]
        private float explosionVolume = 0.5f;

        private void Start()
        {
            if (!explodeWhenCollide)
                Invoke(nameof(Explosion), timeToExplode);
        }

        /// <summary>
        /// Simulates the explosion effect and instantiate the particles.
        /// </summary>
        private void Explosion()
        {
            if (explosionParticle != null)
                Instantiate(explosionParticle, transform.position, Quaternion.identity);

            Vector3 position = transform.position;

            // Calculate damage.
            global::UnnaturalSelection.Weapons.Explosion.CalculateExplosionDamage(explosionRadius, explosionForce, damage, new Vector3(position.x, position.y, position.z), ignoreCover);

            AudioManager.Instance.PlayClipAtPoint(explosionSound, position, 20, 100, explosionVolume);
            Destroy(gameObject);
        }

        /// <summary>
        /// Check if the explosive collided with something and the contact force.
        /// </summary>
        private void OnCollisionEnter()
        {
            if (explodeWhenCollide)
                Explosion();
        }

        /// <summary>
        /// Draw a sphere to visualize the radius of the blast effect.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
