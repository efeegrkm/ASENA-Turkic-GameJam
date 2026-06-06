using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovementManager : MonoBehaviour
{
    [Header("Active Camera")]
    [SerializeField] private Camera activeCamera;

    [Header("CharacterController Related Variables")]
    [SerializeField] private float gravity = -9.81f;
    private bool hasCharacterController;

    [Header("Player Movement Rotation")]
    [SerializeField] private float moveSpeed = 7f;
    // ÇÖZÜM: Dönüş hızlarını formlara göre ayırıyoruz
    [Tooltip("İnsan formunun kendi ekseninde dönme çevikliği")]
    [SerializeField] private float humanRotationSpeed = 10f;
    [Tooltip("Kurt formunun kendi ekseninde dönme hantallığı (Düşük DPI hissiyatı)")]
    [SerializeField] private float wolfRotationSpeed = 4f;

    [Header("Player Physical Attributes")]
    [SerializeField] private float characterRadius = 0.5f;
    [SerializeField] private float characterHeight = 1f;
    [SerializeField] private float wolfHeight = 1.5f;

    [Header("Wolf/Character Form")]
    [SerializeField] private Vector3 formOffset = new(0f, 0f, 0.5f);

    [Header("Aiming Settings")]
    [SerializeField] private float aimMoveSpeed = 3f;
    [SerializeField] private float bodyRotationThreshold = 15f;
    private bool isAiming = false;

    private PlayerInput playerInput;
    private CharacterController characterController;

    private Vector2 inputMoveVector;
    private Vector3 velocity = new(0f, -2f, 0f);

    private bool isMovementLocked = false;
    private float speedMultiplier = 1.0f;
    private bool isDraggingBaby = false;

    // Çalışma anında aktif olan dönüş hızını tutacak değişken
    private float currentRotationSpeed;

    private void Awake()
    {
        SetActiveCamera();
        SetCharacterController();

        hasCharacterController = characterController != null;

        // Oyun başında insan formunun hızıyla başla
        currentRotationSpeed = humanRotationSpeed;

        playerInput = new();
        playerInput.Player.Enable();
    }

    private void SetActiveCamera()
    {
        if (activeCamera == null) activeCamera = Camera.main;
    }

    private void SetCharacterController()
    {
        if (TryGetComponent<CharacterController>(out CharacterController cc))
        {
            this.characterController = cc;
            characterController.center = new(characterController.center.x, characterHeight / 2, characterController.center.z);
            characterController.height = characterHeight;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnAimStateChanged += HandleAimState;
        GameEvents.OnFormChangeStarted += LockMovementTemp;
        GameEvents.OnFormChanged += ChangeForm;
        GameEvents.OnWolfDashStarted += LockMovementTempD;
        GameEvents.OnWolfDashCompleted += UnlockMovement;
        GameEvents.OnWolfDragStateChanged += HandleDragSpeed;

        playerInput.Player.Move.performed += OnMovementPerformed;
        playerInput.Player.Move.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        GameEvents.OnAimStateChanged -= HandleAimState;
        GameEvents.OnFormChangeStarted -= LockMovementTemp;
        GameEvents.OnFormChanged -= ChangeForm;
        GameEvents.OnWolfDashStarted -= LockMovementTempD;
        GameEvents.OnWolfDashCompleted -= UnlockMovement;
        GameEvents.OnWolfDragStateChanged -= HandleDragSpeed;

        playerInput.Player.Move.performed -= OnMovementPerformed;
        playerInput.Player.Move.canceled -= OnMovementCanceled;
    }

    private void HandleAimState(bool state) => isAiming = state;
    private void LockMovementTemp(bool isWolf) => isMovementLocked = true;
    private void LockMovementTempD() => isMovementLocked = true;
    private void UnlockMovement(bool isWolf) => isMovementLocked = false;

    private void ChangeForm(bool isWolf)
    {
        UnlockMovement(isWolf);

        // ÇÖZÜM: Form değiştiği an fare (dönüş) hassasiyetini güncelle!
        currentRotationSpeed = isWolf ? wolfRotationSpeed : humanRotationSpeed;

        if (hasCharacterController)
        {
            if (isWolf)
            {
                characterController.height = wolfHeight;
                characterController.center = new(characterController.center.x, wolfHeight / 2, characterController.center.z);
                characterController.center += formOffset;
            }
            else
            {
                characterController.height = characterHeight;
                characterController.center = new(characterController.center.x, characterHeight / 2, characterController.center.z);
                characterController.center -= formOffset;
            }
        }
    }
    private void UnlockMovement() => isMovementLocked = false;

    private void HandleDragSpeed(bool isDragging)
    {
        isDraggingBaby = isDragging;
        speedMultiplier = isDragging ? 0.3f : 1.0f;
    }

    private void OnMovementPerformed(InputAction.CallbackContext context) => inputMoveVector = playerInput.Player.Move.ReadValue<Vector2>();
    private void OnMovementCanceled(InputAction.CallbackContext context) => inputMoveVector = Vector3.zero;

    void Update()
    {
        if (isMovementLocked) return;

        Move();
        if (hasCharacterController) ApplyGravity();
    }

    private void Move()
    {
        float xMoveInput = inputMoveVector.x;
        float yMoveInput = inputMoveVector.y;

        if (isDraggingBaby)
        {
            yMoveInput = Mathf.Clamp(yMoveInput, -1f, 0f); // W (İleri) iptal, sadece S, A, D çalışır
        }

        Vector3 camForward = activeCamera.transform.forward;
        Vector3 camRight = activeCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDir = (camForward * yMoveInput) + (camRight * xMoveInput);

        float currentSpeed = isAiming ? aimMoveSpeed : moveSpeed;

        if (movementDir != Vector3.zero)
        {
            GameEvents.OnMoved();
            if (hasCharacterController)
                characterController.Move(Time.deltaTime * currentSpeed * speedMultiplier * movementDir);
            else
                transform.Translate(Time.deltaTime * currentSpeed * speedMultiplier * movementDir, Space.World);
        }
        else
        {
            GameEvents.OnStoppedMoving();
        }

        // ROTASYON YÖNETİMİ (Sabit rotationSpeed yerine artık dinamik olan currentRotationSpeed devrede)
        if (isDraggingBaby)
        {
            if (camForward != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(camForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * currentRotationSpeed);
            }
        }
        else if (isAiming)
        {
            float angleDiff = Vector3.Angle(transform.forward, camForward);
            if (angleDiff > bodyRotationThreshold || movementDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(camForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * currentRotationSpeed * 1.5f);
            }
        }
        else if (movementDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(movementDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * currentRotationSpeed);
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}