using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyData data { get; private set; }
    private float currentHealth;

    private Animator anim;
    private EnemyNavigator navigator;
    private Collider col;

    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        navigator = GetComponent<EnemyNavigator>();
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        GameEvents.OnEnemyAttackedByBow += TakeDamageFromBow;
        GameEvents.OnEnemyAttackedByWolf += TakeDamageFromWolf;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyAttackedByBow -= TakeDamageFromBow;
        GameEvents.OnEnemyAttackedByWolf -= TakeDamageFromWolf;
    }
    
    private void TakeDamageFromBow(Collider targetCollider, float damageAmount)
    {
        if (targetCollider.transform.GetComponent<Enemy>() == this)
        {
            TakeDamage(damageAmount, EntityTeam.Player);
        }
    }

    private void TakeDamageFromWolf(Collider targetCollider, float damageAmount)
    {
        if (targetCollider.transform.GetComponent<Enemy>() == this)
        {
            TakeDamage(damageAmount, EntityTeam.Player);
        }
    }

    public void Initialize(EnemyData enemyData)
    {
        data = enemyData;
        currentHealth = data.maxHealth;

        // D��man�n h�z�n� veriden �ekip Navigator'a g�nder
        if (navigator != null)
        {
            navigator.SetSpeed(data.speed);
        }
    }

    // Ok ve Kurt sald�r�lar�n�n buraya hasar vurabilmesi i�in IDamageable fonksiyonu
    public void TakeDamage(float amount, EntityTeam attackerTeam)
    {
        if (IsDead) return;

        if (attackerTeam == EntityTeam.Player)
        {
            currentHealth -= amount;

            // �ste�e ba�l�: GameEvents.OnPlayOneShotSFX("EnemyHurt");

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        IsDead = true;

        // Animator'deki dead parametresini true yap
        anim.SetBool("dead", true);

        // Beynini ve hareketini kapat
        if (navigator != null) navigator.enabled = false;

        if (TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Asena'n�n di�er oklar� �l� cesede saplanmas�n diye collider'� kapat
        if (col != null) col.enabled = false;

        // �ste�e ba�l�: GameEvents.OnPlayOneShotSFX("EnemyDeath");

        // 3 saniye cesedi yerde bekletip sonra sahneden sil
        Destroy(gameObject, 3f);
    }

    // Navigator hedefe yakla��nca bu fonksiyonu �a��racak
    public void TriggerAttackAnimation()
    {
        if (!IsDead) anim.SetTrigger("attack");
    }
}