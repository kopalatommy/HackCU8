using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    [CreateAssetMenu(menuName = "Equipment Data", fileName = "Equipment Data", order = 201)]
    public class EquipmentData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The equipment name.")]
        private string equipmentName = "Equipment";

        [SerializeField]
        [Tooltip("The Prefab dropped when the character remove the equipment from the inventory.")]
        private GameObject droppablePrefab;

        [SerializeField]
        [Tooltip("The equipment weight.")]
        private float weight = 0.5f;

        public string EquipmentName => equipmentName;

        public GameObject DroppablePrefab
        {
            get => droppablePrefab;
            set => droppablePrefab = value;
        }

        public float Weight
        {
            get => weight;
            set => weight = value;
        }
    }
}
