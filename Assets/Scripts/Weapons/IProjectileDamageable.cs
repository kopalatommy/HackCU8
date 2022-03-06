using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public interface IProjectileDamageable : IDamageable
    {
        void ProjectileDamage(float damage, Vector3 targetPosition, Vector3 hitPosition, float penetrationPower);
    }
}
