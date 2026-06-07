using UnityEngine;

public class AtmosphereManager : MonoBehaviour
{
    [Header("Fog Settings")]
    [Tooltip("Sis rengi (Koyu gri/yeþilimsi tonlar korku için idealdir)")]
    [SerializeField] private Color fogColor = new Color(0.1f, 0.12f, 0.1f);
    [Tooltip("Sisin yoðunluðu (Görüþ mesafesini kýsar)")]
    [SerializeField] private float fogDensity = 0.03f;

    private void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }

    private void OnDisable()
    {
        RenderSettings.fog = false;
    }
}