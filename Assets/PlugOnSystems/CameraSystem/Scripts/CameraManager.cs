using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraManager : MonoBehaviour
{
    [Header("--- Core References ---")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    [Header("--- FOV Settings ---")]
    [Tooltip("FOV transition speed")]
    [SerializeField] private float transitionDuration = 1.5f;
    [Tooltip("Default FOV value (Automatically obtained from camera if left empty)")]
    [SerializeField] private float defaultFOV = 60f;

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

    private float originalFOV;
    private Coroutine fovCoroutine;
    private Coroutine orbitCoroutine;
    private CinemachineInputAxisController cinemachineInput;

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
    }

    private void OnDisable()
    {
        GameEvents.OnIncreaseFOVTo -= IncreaseFOVTo;
        GameEvents.OnResetFOV -= ResetFOV;
        GameEvents.OnStartDefaultOrbit -= StartOrbitSequence;
        GameEvents.OnStartCustomOrbit -= StartOrbitSequence;
    }

    private void InitializeCameraValues()
    {
        if (cinemachineCamera != null)
        {
            originalFOV = (defaultFOV > 0) ? defaultFOV : cinemachineCamera.Lens.FieldOfView;
        }
    }

    #region FOV Methods
    private void IncreaseFOVTo(float targetFOV)
    {
        StopAndStartFOV(targetFOV);
    }

    private void ResetFOV()
    {
        StopAndStartFOV(originalFOV);
    }

    private void StopAndStartFOV(float target)
    {
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(ChangeFOVRoutine(target));
    }

    private IEnumerator ChangeFOVRoutine(float targetFOV)
    {
        if (cinemachineCamera == null) yield break;
        float startFOV = cinemachineCamera.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            cinemachineCamera.Lens = lens;

            yield return null;
        }

        var finalLens = cinemachineCamera.Lens;
        finalLens.FieldOfView = targetFOV;
        cinemachineCamera.Lens = finalLens;
    }
    #endregion

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