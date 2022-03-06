using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public enum WeaponType
    {
        Primary,
        Secondary,
        Undefined
    }

    [CreateAssetMenu(menuName = "Gun Data", fileName = "Gun Data", order = 201)]
    public class GunData : ScriptableObject
    {
        public enum FireMode
        {
            None,
            FullAuto,
            Single,
            Burst,
            ShotgunSingle,
            ShotgunAuto
        }
        public enum ReloadMode
        {
            Magazines,
            BulletByBullet
        }

        [SerializeField]
        [Tooltip("The gun name.")]
        private string gunName = "Gun";

        [SerializeField]
        private WeaponType weaponType = WeaponType.Primary;

        [SerializeField]
        [Tooltip("The Prefab dropped when the character picks up a different gun.")]
        private GameObject droppablePrefab;

        [SerializeField]
        [Tooltip("The gun weight.")]
        private float weight = 4.0f;

        [SerializeField]
        [Tooltip("The gun size. (Used to define how far the character can hit with a melee attack)")]
        private float size = 1;

        [SerializeField]
        [Tooltip("Defines how much damage will be inflict by a melee attack.")]
        private float meleeDamage = 50;

        [SerializeField]
        [Tooltip("Defines how much force will be applied when melee attack.")]
        private float meleeForce = 5;

        [SerializeField]
        [Tooltip("Defines the primary fire mode of this gun.")]
        private FireMode primaryFireMode = FireMode.FullAuto;

        [SerializeField]
        [Tooltip("Defines the secondary fire mode of this gun.")]
        private FireMode secondaryFireMode = FireMode.None;

        [SerializeField]
        [Tooltip("Rate of fire is the frequency at which a specific gun can fire or launch its projectiles. It is usually measured in rounds per minute (RPM or round/min).")]
        [Range(1, 1500)]
        private int primaryRateOfFire = 600;

        [SerializeField]
        [Tooltip("Rate of fire is the frequency at which a specific gun can fire or launch its projectiles. It is usually measured in rounds per minute (RPM or round/min).")]
        [Range(1, 1500)]
        private float secondaryRateOfFire = 600;

        [SerializeField]
        [Tooltip("Defines how far this gun can accurately hit a target.")]
        private float range = 400;

        [SerializeField]
        [Tooltip("Defines how many bullets the shotgun will fire at once.")]
        private int bulletsPerShoot = 5;

        [SerializeField]
        [Tooltip("Defines how many bullets will be fired sequentially with a single pull of the trigger.")]
        private int bulletsPerBurst = 3;

        [SerializeField]
        [Tooltip("The Layers affected by this gun.")]
        private LayerMask affectedLayers = 1;

        [SerializeField]
        [Tooltip("The ammo type defines the characteristics of the projectiles utilized by a gun.")]
        private AmmoType ammoType;

        [SerializeField]
        [Tooltip("Defines how the gun is loaded with ammo.")]
        private ReloadMode reloadMode = ReloadMode.Magazines;

        [SerializeField]
        [Tooltip("Defines how many bullets has in the magazine.")]
        private int roundsPerMagazine = 30;

        [SerializeField]
        [Tooltip("Enabling the chamber will add an additional bullet to your gun.")]
        private bool hasChamber;

        [SerializeField]
        [Tooltip("Sets the radius of the conical fustrum. Used to calculate the bullet spread angle.")]
        private float maximumSpread = 1.75f;

        [SerializeField]
        [Range(0.01f, 1)]
        [Tooltip("Defines the minimum accuracy while using this gun. (If the character is moving or holding the trigger it will lose accuracy until it reaches the Base Accuracy)")]
        private float baseAccuracy = 0.75f;

        [SerializeField]
        [Range(0.01f, 1)]
        [Tooltip("Defines the accuracy percentage when the character is shooting from the hip. (0 is totally inaccurate and 1 is totally accurate)")]
        private float hipAccuracy = 0.6f;

        [SerializeField]
        [Range(0.01f, 1)]
        [Tooltip("Defines the accuracy percentage when the character is aiming. (0 is totally inaccurate and 1 is totally accurate)")]
        private float aimAccuracy = 0.9f;

        [SerializeField]
        [Range(0, 3)]
        [Tooltip("Defines how fast this gun will be inaccurate due the character movement.")]
        private float decreaseRateByWalking = 1;

        [SerializeField]
        [Range(0, 3)]
        [Tooltip("Defines how fast this gun will be inaccurate due constant shooting.")]
        private float decreaseRateByShooting = 1;

        public string GunName => gunName;

        /// <summary>
        /// 
        /// </summary>
        public WeaponType Type => weaponType;

        /// <summary>
        /// The Prefab dropped when the character picks up a different gun.
        /// </summary>
        public GameObject DroppablePrefab => droppablePrefab;

        /// <summary>
        /// The gun weight. (Read Only)
        /// </summary>
        public float Weight => weight;

        /// <summary>
        /// The gun size.(Read Only)
        /// </summary>
        public float Size => size;

        /// <summary>
        /// Defines how much force will be applied when melee attack. (Read Only)
        /// </summary>
        public float MeleeForce => meleeForce;

        /// <summary>
        /// Defines how much damage will be inflict by a melee attack. (Read Only)
        /// </summary>
        public float MeleeDamage => meleeDamage;

        /// <summary>
        /// Defines the primary fire mode of this gun. (Read Only)
        /// </summary>
        public FireMode PrimaryFireMode => primaryFireMode;

        /// <summary>
        /// Defines the secondary fire mode of this gun. (Read Only)
        /// </summary>
        public FireMode SecondaryFireMode => secondaryFireMode;

        /// <summary>
        /// Defines the time interval between each shot while the primary fire mode is selected. (Read Only)
        /// </summary>
        public float PrimaryRateOfFire => 60.0f / primaryRateOfFire;

        /// <summary>
        /// Defines the time interval between each shot while the secondary fire mode is selected. (Read Only)
        /// </summary>
        public float SecondaryRateOfFire => 60.0f / secondaryRateOfFire;

        /// <summary>
        /// Defines how far this gun can accurately hit a target. (Read Only)
        /// </summary>
        public float Range => range;

        /// <summary>
        /// Defines how many bullets the shotgun will fire at once. (Read Only)
        /// </summary>
        public int BulletsPerShoot => bulletsPerShoot;

        /// <summary>
        /// Defines how many bullets will be fired sequentially with a single pull of the trigger. (Read Only)
        /// </summary>
        public int BulletsPerBurst => bulletsPerBurst;

        /// <summary>
        /// The Layers affected by this gun. (Read Only)
        /// </summary>
        public LayerMask AffectedLayers => affectedLayers;

        /// <summary>
        /// Defines how the damage inflicted by the projectiles will be calculated. (Read Only)
        /// </summary>
        public AmmoType.DamageMode DamageType => ammoType.DamageType;

        /// <summary>
        /// Returns the damage inflicted by the projectile.
        /// </summary>
        public float Damage => ammoType.Damage;

        /// <summary>
        /// Defines how the damage will be calculated based on the distance. (Read Only)
        /// </summary>
        public AnimationCurve DamageFalloffCurve => ammoType.DamageFalloffCurve;

        /// <summary>
        /// Type of ammunition used by this gun.
        /// </summary>
        public AmmoType AmmoType => ammoType;

        /// <summary>
        /// Defines how the gun is loaded with ammo. (Read Only)
        /// </summary>
        public ReloadMode ReloadType => reloadMode;

        /// <summary>
        /// Defines how many bullets has in the magazine. (Read Only)
        /// </summary>
        public int RoundsPerMagazine => roundsPerMagazine;

        /// <summary>
        /// Returns true if this gun has a chamber, false otherwise. (Read Only)
        /// </summary>
        public bool HasChamber => hasChamber;

        /// <summary>
        /// Defines the radius of the conical fustrum. Used to calculate the bullet spread angle. (Read Only)
        /// </summary>
        public float MaximumSpread => maximumSpread;

        /// <summary>
        /// Defines the minimum accuracy while using this gun. (Read Only)
        /// </summary>
        public float BaseAccuracy => baseAccuracy;

        /// <summary>
        /// Defines the accuracy percentage when the character is shooting from the hip. (Read Only)
        /// </summary>
        public float HIPAccuracy => hipAccuracy;

        /// <summary>
        /// Defines the accuracy percentage when the character is aiming. (Read Only)
        /// </summary>
        public float AIMAccuracy => aimAccuracy;

        /// <summary>
        /// Defines how fast this gun will be inaccurate due the character movement. (Read Only)
        /// </summary>
        public float DecreaseRateByWalking => decreaseRateByWalking;

        /// <summary>
        /// Defines how fast this gun will be inaccurate due constant shooting. (Read Only)
        /// </summary>
        public float DecreaseRateByShooting => decreaseRateByShooting;

        /// <summary>
        /// Calculates the amount of damage that will be inflicted by the projectile based on the target distance.
        /// </summary>
        /// <param name="distance">Distance traveled by the projectile.</param>
        public float CalculateDamage(float distance)
        {
            switch (DamageType)
            {
                case AmmoType.DamageMode.Constant:
                    return Damage;
                case AmmoType.DamageMode.DecreaseByDistance:
                    return Damage * DamageFalloffCurve.Evaluate(distance / Range);
                default:
                    return Damage;
            }
        }
    }
}
