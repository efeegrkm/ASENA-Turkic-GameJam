using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Bu objenin hangi takýmda olduðunu seçin.")]
    [SerializeField] private EntityTeam team = EntityTeam.Enemy;

    [Header("Local Events (VFX, Animasyon vs. için)")]
    [Tooltip("Hasar aldýðýnda kendi üstündeki animatörde kan veya flinch tetiklemek için")]
    public UnityEvent onTakeDamage;
    [Tooltip("Öldüðünde kendi objesini silmesi veya ragdoll olmasý için")]
    public UnityEvent onDie;

    private float currentHealth;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        // Eðer bu script Player'ýn üzerindeyse, oyun baþlar baþlamaz can barý UI'ýný doldur
        if (team == EntityTeam.Player)
        {
            GameEvents.OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float amount, EntityTeam attackerTeam)
    {
        if (isDead) return;

        // DOST ATEÞÝ KONTROLÜ: Saldýran kiþi ile bu objenin takýmý aynýysa hasarý iptal et!
        // (Not: Environment (örn: tuzaklar) herkese hasar verebilir)
        if (attackerTeam == team && attackerTeam != EntityTeam.Environment)
        {
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Lokal objenin kendi tepkilerini (Ses, Animasyon, Kan partikülü) tetikle
        onTakeDamage?.Invoke();

        if (team == EntityTeam.Player)
        {
            // UI Can barýný güncelle
            GameEvents.OnPlayerHealthChanged(currentHealth, maxHealth);
            GameEvents.OnPlayOneShotSFX("PlayerHurt"); // Varsa hasar sesiniz
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        onDie?.Invoke(); 

        if (team == EntityTeam.Player)
        {
            GameEvents.OnPlayerDied();
        }
        else if (team == EntityTeam.Enemy)
        {
            GameEvents.OnEnemyDied(gameObject);
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (team == EntityTeam.Player)
        {
            GameEvents.OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }
}