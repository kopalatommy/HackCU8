using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnnaturalSelection.Character;

namespace UnnaturalSelection.Player
{
    [RequireComponent(typeof(MovementController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private MovementController movementController;

        private InputActionMap inputBindings;
        private InputAction jumpAction;
        private InputAction crouchAction;
        private InputAction moveAction;
        private InputAction sprintingAction;

        private void Awake()
        {
            movementController = GetComponent<MovementController>();

            inputBindings = GameplayManager.Instance.GetActionMap("Movement");
            inputBindings.Enable();

            jumpAction = inputBindings.FindAction("Jump");
            crouchAction = inputBindings.FindAction("Crouch");
            moveAction = inputBindings.FindAction("Move");
            sprintingAction = inputBindings.FindAction("Sprint");

            if (movementController.hasCamera)
                movementController.cameraController.Controllable = true;
        }

        private void Start()
        {
            movementController.OnStart();
        }

        private void Update()
        {
            HandleInput();
            movementController.OnUpdate();
        }

        private void HandleInput()
        {
            movementController.ShouldJump = jumpAction.triggered;
            movementController.ShouldCrouch = crouchAction.triggered;
            movementController.ShouldRun = sprintingAction.ReadValue<float>() > 0;
            movementController.MoveInput = moveAction.ReadValue<Vector2>();
        }
    }
}
