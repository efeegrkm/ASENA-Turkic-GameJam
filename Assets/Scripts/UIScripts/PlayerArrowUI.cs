using UnityEngine;
using TMPro; // TextMeshPro Kütüphanesi

public class PlayerArrowUI : MonoBehaviour
{
    [Tooltip("Kafanýn üzerindeki TextMeshPro objesini buraya sürükle")]
    [SerializeField] private TextMeshProUGUI arrowText;

    private void OnEnable()
    {
        GameEvents.OnArrowCountChanged += UpdateArrowText;
    }

    private void OnDisable()
    {
        GameEvents.OnArrowCountChanged -= UpdateArrowText;
    }

    private void UpdateArrowText(int currentArrowCount)
    {
        if (arrowText != null)
        {
            arrowText.text = $"Ok Sayýsý: {currentArrowCount}";
        }
    }
}