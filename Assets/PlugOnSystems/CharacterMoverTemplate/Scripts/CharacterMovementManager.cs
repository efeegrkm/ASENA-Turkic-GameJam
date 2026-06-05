using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovementManager : MonoBehaviour
{
    [Header("Active Camera")]
    [SerializeField] private Camera activeCamera;

    [Header("CharacterController Related Variables")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool hasCharacterController;

    [Header("Player Movement Rotation")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Player Physical Attributes")]
    [SerializeField] private float characterRadius = 0.5f;
    [SerializeField] private float characterHeight = 1f;

    private PlayerInput playerInput;
    private CharacterController characterController;

    private Vector2 inputMoveVector;

    private Vector3 velocity = new(0f, -2f, 0f);

    private void Awake()
    {
        SetActiveCamera();
        SetCharacterController();

        playerInput = new();
        playerInput.Player.Enable();
    }

    private void SetActiveCamera()
    {
        if (activeCamera == null)
        {
            activeCamera = Camera.main;
        }
    }

    private void SetCharacterController()
    {
        if (TryGetComponent<CharacterController>(out CharacterController characterController))
        {
            this.characterController = characterController;
            characterController.center = new(characterController.center.x, characterHeight / 2, characterController.center.z);
            characterController.height = characterHeight;
        }
    }

    void Start()
    {
        playerInput.Player.Move.performed += OnMovementPerformed;
        playerInput.Player.Move.canceled += OnMovementCanceled;
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        inputMoveVector = playerInput.Player.Move.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        inputMoveVector = Vector3.zero;
    }

    void Update()
    {
        Move();
        if (hasCharacterController)
        {
            ApplyGravity();
        }
    }

    private void Move()
    {
        float xMoveInput = inputMoveVector.x;
        float yMoveInput = inputMoveVector.y;

        Vector3 camForward = activeCamera.transform.forward;
        Vector3 camRight = activeCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDir = (camForward * yMoveInput) + (camRight * xMoveInput);

        if (movementDir != Vector3.zero)
        {
            // DEÐÝÞÝKLÝK: Local event yerine GameEvents tetikleniyor
            GameEvents.OnMoved();
            Move(hasCharacterController, movementDir);
        }
        else
        {
            // DEÐÝÞÝKLÝK: Local event yerine GameEvents tetikleniyor
            GameEvents.OnStoppedMoving();
        }
    }

    private void Move(bool hasCharacterController, Vector3 movementDir)
    {
        if (hasCharacterController)
        {
            characterController.Move(Time.deltaTime * moveSpeed * movementDir);
        }
        else
        {
            List<RaycastHit> hits = GetHitRaycasts(movementDir);
            bool isHit = hits.Count > 0;

            if (!isHit)
            {
                transform.Translate(Time.deltaTime * moveSpeed * movementDir, Space.World);
            }
        }

        Quaternion targetRotation = Quaternion.LookRotation(movementDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private List<RaycastHit> GetHitRaycasts(Vector3 movementDir)
    {
        RaycastHit[] raycastsHit = Physics.CapsuleCastAll(
            transform.position,
            transform.position + Vector3.up * characterHeight,
            characterRadius,
            movementDir,
            0.1f
        );

        List<RaycastHit> filteredRaycastsHit = new();

        foreach (RaycastHit raycastHit in raycastsHit)
        {
            if (IsColliderBlockingMovements(raycastHit.collider))
                filteredRaycastsHit.Add(raycastHit);
        }

        return filteredRaycastsHit;
    }

    private bool IsColliderBlockingMovements(Collider collider)
    {
        return false;
    }
}