using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [Header("Combat Settings")]
    [Tooltip("Okun hedefe vuraca�� hasar miktar�")]
    [SerializeField] private float damageAmount = 30f;

    [Header("Stick Settings")]
    [Tooltip("Okun �arpt��� y�zeye ne kadar g�m�lece�i (metre)")]
    [SerializeField] private float stickDepth = 0.25f;
    [Tooltip("Okun saplanabilece�i katmanlar (�rn: �evre, D��man)")]
    [SerializeField] private LayerMask stickableLayers;

    [Header("References")]
    [Tooltip("Ok sapland���nda aktif edilecek olan etkile�im scriptin")]
    [SerializeField] private InteractableObject interactableComponent;

    private Rigidbody rb;
    private Collider col;
    private bool isStuck = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // ��Z�M 3 (F�Z�K): Okun y�ksek h�zda duvarlar�n/zeminin i�inden ge�mesini engeller.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

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

        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable target))
        {
            target.TakeDamage(damageAmount, EntityTeam.Player);
        }

        if (((1 << collision.gameObject.layer) & stickableLayers) != 0)
        {
            StickToTarget(collision);
            ShootTarget(collision, damageAmount);
        }
        else
        {
            GameEvents.OnPlayOneShotSFX("ArrowDeflect");
        }
    }

    private void StickToTarget(Collision collision)
    {
        isStuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        col.enabled = false;

        ContactPoint contact = collision.GetContact(0);
        transform.position = contact.point + (transform.forward * stickDepth);

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

    private void ShootTarget(Collision collision, float damage)
    {
        GameEvents.OnEnemyAttackedByBow?.Invoke(collision.collider, damage);
    }
}