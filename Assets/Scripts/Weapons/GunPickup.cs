using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    [DisallowMultipleComponent]
    public class GunPickup : MonoBehaviour, IPickup
    {
        [SerializeField]
        [Tooltip("The Gun Data Asset that this gun pickup represents.")]
        private GunData gunData;

        [SerializeField]
        private int currentRounds;

        /// <summary>
        /// Returns the ID represented by this gun pickup.
        /// </summary>
        public int Identifier => gunData != null ? gunData.GetInstanceID() : -1;

        /// <summary>
        /// 
        /// </summary>
        public int CurrentRounds
        {
            get => currentRounds;
            set => currentRounds = Mathf.Max(value, 0);
        }
    }
}
