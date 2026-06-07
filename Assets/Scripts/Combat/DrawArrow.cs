using UnityEngine;

/// <summary>
/// Yayda görünen "dummy" oku yönetir.
/// BowController'a dokunmadan, sadece GameEvents eventlerine abone olarak çalışır.
///
/// KURULUM:
/// 1. Bu scripti yay prefab'ınızdaki herhangi bir GameObject'e ekleyin (örn: "BowVisuals").
/// 2. drawArrowObject → Inspector'dan yayınızdaki dummy ok modelini atayın.
/// 3. nockPoint        → Okun başlangıç pozisyonu (yayın ipe değdiği nokta, "Nock Point").
/// 4. fullyDrawnPoint  → Yay tamamen gerildiğinde okun ulaşacağı pozisyon.
/// </summary>
public class DrawArrow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Yayda görünecek olan dummy ok objesi.")]
    [SerializeField] private GameObject drawArrowObject;

    [Tooltip("Okun başlangıç noktası: yayın ipine temas ettiği yer (Nock Point).")]
    [SerializeField] private Transform nockPoint;

    [Tooltip("Yay tamamen gerilince okun konumlandığı nokta (çekiş noktası).")]
    [SerializeField] private Transform fullyDrawnPoint;

    [Header("Draw Settings")]
    [Tooltip("Germe animasyonunun ne kadar sürede tamamlanacağı (saniye). " +
             "BowController'daki maxChargeTime ile eşleşmesi önerilir.")]
    [SerializeField] private float maxChargeTime = 1.0f;

    [Tooltip("Gerilme eğrisi: yatay eksen zaman (0-1), dikey eksen pozisyon (0-1). " +
             "Varsayılan düz çizgi bırakılabilir, EaseOut için animation curve kullanın.")]
    [SerializeField] private AnimationCurve drawCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // --- Private State ---
    private bool isDrawing = false;
    private float drawTimer = 0f;

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Başlangıçta dummy ok gizli olmalı
        SetArrowVisible(false);
    }

    private void OnEnable()
    {
        GameEvents.OnBowDrawStarted += HandleDrawStarted;
        GameEvents.OnBowDrawCanceled += HandleDrawEnded;
        GameEvents.OnBowShooted += HandleShoot;
    }

    private void OnDisable()
    {
        GameEvents.OnBowDrawStarted -= HandleDrawStarted;
        GameEvents.OnBowDrawCanceled -= HandleDrawEnded;
        GameEvents.OnBowShooted -= HandleShoot;

        // Obje disable olduğunda temizlik
        StopDraw();
    }

    private void Update()
    {
        if (!isDrawing) return;

        drawTimer += Time.deltaTime;

        // 0-1 arası normalize edilmiş germe oranı
        float chargeRatio = Mathf.Clamp01(drawTimer / maxChargeTime);

        // Eğri uygulanmış t değeri (smooth hareket için)
        float curvedT = drawCurve.Evaluate(chargeRatio);

        // Dummy oku nockPoint → fullyDrawnPoint arasında hareket ettir
        UpdateArrowPosition(curvedT);
    }

    // ─────────────────────────────────────────────
    // Event Handlers
    // ─────────────────────────────────────────────

    private void HandleDrawStarted()
    {
        drawTimer = 0f;
        isDrawing = true;
        SetArrowVisible(true);

        // Oku hemen başlangıç pozisyonuna yerleştir
        UpdateArrowPosition(0f);
    }

    /// <summary>
    /// İptal edildiğinde (Space) veya süre yetersizse çağrılır.
    /// </summary>
    private void HandleDrawEnded()
    {
        StopDraw();
    }

    /// <summary>
    /// Ok başarıyla atıldığında çağrılır. chargeRatio parametresi burada kullanılmıyor
    /// ama imzayı GameEvents ile eşleştirmek için gerekli.
    /// </summary>
    private void HandleShoot(float chargeRatio)
    {
        StopDraw();
    }

    // ─────────────────────────────────────────────
    // Internal Helpers
    // ─────────────────────────────────────────────

    /// <summary>
    /// Dummy oku nock ile fullyDrawn noktaları arasında t parametresine göre konumlandırır.
    /// </summary>
    private void UpdateArrowPosition(float t)
    {
        if (drawArrowObject == null || nockPoint == null || fullyDrawnPoint == null) return;

        drawArrowObject.transform.position = Vector3.Lerp(
            nockPoint.position,
            fullyDrawnPoint.position,
            t
        );

        // Okun rotasyonunu da nock noktasından al (yayın yönüne hizalı kalsın)
        drawArrowObject.transform.rotation = nockPoint.rotation;
    }

    private void StopDraw()
    {
        isDrawing = false;
        drawTimer = 0f;
        SetArrowVisible(false);
    }

    private void SetArrowVisible(bool visible)
    {
        if (drawArrowObject != null)
            drawArrowObject.SetActive(visible);
    }
}