using System.Collections;
using UnityEngine;

public class BabyManager : MonoBehaviour
{
    [Header("State Settings")]
    public BabyState currentState = BabyState.Dropped;

    [Header("Hunger Settings")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;
    [SerializeField] private float hungerDepletionRate = 1.5f; // Saniyede ne kadar acýkacak
    [SerializeField] private float nurseThreshold = 60f; // Hangi seviyenin altýndaysa emzirilebilir
    [SerializeField] private float cryingThreshold = 30f; // Hangi seviyenin altýndaysa aðlar
    [SerializeField] private float nurseRestoreAmount = 50f; // Bir emzirmede ne kadar doyacak

    [Header("Interaction Rules")]
    [SerializeField] private float maxNurseDistance = 2.5f; // Yerdeyken emzirebilmek için maksimum mesafe

    [Header("Animation Durations (Simulation)")]
    [Tooltip("Animasyon bitene kadar objenin sýrta geçme süresi")]
    [SerializeField] private float pickupAnimDuration = 1.2f;
    [SerializeField] private float dropAnimDuration = 1.0f;
    [SerializeField] private float nurseAnimDuration = 3.0f;

    private bool isCrying = false;
    private bool isTransitioning = false; 

    private void OnEnable()
    {
        GameEvents.OnTryNurseRequested += HandleNurseRequest;
        GameEvents.OnTryPickupRequested += HandlePickupRequest;
        GameEvents.OnTryDropRequested += HandleDropRequest;
    }

    private void OnDisable()
    {
        GameEvents.OnTryNurseRequested -= HandleNurseRequest;
        GameEvents.OnTryPickupRequested -= HandlePickupRequest;
        GameEvents.OnTryDropRequested -= HandleDropRequest;
    }

    private void Update()
    {
        if (currentHunger > 0)
        {
            currentHunger -= hungerDepletionRate * Time.deltaTime;
            currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
            GameEvents.OnBabyHungerChanged(currentHunger, maxHunger);
        }

        // Aðlama kontrolü
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
    }

    #region Drop & Pickup Logic
    private void HandlePickupRequest(Transform playerBackMountPoint)
    {
        if (isTransitioning || currentState != BabyState.Dropped) return;

        StartCoroutine(PickupRoutine(playerBackMountPoint));
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

    private void HandleDropRequest(Vector3 dropPosition)
    {
        if (isTransitioning || currentState == BabyState.Dropped) return;

        StartCoroutine(DropRoutine(dropPosition));
    }

    private IEnumerator DropRoutine(Vector3 dropPosition)
    {
        isTransitioning = true;
        GameEvents.OnBabyDropStarted();

        yield return new WaitForSeconds(dropAnimDuration);

        transform.SetParent(null); // Sýrttan ayýr
        transform.position = dropPosition; // Karakterin önüne koy

        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        currentState = BabyState.Dropped;
        GameEvents.OnBabyStateChanged(currentState);
        isTransitioning = false;
    }
    #endregion

    #region Nursing Logic
    private void HandleNurseRequest(Vector3 playerPosition)
    {
        if (isTransitioning) return;

        if (currentState == BabyState.Dropped)
        {
            float distance = Vector3.Distance(transform.position, playerPosition);
            if (distance > maxNurseDistance)
            {
                GameEvents.OnShowHint("Bebeði emzirmek için yeterince yakýn deðilsin.", 3f);
                return;
            }
        }

        // 2. Kural: Bebek gerçekten aç mý?
        if (currentHunger >= nurseThreshold)
        {
            GameEvents.OnShowHint("Oðuz bebek þu an tok, emzirmeye gerek yok.", 3f);
            return;
        }

        // Eðer tüm þartlar uygunsa emzirme iþlemini baþlat
        StartCoroutine(NurseRoutine());
    }

    private IEnumerator NurseRoutine()
    {
        isTransitioning = true;
        GameEvents.OnBabyNurseStarted(); // Ýlgili animasyon ve bebek emme ses efektleri baþlar

        yield return new WaitForSeconds(nurseAnimDuration);

        currentHunger += nurseRestoreAmount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        GameEvents.OnBabyNurseCompleted(); // Karakter normal haline döner
        isTransitioning = false;
    }
    #endregion
}