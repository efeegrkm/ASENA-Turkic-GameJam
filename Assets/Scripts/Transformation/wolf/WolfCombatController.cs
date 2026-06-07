using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WolfCombatController : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float meleeDamage = 25f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeAnimDuration = 0.8f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Dash (At�lma) Settings")]
    [Tooltip("Karakterin at�lmadan �nce g�� toplama (gerinme) s�resi")]
    [SerializeField] private float dashWindUpDuration = 0.6f;
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashAoERadius = 4f;
    [SerializeField] private float dashAoEDamage = 40f;
    [SerializeField] private float dashCooldown = 3f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference dashAction;

    private bool isWolf = false;
    private bool isActionLocked = false;
    private float lastDashTime = -10f;

    private CharacterController characterController;

    private void Awake() => characterController = GetComponentInParent<CharacterController>();

    private void OnEnable()
    {
        isActionLocked = false;

        GameEvents.OnFormChanged += UpdateForm;
        GameEvents.OnFormChangeStarted += LockActionsTemporarily;

        if (attackAction != null) attackAction.action.Enable();
        if (dashAction != null) dashAction.action.Enable();

        attackAction.action.performed += TryMeleeAttack;
        dashAction.action.performed += TryDash;
    }

    private void OnDisable()
    {
        GameEvents.OnFormChanged -= UpdateForm;
        GameEvents.OnFormChangeStarted -= LockActionsTemporarily;

        attackAction.action.performed -= TryMeleeAttack;
        dashAction.action.performed -= TryDash;
    }

    private void UpdateForm(bool wolfState) => isWolf = wolfState;
    private void LockActionsTemporarily(bool becomingWolf) => isActionLocked = true;

    private void TryMeleeAttack(InputAction.CallbackContext context)
    {
        if (!isWolf || isActionLocked) return;
        StartCoroutine(MeleeRoutine());
    }

    private IEnumerator MeleeRoutine()
    {
        isActionLocked = true;
        GameEvents.OnWolfMeleeStarted();

        PlayWolfAttackSound();

        yield return new WaitForSeconds(meleeAnimDuration / 2f);

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, meleeRange, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                GameEvents.OnEnemyAttackedByWolf(hit, meleeDamage);
                target.TakeDamage(meleeDamage, EntityTeam.Player);
            }
        }

        yield return new WaitForSeconds(meleeAnimDuration / 2f);
        GameEvents.OnWolfMeleeCompleted();
        isActionLocked = false;
    }

    private void PlayWolfAttackSound()
    {
        int soundIndex = Random.Range(1, 4); // 1, 2 veya 3
        GameEvents.OnPlayOneShotSFX($"WolfBite{soundIndex}");
    }

    private void TryDash(InputAction.CallbackContext context)
    {
        if (!isWolf || isActionLocked || Time.time < lastDashTime + dashCooldown) return;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isActionLocked = true;
        lastDashTime = Time.time;

        // 1. Eventleri ate�le (Animasyon ve hareket kilidi burada ba�lar)
        GameEvents.OnWolfDashStarted();
        GameEvents.OnPlayOneShotSFX("WolfDash");

        // 2. Fiziksel at�lma �ncesi, animasyonun gerinmesi i�in bekle!
        yield return new WaitForSeconds(dashWindUpDuration);

        // 3. Fiziksel At�lma ��lemi
        Vector3 dashDirection = transform.forward;
        float elapsed = 0f;

        float verticalVelocity = 6f;
        float gravity = -20f;

        while (elapsed < dashDuration)
        {
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 currentMove = (dashDirection * dashForce) + (Vector3.up * verticalVelocity);

            if (characterController != null)
                characterController.Move(currentMove * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, dashAoERadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(dashAoEDamage, EntityTeam.Player);
            }
        }

        // Hareketi serbest b�rak
        GameEvents.OnWolfDashCompleted();
        isActionLocked = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, meleeRange);
        }
    }
}