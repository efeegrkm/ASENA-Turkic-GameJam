using UnityEngine;
using UnityEngine.InputSystem;

public class WolfBabyDragController : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private InputActionReference dragAction;
    [SerializeField] private Transform mouthMountPoint;
    [SerializeField] private float grabRange = 2.5f;

    private bool isWolf = false;
    private bool isDragging = false;
    private BabyState currentBabyState = BabyState.Dropped;

    private void OnEnable()
    {
        GameEvents.OnFormChanged += (state) => isWolf = state;
        GameEvents.OnBabyStateChanged += (state) => currentBabyState = state;

        if (dragAction != null) dragAction.action.Enable();

        dragAction.action.started += OnDragStarted;
        dragAction.action.canceled += OnDragCanceled;
    }

    private void OnDisable()
    {
        if (dragAction != null) dragAction.action.Disable();

        dragAction.action.started -= OnDragStarted;
        dragAction.action.canceled -= OnDragCanceled;
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        if (!isWolf || currentBabyState != BabyState.Dropped) return;

        if (GameEvents.GetBabyTransform != null)
        {
            Transform baby = GameEvents.GetBabyTransform();
            if (Vector3.Distance(transform.position, baby.position) > grabRange)
            {
                GameEvents.OnShowHint("Puseti tutmak için yeterince yakýn deðilsin.", 3f);
                return;
            }
        }

        GameEvents.OnTryDragRequested(mouthMountPoint);
        isDragging = true;
        GameEvents.OnWolfDragStateChanged(true);
    }

    private void OnDragCanceled(InputAction.CallbackContext context)
    {
        if (!isDragging) return;

        isDragging = false;
        GameEvents.OnWolfDragStateChanged(false);

        // HATA 4 ÇÖZÜMÜ: BabyManager'ýn býrakma iþleminde artýk parametre dikkate alýnmýyor.
        GameEvents.OnTryDropRequested(Vector3.zero);
    }
}