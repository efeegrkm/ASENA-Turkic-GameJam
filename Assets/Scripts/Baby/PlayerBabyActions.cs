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
    [SerializeField] private InputActionReference nurseAction;
    [SerializeField] private InputActionReference dropAction;

    private BabyState currentBabyState = BabyState.Dropped;
    private bool isActionLocked = false;

    private void OnEnable()
    {
        GameEvents.OnBabyStateChanged += UpdateBabyState;

        GameEvents.OnBabyNurseStarted += LockActions;
        GameEvents.OnBabyNurseCompleted += UnlockActions;
        GameEvents.OnBabyPickupStarted += LockActions;
        GameEvents.OnBabyDropStarted += LockActions;

        if (nurseAction != null) nurseAction.action.Enable();
        if (dropAction != null) dropAction.action.Enable();

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

        // BURADAKÝ .Disable() SATIRLARI SÝLÝNDÝ (G Tuþu artýk global olarak ölmeyecek)
        nurseAction.action.performed -= TryNurse;
        dropAction.action.performed -= TryDrop;
    }

    private void UpdateBabyState(BabyState newState)
    {
        currentBabyState = newState;
        UnlockActions();
    }

    private void LockActions() => isActionLocked = true;
    private void UnlockActions() => isActionLocked = false;

    private void TryNurse(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || isActionLocked) return;
        GameEvents.OnTryNurseRequested(transform.position);
    }

    private void TryDrop(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || isActionLocked || currentBabyState == BabyState.Dropped) return;

        Vector3 targetDropPos = transform.position + transform.TransformDirection(dropOffset);
        GameEvents.OnTryDropRequested(targetDropPos);
    }

    public void TryPickup()
    {
        if (!isActiveAndEnabled || isActionLocked || currentBabyState != BabyState.Dropped) return;
        GameEvents.OnTryPickupRequested(backMountPoint);
    }
}