using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Raycast Spread Settings")]
    [Tooltip("Merkezdeki ana raycast'in etrafýna atýlacak 4 yardýmcý raycast'in merkezden uzaklýðý")]
    [SerializeField] private float raySpacing = 0.5f;

    [Tooltip("Input Action Asset'inizdeki etkileþim aksiyonunu buraya sürükleyin.")]
    [SerializeField] private InputActionReference interactAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private IInteractable currentInteractable;

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        Vector3[] rayOffsets = new Vector3[]
        {
            Vector3.zero,                    
            cameraTransform.right * raySpacing,       
            -cameraTransform.right * raySpacing,        
            cameraTransform.up * raySpacing,    
            -cameraTransform.up * raySpacing 
        };

        IInteractable foundInteractable = null;
        Color debugRayColor = Color.red;

        foreach (Vector3 offset in rayOffsets)
        {
            Ray ray = new Ray(cameraTransform.position + offset, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    foundInteractable = interactable;
                    debugRayColor = Color.green;
                    break;
                }
            }
        }

        if (foundInteractable != null)
        {
            if (foundInteractable != currentInteractable)
            {
                currentInteractable?.OnLoseFocus();
                currentInteractable = foundInteractable;
                currentInteractable.OnFocus();
            }

            // Open press to interact

            if (interactAction.action.WasPressedThisFrame())
            {
                foundInteractable.Interact();
            }
        }
        else
        {
            ClearInteractable(); 
        }

        foreach (Vector3 offset in rayOffsets)
        {
            Debug.DrawRay(cameraTransform.position + offset, cameraTransform.forward * interactRange, debugRayColor);
        }
    }

    private void ClearInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraTransform == null) return;
        Gizmos.color = Color.yellow;

        Vector3[] rayOffsets = new Vector3[]
        {
            Vector3.zero,
            cameraTransform.right * raySpacing,
            -cameraTransform.right * raySpacing,
            cameraTransform.up * raySpacing,
            -cameraTransform.up * raySpacing
        };

        foreach (Vector3 offset in rayOffsets)
        {
            Gizmos.DrawRay(cameraTransform.position + offset, cameraTransform.forward * interactRange);
        }
    }
}