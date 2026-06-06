using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBabyActions : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Bebeðin sýrta yapýþacaðý Transform (Örn: Spine veya Neck kemiði)")]
    [SerializeField] private Transform backMountPoint;
    [Tooltip("Bebeðin yere býrakýlacaðý konum offseti (Karakterin biraz önü)")]
    [SerializeField] private Vector3 dropOffset = new Vector3(0, 0, 1f);

    [Header("Input Actions")]
    [SerializeField] private InputActionReference nurseAction; //'F' Tuþu
    [SerializeField] private InputActionReference dropAction;  //'G' veya 'Q' Tuþu

    private BabyState currentBabyState = BabyState.Dropped;
    private bool isActionLocked = false;

    private void OnEnable()
    {
        GameEvents.OnBabyStateChanged += UpdateBabyState;

        // Animasyonlar oynarken inputlarý/hareketi kitlemek için
        GameEvents.OnBabyNurseStarted += LockActions;
        GameEvents.OnBabyNurseCompleted += UnlockActions;
        GameEvents.OnBabyPickupStarted += LockActions;
        GameEvents.OnBabyDropStarted += LockActions;

        nurseAction.action.performed += TryNurse;
        dropAction.action.performed += TryDrop;
    }

    private void OnDisable()
    {
        GameEvents.OnBabyStateChanged -= UpdateBabyState;

        GameEvents.OnBabyNurseStarted -= LockActions;
        GameEvents.OnBabyNurseCompleted -= UnlockActions;
        GameEvents.OnBabyPickupStarted -= LockActions;
        GameEvents.OnBabyDropStarted -= LockActions;

        nurseAction.action.performed -= TryNurse;
        dropAction.action.performed -= TryDrop;
    }

    private void UpdateBabyState(BabyState newState)
    {
        currentBabyState = newState;
        UnlockActions(); // Animasyon bittiðinde ve state deðiþtiðinde kilidi aç
    }

    private void LockActions() => isActionLocked = true;
    private void UnlockActions() => isActionLocked = false;

    private void TryNurse(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || isActionLocked) return;

        // Oyuncu F'ye bastýðýnda global sisteme pozisyonunu yollayarak "Emzirmeyi dene" der.
        GameEvents.OnTryNurseRequested(transform.position);
    }

    private void TryDrop(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || isActionLocked || currentBabyState == BabyState.Dropped) return;

        Vector3 targetDropPos = transform.position + transform.TransformDirection(dropOffset);
        GameEvents.OnTryDropRequested(targetDropPos);
    }

    // Bebeði yerden almak için Interact sistemin bunu tetikleyecek
    public void TryPickup()
    {
        // HATA ÇÖZÜMÜ: Eðer bu obje (Ýnsan Formu) kapalýysa, dýþarýdan gelen etkileþimleri reddet!
        if (!isActiveAndEnabled || isActionLocked || currentBabyState != BabyState.Dropped) return;

        // Sýrt montaj noktasýný gönderiyoruz ki bebek oraya snaplensin
        GameEvents.OnTryPickupRequested(backMountPoint);
    }
}