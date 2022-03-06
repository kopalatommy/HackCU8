using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public interface IDamageable
    {
        bool IsAlive
        {
            get;
        }

        void Damage(float damage);

        void Damage(float damage, Vector3 targetPosition, Vector3 hitPosition);
    }
}
