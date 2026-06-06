using System.Collections;
using UnityEngine;

public class BabyManager : MonoBehaviour
{
    [Header("State Settings")]
    public BabyState currentState = BabyState.Dropped;

    [Header("Hunger Settings")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float hungerDepletionRate = 1.5f;
    [SerializeField] private float nurseThreshold = 60f;
    [SerializeField] private float cryingThreshold = 30f;
    [SerializeField] private float nurseRestoreAmount = 50f;

    [Header("Interaction Rules")]
    [SerializeField] private float maxNurseDistance = 2.5f;

    [Header("Animation Durations")]
    [SerializeField] private float pickupAnimDuration = 1.2f;
    [SerializeField] private float dropAnimDuration = 1.0f;
    [SerializeField] private float nurseAnimDuration = 3.0f;

    private bool isCrying = false;
    private bool isTransitioning = false;

    private void OnEnable()
    {
        GameEvents.GetBabyTransform += GetMyTransform;
        GameEvents.OnTryNurseRequested += HandleNurseRequest;
        GameEvents.OnTryPickupRequested += HandlePickupRequest;
        GameEvents.OnTryDragRequested += HandleDragRequest;
        GameEvents.OnTryDropRequested += HandleDropRequest;
    }

    private void OnDisable()
    {
        GameEvents.GetBabyTransform -= GetMyTransform;
        GameEvents.OnTryNurseRequested -= HandleNurseRequest;
        GameEvents.OnTryPickupRequested -= HandlePickupRequest;
        GameEvents.OnTryDragRequested -= HandleDragRequest;
        GameEvents.OnTryDropRequested -= HandleDropRequest;
    }

    private Transform GetMyTransform() => transform;

    private void Update()
    {
        if (currentHunger > 0)
        {
            currentHunger -= hungerDepletionRate * Time.deltaTime;
            currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
            GameEvents.OnBabyHungerChanged(currentHunger, maxHunger);
        }

        if (currentHunger <= cryingThreshold && !isCrying)
        {
            isCrying = true;
            GameEvents.OnBabyCrying(true);
            GameEvents.OnShowHint("Bebek aðlýyor! Yýrtýcýlarý çekmeden önce onu besle.", 4f);
        }
        else if (currentHunger > cryingThreshold && isCrying)
        {
            isCrying = false;
            GameEvents.OnBabyCrying(false);
        }

        // Fýldýr fýldýr dönme kontrolünü sildik çünkü SetParent(true) ile obje kendi fiziðini koruyacak.
    }

    private void HandlePickupRequest(Transform mountPoint)
    {
        if (isTransitioning || currentState != BabyState.Dropped) return;
        StartCoroutine(PickupRoutine(mountPoint));
    }

    private IEnumerator PickupRoutine(Transform mountPoint)
    {
        isTransitioning = true;
        GameEvents.OnBabyPickupStarted();

        yield return new WaitForSeconds(pickupAnimDuration);

        transform.SetParent(mountPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        currentState = BabyState.CarriedOnBack;
        GameEvents.OnBabyStateChanged(currentState);
        isTransitioning = false;
    }

    // HATA 4 ÇÖZÜMÜ: Kurdun puseti tuttuðu yer. Transform ellenmiyor.
    private void HandleDragRequest(Transform mouthPoint)
    {
        if (isTransitioning || currentState != BabyState.Dropped) return;

        // Parent yap, ancak dünya üzerindeki pozisyonunu ve rotasyonunu (true parametresi ile) koru.
        transform.SetParent(mouthPoint, true);

        currentState = BabyState.CarriedInMouth;
        GameEvents.OnBabyStateChanged(currentState);
    }

    private void HandleDropRequest(Vector3 dropPosition)
    {
        if (isTransitioning || currentState == BabyState.Dropped) return;

        if (currentState == BabyState.CarriedOnBack)
        {
            StartCoroutine(DropRoutine(dropPosition));
        }
        else if (currentState == BabyState.CarriedInMouth)
        {
            // HATA 4 ÇÖZÜMÜ: Sadece parent'ý kaldýr. Pozisyon/Rotasyon oynama.
            transform.SetParent(null, true);

            currentState = BabyState.Dropped;
            GameEvents.OnBabyStateChanged(currentState);
        }
    }

    private IEnumerator DropRoutine(Vector3 dropPosition)
    {
        isTransitioning = true;
        GameEvents.OnBabyDropStarted();

        yield return new WaitForSeconds(dropAnimDuration);

        transform.SetParent(null);
        transform.position = dropPosition;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        currentState = BabyState.Dropped;
        GameEvents.OnBabyStateChanged(currentState);
        isTransitioning = false;
    }

    private void HandleNurseRequest(Vector3 playerPosition)
    {
        if (isTransitioning || currentState == BabyState.CarriedInMouth) return;

        if (currentState == BabyState.Dropped)
        {
            if (Vector3.Distance(transform.position, playerPosition) > maxNurseDistance)
            {
                GameEvents.OnShowHint("Bebeði emzirmek için yeterince yakýn deðilsin.", 3f);
                return;
            }
        }

        if (currentHunger >= nurseThreshold)
        {
            GameEvents.OnShowHint("Oðuz bebek þu an tok, emzirmeye gerek yok.", 3f);
            return;
        }
        StartCoroutine(NurseRoutine());
    }

    private IEnumerator NurseRoutine()
    {
        isTransitioning = true;
        GameEvents.OnBabyNurseStarted();
        yield return new WaitForSeconds(nurseAnimDuration);
        currentHunger += nurseRestoreAmount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        GameEvents.OnBabyNurseCompleted();
        isTransitioning = false;
    }
}