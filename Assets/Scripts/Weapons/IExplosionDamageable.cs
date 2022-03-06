using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public interface IExplosionDamageable : IDamageable
    {
        void ExplosionDamage(float damage, Vector3 targetPosition, Vector3 hitPosition);
    }
}
