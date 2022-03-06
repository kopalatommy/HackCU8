using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    [CreateAssetMenu(menuName = "Ammo Type", fileName = "Ammo Type", order = 201)]
    public class AmmoType : ScriptableObject
    {
        public enum DamageMode
        {
            DecreaseByDistance,
            Constant
        }

        [SerializeField]
        [Tooltip("Name of the ammunition.")]
        private string ammunitionName = "Ammunition";

        [SerializeField]
        [Tooltip("Bullet's mass in kilograms.")]
        [Range(0.001f, 0.4f)]
        private float projectileMass = 0.02f;

        [SerializeField]
        [Tooltip("Defines whether or not a projectile can penetrate the Target's body.")]
        private bool canPenetrate = true;

        [SerializeField]
        [Tooltip("The maximum distance that a projectile can travel penetrating an object.")]
        private float penetrationPower = 1;

        [SerializeField]
        private Vector2 refraction = new Vector2(-0.5f, 0.5f);

        [SerializeField]
        [Tooltip("The density influence defines how much the object density will affect the refraction in the bullet direction.")]
        private float densityInfluence = 3;

        [SerializeField]
        [Tooltip("Defines how the damage inflicted by the projectiles will be calculated.")]
        private DamageMode damageMode = DamageMode.Constant;

        [SerializeField]
        private Vector2 damage = new Vector2(15, 30);

        [SerializeField]
        [Tooltip("Defines how the damage will be calculated based on the distance. " +
                 "(The X axis is the target distance, in which 0 means 0 units and 1 means the full effective range and the Y axis is the damage percent)")]
        private AnimationCurve damageFalloffCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.4f, 1), new Keyframe(0.6f, 0.5f), new Keyframe(1, 0.5f));

        [SerializeField]
        [Tooltip("Bullet's initial velocity in meters per second.")]
        private float projectileSpeed = 600;

        [SerializeField]
        [Tooltip("Defines the maximum impact force that can applied by the projectile.")]
        private float maximumImpactForce = 150;

        [SerializeField]
        [Tooltip("Allow the projectile ricochet when not able to penetrate the target's body.")]
        private bool ricochet = true;

        [SerializeField]
        [Tooltip("Chance of bullet bouncing off a surface when it is shot at an angle. ")]
        [Range(0, 1)]
        private float ricochetChance = 0.1f;

        [SerializeField]
        [Tooltip("Angle of incidence is the angle between a ray incident on a surface and the normal at the point of incidence. " +
                 "Maximum Incident Angle is the highest angle that a bullet can bounce off a surface. Any angle greater than this value " +
                 "will make the bullet penetrate the object.")]
        [Range(0, 90)]
        private float maxIncidentAngle = 45;

        [SerializeField]
        [Tooltip("Defines the trajectory variation of a bullet after ricocheting a surface.")]
        [Range(0, 1)]
        private float trajectoryDeflection = 0.75f;

        [SerializeField]
        [Tooltip("Defines the density threshold so that a bullet can bounce off a surface.")]
        private float ricochetDensityThreshold = 1;

        [SerializeField]
        [Tooltip("Allow the projectile to splitting/splintering after entering Target's body.")]
        private bool fragmentation = true;

        [SerializeField]
        [Tooltip("A likelihood of a bullet splitting/splintering after entering Target's body.")]
        [Range(0, 1)]
        private float fragmentationChance = 0.1f;

        [SerializeField]
        [Tooltip("Maximum number of fragments a bullet can splinter into.")]
        private int maxFragments = 3;

        [SerializeField]
        private Vector2 fragmentScattering = new Vector2(-0.5f, 0.5f);

        [SerializeField]
        [Tooltip("Defines the density threshold so that a bullet can splinter into small fragments.")]
        private float fragmentationDensityThreshold = 0.25f;

        [SerializeField]
        [Tooltip("Projectiles that have Fragmented after impact deal high bonus Damage to Targets.")]
        private float fragmentationDamageMultiplier = 1.5f;

        /// <summary>
        /// Name of the ammunition.
        /// </summary>
        public string AmmunitionName => ammunitionName;

        /// <summary>
        /// Bullet's mass in kilograms.
        /// </summary>
        public float ProjectileMass => projectileMass;

        /// <summary>
        /// Defines whether or not a projectile can penetrate the Target's body.
        /// </summary>
        public bool CanPenetrate => canPenetrate;

        /// <summary>
        /// Defines the maximum distance that a projectile can travel penetrating an object. (Read Only)
        /// </summary>
        public float PenetrationPower => penetrationPower;

        /// <summary>
        /// Refraction is the effect of altering the bullet path after transferring energy on the collision.
        /// </summary>
        public Vector2 Refraction => refraction;

        /// <summary>
        /// The density influence defines how much the object density will affect the refraction in the bullet direction.
        /// </summary>
        public float DensityInfluence => densityInfluence;

        /// <summary>
        /// Defines how the damage inflicted by the projectiles will be calculated. (Read Only)
        /// </summary>
        public DamageMode DamageType => damageMode;

        /// <summary>
        /// Bullet's initial velocity in meters per second. (Read Only)
        /// </summary>
        public float ProjectileSpeed => projectileSpeed;

        /// <summary>
        /// Returns the damage inflicted by the projectile.
        /// </summary>
        public float Damage => Random.Range(damage.x, damage.y);

        /// <summary>
        /// Defines how the damage will be calculated based on the distance. (Read Only)
        /// </summary>
        public AnimationCurve DamageFalloffCurve => damageFalloffCurve;

        /// <summary>
        /// Allow the projectile ricochet when not able to penetrate the target's body.
        /// </summary>
        public bool Ricochet => ricochet;

        /// <summary>
        /// Chance of bullet bouncing off a surface when it is shot at an angle.
        /// </summary>
        public float RicochetChance => ricochetChance;

        /// <summary>
        /// Angle of incidence is the angle between a ray incident on a surface and the normal at the point of incidence.
        /// Maximum Incident Angle is the highest angle that a bullet can bounce off a surface. Any angle greater than this value
        /// will make the bullet penetrate the object.
        /// </summary>
        public float MaxIncidentAngle => maxIncidentAngle;

        /// <summary>
        /// Defines the trajectory variation of a bullet after ricocheting a surface.
        /// </summary>
        public float TrajectoryDeflection => trajectoryDeflection;

        /// <summary>
        /// Defines the density threshold so that a bullet can bounce off a surface.
        /// </summary>
        public float RicochetDensityThreshold => ricochetDensityThreshold;

        /// <summary>
        /// Allow the projectile to splitting/splintering after entering Target's body.
        /// </summary>
        public bool Fragmentation => fragmentation;

        /// <summary>
        /// A likelihood of a bullet splitting/splintering after entering Target's body.
        /// </summary>
        public float FragmentationChance => fragmentationChance;

        /// <summary>
        /// Maximum number of fragments a bullet can splinter into.
        /// </summary>
        public int MaxFragments => maxFragments;

        /// <summary>
        /// Change in the movement direction of a bullet fragment because of a collision with another object.
        /// </summary>
        public Vector2 FragmentScattering => fragmentScattering;

        /// <summary>
        /// Defines the density threshold so that a bullet can splinter into small fragments.
        /// </summary>
        public float FragmentationDensityThreshold => fragmentationDensityThreshold;

        /// <summary>
        /// Projectiles that have Fragmented after impact deal bonus Damage to Targets.
        /// </summary>
        public float FragmentationDamageMultiplier => fragmentationDamageMultiplier;

        /// <summary>
        /// Returns how much impact force will be applied by the projectile based on the distance.
        /// </summary>
        /// <param name="distance">Distance traveled by the projectile.</param>
        /// <returns></returns>
        public float CalculateImpactForce(float distance)
        {
            return Mathf.Min(0.5f * projectileMass * projectileSpeed * projectileSpeed / distance, maximumImpactForce);
        }
    }
}
