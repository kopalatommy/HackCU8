using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Audio;

namespace UnnaturalSelection.Weapons
{
    [RequireComponent(typeof(AudioSource))]
    public class CollisionSound : MonoBehaviour
    {
        /// <summary>
        /// Should ignore collision with the character?
        /// </summary>
        [SerializeField]
        [Tooltip("Should ignore collision with the character?")]
        private bool ignoreCharacter = true;

        /// <summary>
        /// The minimum force required when the collider hits another one to play the impact sound.
        /// </summary>
        [SerializeField]
        [Tooltip("The minimum force required when the collider hits another one to play the impact sound.")]
        private float minimumImpactForce = 0.25f;

        /// <summary>
        /// The sound played when the collider hits another one.
        /// </summary>
        [SerializeField]
        [Tooltip("The sound played when the collider hits another one.")]
        private AudioClip collisionSound;

        /// <summary>
        /// The collision sound volume.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        private float collisionVolume = 0.3f;

        private AudioSource m_ColliderSource;

        private void Start()
        {
            m_ColliderSource = GetComponent<AudioSource>();
            m_ColliderSource.playOnAwake = false;
            m_ColliderSource.clip = collisionSound;
            m_ColliderSource.volume = collisionVolume * AudioManager.Instance.SFxVolume;
        }

        /// <summary>
        /// Check if the collider collided with something and the contact force.
        /// </summary>
        /// <param name="col">Collider hit by the explosive.</param>
        private void OnCollisionEnter(Collision col)
        {
            if (ignoreCharacter && col.gameObject.CompareTag("Player"))
                return;

            if (col.relativeVelocity.magnitude > minimumImpactForce)
            {
                m_ColliderSource.Play();
            }
        }
    }
}
