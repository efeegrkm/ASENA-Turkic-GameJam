using UnityEngine;
using UnityEngine.InputSystem;

public class BowController : MonoBehaviour
{
    [Header("Bow Settings")]
    [SerializeField] private int currentArrows = 5;
    [SerializeField] private float maxChargeTime = 1.0f;
    [SerializeField] private float minChargeTime = 0.2f;

    [Header("Aim & Raycast Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private float aimDistance = 100f;
    [SerializeField] private LayerMask aimMask;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float maxShootForce = 40f;

    [Header("Input Dependencies")]
    [SerializeField] private InputActionReference aimAndShootAction;
    [SerializeField] private InputActionReference cancelAction;

    private bool isDrawing = false;
    private float drawTimer = 0f;
    private Vector3 currentAimPoint;

    private bool hasShownCancelHint = false;
    private bool hasShownPickupHint = false;
    private BabyState currentBabyState = BabyState.Dropped;

    private void OnEnable()
    {
        if (aimAndShootAction != null) aimAndShootAction.action.Enable();
        if (cancelAction != null) cancelAction.action.Enable();

        aimAndShootAction.action.started += OnDrawStarted;
        aimAndShootAction.action.canceled += OnDrawReleased;
        cancelAction.action.performed += OnCancelAim;
        GameEvents.OnBabyStateChanged += UpdateBabyState;
    }

    private void OnDisable()
    {
        CancelAimingProcess();

        aimAndShootAction.action.started -= OnDrawStarted;
        aimAndShootAction.action.canceled -= OnDrawReleased;
        cancelAction.action.performed -= OnCancelAim;
        GameEvents.OnBabyStateChanged -= UpdateBabyState;
    }

    private void Start()
    {
        GameEvents.OnArrowCountChanged(currentArrows);
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isDrawing)
        {
            drawTimer += Time.deltaTime;
            UpdateAimAndBowRotation();
        }
    }

    private void UpdateBabyState(BabyState state)
    {
        currentBabyState = state;

        if (isDrawing && currentBabyState == BabyState.CarriedOnBack)
        {
            CancelAimingProcess();
            GameEvents.OnBowDrawCanceled();
        }
    }

    private void UpdateAimAndBowRotation()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, aimDistance, aimMask))
        {
            currentAimPoint = hit.point;
        }
        else
        {
            currentAimPoint = ray.GetPoint(aimDistance);
        }

        if (weaponHolder != null)
        {
            weaponHolder.LookAt(currentAimPoint);
        }
    }

    private void OnDrawStarted(InputAction.CallbackContext context)
    {
        if (currentArrows <= 0)
        {
            GameEvents.OnShowHint("Ok kalmadý...", 3f);
            return;
        }
        if (currentBabyState == BabyState.CarriedOnBack)
        {
            GameEvents.OnShowHint("Oðuzu taþýrken yay kullanamazsýn! Bebeði býrakmak için G tuþuna bas", 3f);
            return;
        }

        isDrawing = true;
        drawTimer = 0f;

        GameEvents.OnAimStateChanged(true);
        GameEvents.OnBowDrawStarted();
        GameEvents.OnCrosshairVisibilityChanged(true);
        GameEvents.OnToggleAimCamera(true);

        if (!hasShownCancelHint)
        {
            GameEvents.OnShowHint("Niþan almayý iptal etmek için [SPACE] tuþuna bas", 4f);
            hasShownCancelHint = true;
        }
    }

    private void OnCancelAim(InputAction.CallbackContext context)
    {
        if (isDrawing)
        {
            CancelAimingProcess();
            GameEvents.OnBowDrawCanceled();
        }
    }

    private void OnDrawReleased(InputAction.CallbackContext context)
    {
        if (!isDrawing) return;

        if (drawTimer >= minChargeTime)
        {
            Shoot();
        }
        else
        {
            GameEvents.OnBowDrawCanceled();
        }

        CancelAimingProcess();
    }

    private void CancelAimingProcess()
    {
        isDrawing = false;

        GameEvents.OnAimStateChanged(false);
        GameEvents.OnCrosshairVisibilityChanged(false);
        GameEvents.OnToggleAimCamera(false);

        if (weaponHolder != null)
            weaponHolder.localRotation = Quaternion.identity;
    }

    private void Shoot()
    {
        currentArrows--;
        GameEvents.OnArrowCountChanged(currentArrows);

        float chargeRatio = Mathf.Clamp01(drawTimer / maxChargeTime);

        GameEvents.OnBowShooted(chargeRatio);
        GameEvents.OnPlayOneShotSFX("BowShoot");

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            Vector3 shootDirection = (currentAimPoint - arrowSpawnPoint.position).normalized;

            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(shootDirection));

            if (arrow.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(shootDirection * (maxShootForce * chargeRatio), ForceMode.Impulse);
            }
        }

        if (!hasShownPickupHint)
        {
            GameEvents.OnShowHint("Attýðýn okun yanýna gidip onu geri toplayabilirsin", 5f);
            hasShownPickupHint = true;
        }
    }
}