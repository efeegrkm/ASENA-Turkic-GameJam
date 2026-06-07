using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanDodgeController : MonoBehaviour
{
    [Header("Dodge (Kite) Settings")]
    [SerializeField] private float dodgeForce = 12f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float dodgeCooldown = 2f;

    [Header("Inputs")]
    [Tooltip("Shift (Dash) tuþunu buraya sürükleyin")]
    [SerializeField] private InputActionReference dodgeAction;

    private bool isWolf = false;
    private bool isActionLocked = false;
    private float lastDodgeTime = -10f;

    private CharacterController characterController;
    private Camera mainCamera;

    private void Awake()
    {
        characterController = GetComponentInParent<CharacterController>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        isActionLocked = false;
        GameEvents.OnFormChanged += UpdateForm;
        GameEvents.OnFormChangeStarted += LockActionsTemporarily;

        if (dodgeAction != null) dodgeAction.action.Enable();
        dodgeAction.action.performed += TryDodge;
    }

    private void OnDisable()
    {
        GameEvents.OnFormChanged -= UpdateForm;
        GameEvents.OnFormChangeStarted -= LockActionsTemporarily;

        dodgeAction.action.performed -= TryDodge;
    }

    private void UpdateForm(bool wolfState) => isWolf = wolfState;
    private void LockActionsTemporarily(bool becomingWolf) => isActionLocked = true;

    private void TryDodge(InputAction.CallbackContext context)
    {
        if (isWolf || isActionLocked || Time.time < lastDodgeTime + dodgeCooldown) return;

        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine()
    {
        isActionLocked = true;
        lastDodgeTime = Time.time;

        GameEvents.OnWolfDashStarted();

        // Kameranýn baktýðý yönün TAM TERSÝNÝ hesapla (Saf Kite Mantýðý)
        Vector3 dodgeDirection = -mainCamera.transform.forward;
        dodgeDirection.y = 0f;
        dodgeDirection.Normalize();

        float elapsed = 0f;
        float verticalVelocity = jumpHeight;
        float gravity = -20f;

        while (elapsed < dodgeDuration)
        {
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 currentMove = (dodgeDirection * dodgeForce) + (Vector3.up * verticalVelocity);

            if (characterController != null)
                characterController.Move(currentMove * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        GameEvents.OnWolfDashCompleted();
        isActionLocked = false;
    }
}