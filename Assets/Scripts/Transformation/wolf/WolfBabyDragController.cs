using UnityEngine;
using UnityEngine.InputSystem;

public class WolfBabyDragController : MonoBehaviour
{
    [Header("Drag Inputs")]
    [Tooltip("Puseti tutmak için basýlacak tuþ (E - Interact)")]
    [SerializeField] private InputActionReference grabAction;
    [Tooltip("Puseti býrakmak için basýlacak tuþ (G - Drop)")]
    [SerializeField] private InputActionReference dropAction;

    [Header("References")]
    [SerializeField] private Transform mouthMountPoint;
    [SerializeField] private float grabRange = 2.5f;

    private bool isWolf = false;
    private bool isDragging = false;
    private BabyState currentBabyState = BabyState.Dropped;

    private void OnEnable()
    {
        GameEvents.OnFormChanged += (state) => isWolf = state;
        GameEvents.OnBabyStateChanged += (state) => currentBabyState = state;

        // ÇÖZÜM: 'performed' yerine 'started' kullanýyoruz. 
        // Böylece tuþa basýldýðý o ilk milisaniyede komut çalýþýr, basýlý tutmaya gerek kalmaz.
        grabAction.action.started += OnGrabPerformed;
        dropAction.action.started += OnDropPerformed;
    }

    private void OnDisable()
    {
        grabAction.action.started -= OnGrabPerformed;
        dropAction.action.started -= OnDropPerformed;
    }

    private void OnGrabPerformed(InputAction.CallbackContext context)
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

        GameEvents.OnTryDragRequested(mouthMountPoint);
        isDragging = true;
        GameEvents.OnWolfDragStateChanged(true);
    }

    private void OnDropPerformed(InputAction.CallbackContext context)
    {
        if (!isDragging) return;

        isDragging = false;
        GameEvents.OnWolfDragStateChanged(false);

        GameEvents.OnTryDropRequested(Vector3.zero);
    }
}