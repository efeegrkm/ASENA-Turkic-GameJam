using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineInputAxisController))]
public class ShoulderCamManager : MonoBehaviour
{
    [Header("--- Core References ---")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [Tooltip("Kameraya eklediğiniz Cinemachine Camera Offset bileşenini buraya sürükleyin.")]
    [SerializeField] private CinemachineCameraOffset cameraOffset;

    [Header("--- FOV Settings ---")]
    [Tooltip("Default FOV value (Automatically obtained from camera if left empty)")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float aimFOV = 40f;

    [Header("--- Shoulder Aim Lerp Settings ---")]
    [Tooltip("Karakterin omzuna hizalanacak offset mesafesi (X: Sağ-Sol, Y: Yukarı-Aşağı, Z: İleri-Geri)")]
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.6f, -0.2f, -0.8f);
    [Tooltip("Omza geçiş ve eski haline dönme pürüzsüzlük süresi (Saniye)")]
    [SerializeField] private float lerpDuration = 0.3f;

    [Header("--- Orbit Settings ---")]
    [SerializeField] private GameObject defaultOrbitTarget;
    [Range(1f, 20f)]
    [SerializeField] private float defaultOrbitDuration = 8f;
    [SerializeField] private float defaultOrbitSpeed = 60f;
    [Range(-80f, 80f)]
    [SerializeField] private float defaultTargetVerticalAngle = 45f;
    [Tooltip("Time required to reach vertical angle")]
    [SerializeField] private float verticalLerpDuration = 1.0f;
    [Tooltip("Time required for input to be reenabled after sequence ends")]
    [SerializeField] private float inputReEnableDelay = 1.5f;

    [Header("--- Aim Sensitivity ---")]
    [Tooltip("Nişan alırken mouse hızının ne kadar yavaşlayacağı")]
    [SerializeField] private float aimSensitivityMultiplier = 0.4f;

    [Header("--- Form Tracking Targets ---")]
    [Tooltip("İnsan formunda Cinemachine'in takip edeceği kafa objesi (Human Head transform)")]
    [SerializeField] private Transform humanTrackingTarget;
    [Tooltip("Kurt formunda Cinemachine'in takip edeceği kafa objesi (Wolf Head transform)")]
    [SerializeField] private Transform wolfTrackingTarget;

    // ─────────────────────────────────────────────
    // Private State
    // ─────────────────────────────────────────────
    private float originalFOV;
    private Vector3 originalOffset;
    private Coroutine transitionCoroutine;
    private Coroutine orbitCoroutine;
    private CinemachineInputAxisController cinemachineInput;

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        cinemachineInput = GetComponent<CinemachineInputAxisController>();
        if (cinemachineInput == null && cinemachineCamera != null)
        {
            cinemachineInput = cinemachineCamera.GetComponent<CinemachineInputAxisController>();
        }
        InitializeCameraValues();
    }

    private void OnEnable()
    {
        GameEvents.OnIncreaseFOVTo += IncreaseFOVTo;
        GameEvents.OnResetFOV += ResetFOV;
        GameEvents.OnStartDefaultOrbit += StartOrbitSequence;
        GameEvents.OnStartCustomOrbit += StartOrbitSequence;
        GameEvents.OnToggleAimCamera += ToggleAim;
        GameEvents.OnFormChanged += HandleFormChanged;   // YENİ
    }

    private void OnDisable()
    {
        GameEvents.OnIncreaseFOVTo -= IncreaseFOVTo;
        GameEvents.OnResetFOV -= ResetFOV;
        GameEvents.OnStartDefaultOrbit -= StartOrbitSequence;
        GameEvents.OnStartCustomOrbit -= StartOrbitSequence;
        GameEvents.OnToggleAimCamera -= ToggleAim;
        GameEvents.OnFormChanged -= HandleFormChanged;   // YENİ
    }

    private void InitializeCameraValues()
    {
        if (cinemachineCamera != null)
        {
            originalFOV = (defaultFOV > 0) ? defaultFOV : cinemachineCamera.Lens.FieldOfView;
        }

        if (cameraOffset == null && cinemachineCamera != null)
        {
            cameraOffset = cinemachineCamera.GetComponent<CinemachineCameraOffset>();
        }

        if (cameraOffset != null)
        {
            originalOffset = cameraOffset.Offset;
        }
        else
        {
            originalOffset = Vector3.zero;
            Debug.LogWarning("CinemachineCameraOffset bileşeni bulunamadı! Lütfen CinemachineCamera'ya ekleyin.");
        }
    }

    // ─────────────────────────────────────────────
    // Form Change — Tracking Target
    // ─────────────────────────────────────────────

    /// <summary>
    /// FormManager'ın TransformRoutine'i bittiğinde (modeller swap olduktan sonra)
    /// GameEvents.OnFormChanged(isWolf) ile tetiklenir.
    /// Cinemachine'in TrackingTarget ve LookAtTarget'ını aktif forma göre günceller.
    /// </summary>
    private void HandleFormChanged(bool isWolf)
    {
        if (cinemachineCamera == null) return;

        Transform newTarget = isWolf ? wolfTrackingTarget : humanTrackingTarget;

        if (newTarget == null)
        {
            Debug.LogWarning($"ShoulderCamManager: {'{'}{(isWolf ? "Wolf" : "Human")}TrackingTarget{'}'} atanmamış!");
            return;
        }

        cinemachineCamera.Target.TrackingTarget = newTarget;
        cinemachineCamera.Target.LookAtTarget = newTarget;
    }

    // ─────────────────────────────────────────────
    // Aim
    // ─────────────────────────────────────────────

    private void ToggleAim(bool isAiming)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        float targetFOV = isAiming ? aimFOV : originalFOV;
        Vector3 targetOffset = isAiming ? shoulderOffset : originalOffset;

        transitionCoroutine = StartCoroutine(AimTransitionRoutine(targetOffset, targetFOV));

        if (cinemachineInput != null && cinemachineInput.Controllers != null)
        {
            foreach (var controller in cinemachineInput.Controllers)
            {
                if (controller.Input != null)
                {
                    controller.Input.Gain = isAiming
                        ? controller.Input.Gain * aimSensitivityMultiplier
                        : controller.Input.Gain / aimSensitivityMultiplier;
                }
            }
        }
    }

    private IEnumerator AimTransitionRoutine(Vector3 targetOffset, float targetFOV)
    {
        if (cinemachineCamera == null || cameraOffset == null) yield break;

        Vector3 startOffset = cameraOffset.Offset;
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / lerpDuration);

            cameraOffset.Offset = Vector3.Lerp(startOffset, targetOffset, t);

            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            cinemachineCamera.Lens = lens;

            yield return null;
        }

        cameraOffset.Offset = targetOffset;
        var finalLens = cinemachineCamera.Lens;
        finalLens.FieldOfView = targetFOV;
        cinemachineCamera.Lens = finalLens;
    }

    // ─────────────────────────────────────────────
    // Old FOV Fallbacks
    // ─────────────────────────────────────────────

    #region Old FOV Methods Fallbacks
    private void IncreaseFOVTo(float targetFOV)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        if (cameraOffset != null)
        {
            transitionCoroutine = StartCoroutine(AimTransitionRoutine(cameraOffset.Offset, targetFOV));
        }
    }

    private void ResetFOV()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        if (cameraOffset != null)
        {
            transitionCoroutine = StartCoroutine(AimTransitionRoutine(cameraOffset.Offset, originalFOV));
        }
    }
    #endregion

    // ─────────────────────────────────────────────
    // Orbit
    // ─────────────────────────────────────────────

    #region Orbit Methods
    private void StartOrbitSequence()
    {
        if (defaultOrbitTarget == null) return;
        StartOrbitSequence(defaultOrbitTarget, defaultOrbitDuration, defaultOrbitSpeed, defaultTargetVerticalAngle);
    }

    private void StartOrbitSequence(GameObject target, float duration, float rotationSpeed, float vAngle)
    {
        if (orbitCoroutine != null) StopCoroutine(orbitCoroutine);
        orbitCoroutine = StartCoroutine(OrbitRoutine(target, duration, rotationSpeed, vAngle));
    }

    private IEnumerator OrbitRoutine(GameObject target, float duration, float speed, float vAngle)
    {
        if (cinemachineCamera == null || orbitalFollow == null) yield break;

        if (cinemachineInput != null) cinemachineInput.enabled = false;

        Transform originalFollow = cinemachineCamera.Target.TrackingTarget;
        Transform originalLookAt = cinemachineCamera.Target.LookAtTarget;

        cinemachineCamera.Target.TrackingTarget = target.transform;
        cinemachineCamera.Target.LookAtTarget = target.transform;

        float elapsed = 0f;
        float startVAngle = orbitalFollow.VerticalAxis.Value;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            orbitalFollow.HorizontalAxis.Value += speed * Time.deltaTime;

            float vT = Mathf.Clamp01(elapsed / verticalLerpDuration);
            orbitalFollow.VerticalAxis.Value = Mathf.Lerp(startVAngle, vAngle, Mathf.SmoothStep(0, 1, vT));

            yield return null;
        }

        cinemachineCamera.Target.TrackingTarget = originalFollow;
        cinemachineCamera.Target.LookAtTarget = originalLookAt;

        yield return new WaitForSeconds(inputReEnableDelay);

        if (cinemachineInput != null) cinemachineInput.enabled = true;
    }
    #endregion
}