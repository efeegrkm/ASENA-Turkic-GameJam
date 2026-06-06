using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Raycast Spread Settings")]
    [Tooltip("Raycastlerin merkezden ne kadar uzaða açýlacaðý")]
    [SerializeField] private float raySpacing = 0.5f;

    [Tooltip("Input Action Asset'inizdeki etkileþim aksiyonunu buraya sürükleyin.")]
    [SerializeField] private InputActionReference interactAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Kamera ile oyuncu arasýndaki objeleri yoksaymak için Oyuncunun (Player) Transform'unu buraya sürükleyin.")]
    [SerializeField] private Transform playerTransform;

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
        Vector3 up = cameraTransform.up;
        Vector3 right = cameraTransform.right;

        // ÝSTENÝLEN DÝZÝLÝM: 3 Ortada (Üçgen) + 4 Kenarlarda = 7 Iþýn
        Vector3[] rayOffsets = new Vector3[]
        {
            // --- Ýç Kýsým (Ortadaki Üçgen) ---
            up * (raySpacing * 0.25f),                                      // Merkez Üst
            (-up - right).normalized * (raySpacing * 0.25f),                // Merkez Sol Alt
            (-up + right).normalized * (raySpacing * 0.25f),                // Merkez Sað Alt

            // --- Dýþ Kýsým (4 Kenar) ---
            (up + right).normalized * raySpacing,                           // Sað Üst
            (up - right).normalized * raySpacing,                           // Sol Üst
            (-up + right).normalized * raySpacing,                          // Sað Alt
            (-up - right).normalized * raySpacing                           // Sol Alt
        };

        IInteractable foundInteractable = null;
        Color debugRayColor = Color.red;

        foreach (Vector3 offset in rayOffsets)
        {
            Ray ray = new Ray(cameraTransform.position + offset, cameraTransform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
            {
                bool isValidHit = true;

                // KAMERA VE OYUNCU ARASINA GÝRENLERÝ ENGELLEME MANTIÐI
                if (playerTransform != null)
                {
                    // Çarpýlan noktanýn ve oyuncunun kameranýn ileri eksenindeki derinliklerini ölçüyoruz
                    float hitDepth = Vector3.Dot(hit.point - cameraTransform.position, cameraTransform.forward);
                    float playerDepth = Vector3.Dot(playerTransform.position - cameraTransform.position, cameraTransform.forward);

                    // Eðer çarpýlan obje oyuncudan daha gerideyse (kamera ile oyuncu arasýndaysa)
                    // Ayaktaki objeleri rahat almak için -0.5f'lik küçük bir hata payý býrakýyoruz
                    if (hitDepth < playerDepth - 0.5f)
                    {
                        isValidHit = false; 
                    }
                }

                if (isValidHit)
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        foundInteractable = interactable;
                        debugRayColor = Color.green;
                        break; // Ýlk geçerli etkileþimi bulduðunda döngüden çýk
                    }
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

            if (interactAction.action.WasPressedThisFrame())
            {
                foundInteractable.Interact();
            }
        }
        else
        {
            ClearInteractable(); 
        }

        // Scene ekranýnda test edebilmen için ýþýnlarý çizer
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

        Vector3 up = cameraTransform.up;
        Vector3 right = cameraTransform.right;

        Vector3[] rayOffsets = new Vector3[]
        {
            up * (raySpacing * 0.25f),
            (-up - right).normalized * (raySpacing * 0.25f),
            (-up + right).normalized * (raySpacing * 0.25f),
            (up + right).normalized * raySpacing,
            (up - right).normalized * raySpacing,
            (-up + right).normalized * raySpacing,
            (-up - right).normalized * raySpacing
        };

        foreach (Vector3 offset in rayOffsets)
        {
            Gizmos.DrawRay(cameraTransform.position + offset, cameraTransform.forward * interactRange);
        }
    }
}