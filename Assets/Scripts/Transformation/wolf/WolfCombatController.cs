using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WolfCombatController : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float meleeDamage = 25f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeAnimDuration = 0.8f;
    [SerializeField] private Transform attackPoint; // Aðýz/Pençe hizasý
    [SerializeField] private LayerMask enemyLayer;

    [Header("Dash (Atýlma) Settings")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashAoERadius = 4f;
    [SerializeField] private float dashAoEDamage = 40f;
    [SerializeField] private float dashCooldown = 3f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference attackAction; // Sol Týk
    [SerializeField] private InputActionReference dashAction;   // Shift

    private bool isWolf = false;
    private bool isActionLocked = false; // Saldýrý/Atýlma yaparken tekrar basmayý önler
    private float lastDashTime = -10f;

    private CharacterController characterController; // Sadece dash sýrasýndaki fiziksel itme için gerekli

    private void Awake() => characterController = GetComponent<CharacterController>();

    private void OnEnable()
    {
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

        if (attackAction != null) attackAction.action.Disable();
        if (dashAction != null) dashAction.action.Disable();

        attackAction.action.performed -= TryMeleeAttack;
        dashAction.action.performed -= TryDash;
    }

    private void UpdateForm(bool wolfState) => isWolf = wolfState;
    private void LockActionsTemporarily(bool becomingWolf) => isActionLocked = true; // Dönüþüm bitince FormManager OnFormChanged atacak, o da baþka yeri tetikleyecek ama burada actionlarý elle açmalýyýz

    private void Update()
    {
        // Dönüþüm bittiðinde kilitleri açmak için ufak bir kontrol
        if (isActionLocked && !isWolf) isActionLocked = false;
    }

    private void TryMeleeAttack(InputAction.CallbackContext context)
    {
        if (!isWolf || isActionLocked) return;
        StartCoroutine(MeleeRoutine());
    }

    private IEnumerator MeleeRoutine()
    {
        isActionLocked = true;
        GameEvents.OnWolfMeleeStarted();
        GameEvents.OnPlayOneShotSFX("WolfBite");

        // Hasar tespiti (Animasyonun ortasýnda hasar vurmasý için ufak bir bekleme eklenebilir)
        yield return new WaitForSeconds(meleeAnimDuration / 2f);

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, meleeRange, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(meleeDamage);
            }
        }

        yield return new WaitForSeconds(meleeAnimDuration / 2f);
        GameEvents.OnWolfMeleeCompleted();
        isActionLocked = false;
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
        GameEvents.OnWolfDashStarted();
        GameEvents.OnPlayOneShotSFX("WolfDash");

        Vector3 dashDirection = transform.forward;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            if (characterController != null)
                characterController.Move(dashDirection * (dashForce * Time.deltaTime));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Dash bittiðinde Alan Hasarý (AoE) patlamasý
        Collider[] hits = Physics.OverlapSphere(transform.position, dashAoERadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(dashAoEDamage);
            }
        }

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