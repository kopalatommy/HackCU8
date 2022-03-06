using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Environment
{
    public class SurfaceType : ScriptableObject
    {
        [SerializeField]
        [Tooltip("List of Walking Footsteps Sounds.")]
        private List<AudioClip> footstepsSounds = new List<AudioClip>();

        [SerializeField]
        [Tooltip("List of Sprinting Footsteps Sounds.")]
        private List<AudioClip> sprintingFootstepsSounds = new List<AudioClip>();

        [SerializeField]
        [Tooltip("List of Jump Sounds.")]
        private List<AudioClip> jumpSounds = new List<AudioClip>();

        [SerializeField]
        [Tooltip("List of Landing Sounds.")]
        private List<AudioClip> landingSounds = new List<AudioClip>();

        [SerializeField]
        [Tooltip("List of Sliding Sounds.")]
        private List<AudioClip> slidingSounds = new List<AudioClip>();

        [SerializeField]
        private Vector2 decalSize = new Vector2(0.75f, 1.5f);

        [SerializeField]
        [Tooltip("List of bullet mark materials.")]
        private List<Material> bulletImpactMaterial = new List<Material>();

        [SerializeField]
        [Tooltip("List of particles emitted by the impact of projectiles.")]
        private List<GameObject> bulletImpactParticle = new List<GameObject>();

        [SerializeField]
        [Tooltip("List of sounds emitted by the impact of projectiles.")]
        private List<AudioClip> bulletImpactSound = new List<AudioClip>();

        [SerializeField]
        [Tooltip("List of sounds of projectiles bouncing off a surface.")]
        private List<AudioClip> bulletRicochetSound = new List<AudioClip>();

        [SerializeField, Range(0, 1)]
        [Tooltip("Projectiles impact sound volume.")]
        private float bulletImpactVolume = 0.25f;

        public float BulletImpactVolume => bulletImpactVolume;

        public AudioClip GetRandomWalkingFootsteps()
        {
            if (footstepsSounds.Count <= 0)
                return null;

            if (footstepsSounds.Count == 1)
                return footstepsSounds[0];

            int i = Random.Range(1, footstepsSounds.Count);
            AudioClip a = footstepsSounds[i];

            footstepsSounds[i] = footstepsSounds[0];
            footstepsSounds[0] = a;

            return a;
        }

        public AudioClip GetRandomSprintingFootsteps()
        {
            if (sprintingFootstepsSounds.Count <= 0)
                return null;

            if (sprintingFootstepsSounds.Count == 1)
                return sprintingFootstepsSounds[0];

            int i = Random.Range(1, sprintingFootstepsSounds.Count);
            AudioClip a = sprintingFootstepsSounds[i];

            sprintingFootstepsSounds[i] = sprintingFootstepsSounds[0];
            sprintingFootstepsSounds[0] = a;

            return a;
        }

        public AudioClip GetRandomLandingSound()
        {
            if (landingSounds.Count <= 0)
                return null;

            if (landingSounds.Count == 1)
                return landingSounds[0];

            int i = Random.Range(1, landingSounds.Count);
            AudioClip a = landingSounds[i];

            landingSounds[i] = landingSounds[0];
            landingSounds[0] = a;

            return a;
        }

        public AudioClip GetRandomJumpSound()
        {
            if (jumpSounds.Count <= 0)
                return null;

            if (jumpSounds.Count == 1)
                return jumpSounds[0];

            int i = Random.Range(1, jumpSounds.Count);
            AudioClip a = jumpSounds[i];

            jumpSounds[i] = jumpSounds[0];
            jumpSounds[0] = a;

            return a;
        }

        public AudioClip GetRandomSlidingSound()
        {
            if (slidingSounds.Count <= 0)
                return null;

            if (slidingSounds.Count == 1)
                return slidingSounds[0];

            int i = Random.Range(1, slidingSounds.Count);
            AudioClip a = slidingSounds[i];

            slidingSounds[i] = slidingSounds[0];
            slidingSounds[0] = a;

            return a;
        }

        public Material GetRandomDecalMaterial()
        {
            if (bulletImpactMaterial.Count <= 0)
                return null;

            if (bulletImpactMaterial.Count == 1)
                return bulletImpactMaterial[0];

            int i = Random.Range(1, bulletImpactMaterial.Count);
            Material material = bulletImpactMaterial[i];

            bulletImpactMaterial[i] = bulletImpactMaterial[0];
            bulletImpactMaterial[0] = material;

            return material;
        }

        public GameObject GetRandomImpactParticle()
        {
            if (bulletImpactParticle.Count <= 0)
                return null;

            if (bulletImpactParticle.Count == 1)
                return bulletImpactParticle[0];

            int i = Random.Range(1, bulletImpactParticle.Count);
            GameObject gameObject = bulletImpactParticle[i];

            bulletImpactParticle[i] = bulletImpactParticle[0];
            bulletImpactParticle[0] = gameObject;

            return gameObject;
        }

        public AudioClip GetRandomImpactSound()
        {
            if (bulletImpactSound.Count <= 0)
                return null;

            if (bulletImpactSound.Count == 1)
                return bulletImpactSound[0];

            int i = Random.Range(1, bulletImpactSound.Count);
            AudioClip audioClip = bulletImpactSound[i];

            bulletImpactSound[i] = bulletImpactSound[0];
            bulletImpactSound[0] = audioClip;

            return audioClip;
        }

        public AudioClip GetRandomRicochetSound()
        {
            if (bulletRicochetSound.Count <= 0)
                return null;

            if (bulletRicochetSound.Count == 1)
                return bulletRicochetSound[0];

            int i = Random.Range(1, bulletRicochetSound.Count);
            AudioClip audioClip = bulletRicochetSound[i];

            bulletRicochetSound[i] = bulletRicochetSound[0];
            bulletRicochetSound[0] = audioClip;

            return audioClip;
        }

        public float GetRandomDecalSize()
        {
            return Random.Range(decalSize.x, decalSize.y);
        }
    }
}
