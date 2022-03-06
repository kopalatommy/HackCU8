using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Animation
{
    public interface IWeaponAnimator
    {
        void Draw();

        void Hide();

        void Hit(Vector3 position);

        void Interact();

        void Vault();
    }
}
