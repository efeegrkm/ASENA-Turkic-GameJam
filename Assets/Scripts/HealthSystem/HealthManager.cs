using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Bu objenin hangi takýmda olduðunu seçin.")]
    [SerializeField] private EntityTeam team = EntityTeam.Enemy;

    [Header("Game Over UI (Sadece Player Ýçin)")]
    [Tooltip("Oyun bittiðinde aktif edilecek Ekran Canvas paneli")]
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("Neden öldüðünü yazdýracaðýn Text nesnesi")]
    [SerializeField] private TMPro.TextMeshProUGUI gameOverReasonText;

    [Header("Local Events (VFX, Animasyon vs. için)")]
    public UnityEvent onTakeDamage;
    public UnityEvent onDie;

    private float currentHealth;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (team == EntityTeam.Player)
        {
            GameEvents.OnPlayerHealthChanged(currentHealth, maxHealth);

            // Baþlangýçta paneli kesin olarak gizle
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Sadece Player oyunun bitiþ eventlerini dinlesin
        if (team == EntityTeam.Player) GameEvents.OnGameOver += TriggerGameOver;
    }

    private void OnDisable()
    {
        if (team == EntityTeam.Player) GameEvents.OnGameOver -= TriggerGameOver;
    }

    public void TakeDamage(float amount, EntityTeam attackerTeam)
    {
        if (isDead) return;

        if (attackerTeam == team && attackerTeam != EntityTeam.Environment) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        onTakeDamage?.Invoke();

        if (team == EntityTeam.Player)
        {
            GameEvents.OnPlayerHealthChanged(currentHealth, maxHealth);
            GameEvents.OnPlayOneShotSFX("PlayerHurt");
        }

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        onDie?.Invoke();

        if (team == EntityTeam.Player)
        {
            GameEvents.OnPlayerDied();
            // OYUN BÝTTÝ: Asena Öldü
            GameEvents.OnGameOver?.Invoke("Asena Öldü... Oðuz Bebek Kimsesiz Kaldý.");
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

    private void TriggerGameOver(string reason)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverReasonText != null) gameOverReasonText.text = reason;
        }

        // Oyunu, fizikleri ve Update metotlarýný tamamen durdurur
        Time.timeScale = 0f;

        // Yeniden baþlatmak vb. için Mouse imlecini geri ver
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}