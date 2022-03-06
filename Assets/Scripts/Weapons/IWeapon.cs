using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public interface IWeapon : IEquipable
    {
        int Identifier { get; }

        GameObject Viewmodel { get; }

        bool CanSwitch
        {
            get;
        }

        bool CanUseEquipment
        {
            get;
        }

        bool IsBusy
        {
            get;
        }

        float HideAnimationLength
        {
            get;
        }

        float InteractAnimationLength
        {
            get;
        }

        float InteractDelay
        {
            get;
        }

        void Interact();

        void SetCurrentRounds(int currentRounds);
    }
}
