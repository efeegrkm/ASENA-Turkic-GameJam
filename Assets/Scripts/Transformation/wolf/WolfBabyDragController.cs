using UnityEngine;
using UnityEngine.InputSystem;

public class WolfBabyDragController : MonoBehaviour
{
    [Header("Drag Inputs")]
    [Tooltip("Puseti tutmak için basýlacak tuþ (E - Interact)")]
    [SerializeField] private InputActionReference dragAction;
    [Tooltip("Puseti býrakmak için basýlacak tuþ (G - Drop)")]
    [SerializeField] private InputActionReference dropAction;

    [Header("References")]
    [SerializeField] private Transform mouthMountPoint;
    [SerializeField] private float grabRange = 2.5f;

    private bool isWolf = false;
    private bool isDragging = false;
    private BabyState currentBabyState = BabyState.Dropped;

    // ÇÖZÜM 1: Oyuncuya rehberlik edecek Hint için kontrol deðiþkeni
    private bool hasShownDragHint = false;

    private void OnEnable()
    {
        GameEvents.OnFormChanged += UpdateForm;
        GameEvents.OnBabyStateChanged += HandleBabyStateChanged;

        if (dragAction != null) dragAction.action.Enable();
        if (dropAction != null) dropAction.action.Enable();

        dragAction.action.started += OnDragStarted;
        dropAction.action.started += OnDropPerformed;
    }

    private void OnDisable()
    {
        GameEvents.OnFormChanged -= UpdateForm;
        GameEvents.OnBabyStateChanged -= HandleBabyStateChanged;

        dragAction.action.started -= OnDragStarted;
        dropAction.action.started -= OnDropPerformed;
    }

    private void UpdateForm(bool wolfState) => isWolf = wolfState;

    private void HandleBabyStateChanged(BabyState state)
    {
        currentBabyState = state;

        if (isDragging && state == BabyState.Stolen)
        {
            isDragging = false;
            GameEvents.OnWolfDragStateChanged(false);
        }
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        if (!isWolf || currentBabyState != BabyState.Dropped || isDragging) return;

        if (GameEvents.GetBabyTransform != null)
        {
            Transform baby = GameEvents.GetBabyTransform();
            if (Vector3.Distance(transform.position, baby.position) > grabRange)
            {
                GameEvents.OnShowHint("Puseti tutmak için yeterince yakýn deðilsin.", 3f);
                return;
            }
        }

        // Ýlk defa çekiyorsa Hint göster
        if (!hasShownDragHint)
        {
            GameEvents.OnShowHint("Puseti [S] tuþuna basýlý tutup MOUSE ile yönlendirerek çekebilirsin.", 5f);
            hasShownDragHint = true;
        }

        GameEvents.OnTryDragRequested(mouthMountPoint);
        isDragging = true;
        GameEvents.OnWolfDragStateChanged(true);
    }

    private void OnDropPerformed(InputAction.CallbackContext context)
    {
        if (!isDragging) return;

        isDragging = false;
        GameEvents.OnWolfDragStateChanged(false);

        // ÇÖZÜM 2: Ayaklara deðil, tam olarak aðzýmýzýn (puseti tuttuðumuz yerin) koordinatýna býrak!
        GameEvents.OnTryDropRequested(mouthMountPoint.position);
    }
}