using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Character
{
    [RequireComponent(typeof(MovementController))]
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField] private float health = 100;

        // Drivers
        [SerializeField] private MovementController movementController;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();
        }
    }
}
