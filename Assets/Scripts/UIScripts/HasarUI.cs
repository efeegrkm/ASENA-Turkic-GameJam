using UnityEngine;
using UnityEngine.UI;

public class DamageFlashUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image uiImageDisplay;

    [Header("Damage Frames (3 PNG)")]
    [SerializeField] private Sprite[] damageSprites = new Sprite[3];

    [Header("Animation Settings")]
    [Tooltip("Her bir PNG'nin ekranda kalacağı süre (Örn: 0.05 saniye)")]
    [SerializeField] private float frameDuration = 0.05f;

    private float timer;
    private int currentFrameIndex = -1;
    private bool isAnimating = false;

    private void Start()
    {
        if (uiImageDisplay != null)
        {
            uiImageDisplay.enabled = false;
        }
    }

    private void Update()
    {
        // Eğer hasar yemediysek arkada boşuna hiçbir şey hesaplama, çalışmayı bırak
        if (!isAnimating) return;

        // Zamanlayıcıyı geriye doğru akıtıyoruz
        timer -= Time.deltaTime;

        // Bir karenin süresi doldu mu?
        if (timer <= 0)
        {
            currentFrameIndex++; // Sonraki PNG görseline geç

            // Eğer 3 resmi de sırayla gösterip bitirdiysek animasyonu kapat
            if (currentFrameIndex >= damageSprites.Length)
            {
                StopAnimation();
            }
            else
            {
                // Sıradaki resmi ekrana bas ve zamanlayıcıyı o resim için sıfırla
                DisplayCurrentFrame();
                timer = frameDuration;
            }
        }
    }

    // Bu metot yine HealthManager'daki "onTakeDamage" olayından tetiklenecek
    public void PlayDamageFlash()
    {
        if (uiImageDisplay == null || damageSprites.Length < 3) return;

        isAnimating = true;
        currentFrameIndex = 0; // İlk resimden (0. indeksten) başla
        timer = frameDuration; // İlk resmin süresini kur

        uiImageDisplay.enabled = true;
        DisplayCurrentFrame();
    }

    private void DisplayCurrentFrame()
    {
        if (damageSprites[currentFrameIndex] != null)
        {
            uiImageDisplay.sprite = damageSprites[currentFrameIndex];
        }
    }

    private void StopAnimation()
    {
        isAnimating = false;
        currentFrameIndex = -1;
        if (uiImageDisplay != null)
        {
            uiImageDisplay.enabled = false; // Resmi tamamen gizle
        }
    }
}