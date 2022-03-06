using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public interface IEquipable
    {
        void Select();

        void Deselect();
    }
}
