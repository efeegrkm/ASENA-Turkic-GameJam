using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [Header("Combat Settings")]
    [Tooltip("Okun hedefe vuracaðý hasar miktarý")]
    [SerializeField] private float damageAmount = 30f;

    [Header("Stick Settings")]
    [Tooltip("Okun çarptýðý yüzeye ne kadar gömüleceði (metre)")]
    [SerializeField] private float stickDepth = 0.25f;
    [Tooltip("Okun saplanabileceði katmanlar (Örn: Çevre, Düþman)")]
    [SerializeField] private LayerMask stickableLayers;

    [Header("References")]
    [Tooltip("Ok saplandýðýnda aktif edilecek olan etkileþim scriptin")]
    [SerializeField] private InteractableObject interactableComponent;

    private Rigidbody rb;
    private Collider col;
    private bool isStuck = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (interactableComponent != null)
        {
            interactableComponent.enabled = false;
        }
    }

    private void Update()
    {
        if (!isStuck && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;

        // 1. ADIM: Çarptýðýmýz objede bir can sistemi (HealthManager) var mý kontrol et
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable target))
        {
            // Varsa, Oyuncu (Player) takýmý adýna hasarý vur!
            target.TakeDamage(damageAmount, EntityTeam.Player);
            
            // GameEvents.OnPlayOneShotSFX("ArrowHitFlesh");
        }

        if (((1 << collision.gameObject.layer) & stickableLayers) != 0)
        {
            StickToTarget(collision);
        }
        else
        {
            // Saplanýlamaz bir yere (örneðin demir kalkan) çarptýysa sekme sesi çal
            GameEvents.OnPlayOneShotSFX("ArrowDeflect");
        }
    }

    private void StickToTarget(Collision collision)
    {
        isStuck = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position += transform.forward * stickDepth;

        col.enabled = false;

        transform.SetParent(collision.transform);

        if (interactableComponent != null)
        {
            interactableComponent.enabled = true;
        }

        if (collision.gameObject.GetComponent<IDamageable>() == null)
        {
            GameEvents.OnPlayOneShotSFX("ArrowHitWood");
        }
    }
}