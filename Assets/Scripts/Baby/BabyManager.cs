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

    [Header("Ground Snapping (Zemin Algılama)")]
    [SerializeField] private LayerMask groundLayer;

    private bool isCrying = false;
    private bool isTransitioning = false;
    private float meshPivotOffset = 0f;
    private Vector3 baseEulerAngles;

    private void Start()
    {
        baseEulerAngles = transform.eulerAngles;

        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
        {
            meshPivotOffset = transform.position.y - hit.point.y;
        }
    }

    private void OnEnable()
    {
        GameEvents.GetBabyTransform += GetMyTransform;
        GameEvents.OnTryNurseRequested += HandleNurseRequest;
        GameEvents.OnTryPickupRequested += HandlePickupRequest;
        GameEvents.OnTryDragRequested += HandleDragRequest;
        GameEvents.OnTryDropRequested += HandleDropRequest;
        GameEvents.OnBabyStolen += HandleStolenRequest;

        // Arkadaşının eklediği ses eventi bağlantısı
        GameEvents.OnBabyCrying += HandleBabyCrying;
    }

    private void OnDisable()
    {
        GameEvents.GetBabyTransform -= GetMyTransform;
        GameEvents.OnTryNurseRequested -= HandleNurseRequest;
        GameEvents.OnTryPickupRequested -= HandlePickupRequest;
        GameEvents.OnTryDragRequested -= HandleDragRequest;
        GameEvents.OnTryDropRequested -= HandleDropRequest;
        GameEvents.OnBabyStolen -= HandleStolenRequest;

        // Arkadaşının eklediği ses eventi bağlantısı
        GameEvents.OnBabyCrying -= HandleBabyCrying;
    }

    private Transform GetMyTransform() => transform;

    private void Update()
    {
        // 1. Açlığı Düşür ve Sınırla
        if (currentHunger > 0)
        {
            currentHunger -= hungerDepletionRate * Time.deltaTime;
        }
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        // 2. Her Update'te UI'a haber ver (Slider kodu koptu, BabyHungerUI.cs bunu dinleyecek)
        GameEvents.OnBabyHungerChanged(currentHunger, maxHunger);

        // 3. Ağlama Kontrolü
        if (currentHunger <= cryingThreshold && currentHunger > 0 && !isCrying)
        {
            isCrying = true;
            GameEvents.OnBabyCrying(true);
            GameEvents.OnShowHint("Bebek ağlıyor! Yırtıcıları çekmeden önce onu besle.", 4f);
        }
        else if (currentHunger > cryingThreshold && isCrying)
        {
            isCrying = false;
            GameEvents.OnBabyCrying(false);
        }

        // 4. Oyun Bitiş Kontrolü (Açlık 0 ise)
        else if (currentHunger <= 0 && currentState != BabyState.Stolen)
        {
            GameEvents.OnGameOver?.Invoke("Oğuz Bebek Açlıktan Öldü...");
        }
    }

    private void HandleStolenRequest(Transform enemyHand)
    {
        transform.SetParent(enemyHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(baseEulerAngles);

        currentState = BabyState.Stolen;
        GameEvents.OnBabyStateChanged(currentState);

        GameEvents.OnGameOver?.Invoke("Yelbegen Oğuz Bebeği Ormana Kaçırdı...");
    }

    // Arkadaşının eklediği harika ses metodu
    private void HandleBabyCrying(bool isNowCrying)
    {
        if (isNowCrying)
        {
            GameEvents.OnPlayOneShotSFX("BabyCry");
        }
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
        transform.localRotation = Quaternion.Euler(baseEulerAngles.x + 90f, baseEulerAngles.y, baseEulerAngles.z);

        currentState = BabyState.CarriedOnBack;
        GameEvents.OnBabyStateChanged(currentState);
        isTransitioning = false;
    }

    private void HandleDragRequest(Transform mouthPoint)
    {
        if (isTransitioning || currentState != BabyState.Dropped) return;
        transform.SetParent(mouthPoint, true);
        currentState = BabyState.CarriedInMouth;
        GameEvents.OnBabyStateChanged(currentState);
    }

    private void HandleDropRequest(Vector3 dropPosition)
    {
        if (isTransitioning || currentState == BabyState.Dropped) return;

        Vector3 finalDropPos = dropPosition;
        if (Physics.Raycast(dropPosition + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            finalDropPos.y = hit.point.y + meshPivotOffset;
        }

        if (currentState == BabyState.CarriedOnBack)
        {
            StartCoroutine(DropRoutine(finalDropPos));
        }
        else if (currentState == BabyState.CarriedInMouth || currentState == BabyState.Stolen)
        {
            transform.SetParent(null, true);
            transform.position = finalDropPos;
            transform.rotation = Quaternion.Euler(baseEulerAngles.x, transform.eulerAngles.y, baseEulerAngles.z);
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
        transform.rotation = Quaternion.Euler(baseEulerAngles.x, transform.eulerAngles.y, baseEulerAngles.z);

        currentState = BabyState.Dropped;
        GameEvents.OnBabyStateChanged(currentState);
        isTransitioning = false;
    }

    private void HandleNurseRequest(Vector3 playerPosition)
    {
        if (isTransitioning || currentState == BabyState.CarriedInMouth || currentState == BabyState.Stolen) return;

        if (currentState == BabyState.Dropped && Vector3.Distance(transform.position, playerPosition) > maxNurseDistance)
        {
            GameEvents.OnShowHint("Bebeği emzirmek için yeterince yakın değilsin.", 3f);
            return;
        }

        if (currentHunger >= nurseThreshold)
        {
            GameEvents.OnShowHint("Oğuz bebek şu an tok, emzirmeye gerek yok.", 3f);
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

        // BİZİM EKLEDİĞİMİZ KISIM: Emzirme bittiğinde barın anında dolduğunu görmek için eventi bir kez daha fırlatıyoruz
        GameEvents.OnBabyHungerChanged(currentHunger, maxHunger);

        GameEvents.OnBabyNurseCompleted();
        isTransitioning = false;
    }
}