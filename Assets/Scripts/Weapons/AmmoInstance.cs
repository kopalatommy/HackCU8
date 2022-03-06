using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    [System.Serializable]
    public class AmmoInstance
    {
        [SerializeField]
        private AmmoType ammoType;

        [SerializeField]
        private int amount;

        [SerializeField]
        private int maxAmount;

        public AmmoType Instance => ammoType;

        public int Amount
        {
            get => amount;
            set => amount = Mathf.Clamp(value, 0, maxAmount);
        }

        public int MaxAmount => maxAmount;

        public bool IsEmptySlot => ammoType == null;
    }
}
