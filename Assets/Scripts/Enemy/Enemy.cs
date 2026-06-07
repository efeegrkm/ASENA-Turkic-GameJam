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

    public void Initialize(EnemyData enemyData)
    {
        data = enemyData;
        currentHealth = data.maxHealth;

        // Düþmanýn hýzýný veriden çekip Navigator'a gönder
        if (navigator != null)
        {
            navigator.SetSpeed(data.speed);
        }
    }

    // Ok ve Kurt saldýrýlarýnýn buraya hasar vurabilmesi için IDamageable fonksiyonu
    public void TakeDamage(float amount, EntityTeam attackerTeam)
    {
        if (IsDead) return;

        if (attackerTeam == EntityTeam.Player)
        {
            currentHealth -= amount;

            // Ýsteðe baðlý: GameEvents.OnPlayOneShotSFX("EnemyHurt");

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

        // Asena'nýn diðer oklarý ölü cesede saplanmasýn diye collider'ý kapat
        if (col != null) col.enabled = false;

        // Ýsteðe baðlý: GameEvents.OnPlayOneShotSFX("EnemyDeath");

        // 3 saniye cesedi yerde bekletip sonra sahneden sil
        Destroy(gameObject, 3f);
    }

    // Navigator hedefe yaklaþýnca bu fonksiyonu çaðýracak
    public void TriggerAttackAnimation()
    {
        if (!IsDead) anim.SetTrigger("attack");
    }
}