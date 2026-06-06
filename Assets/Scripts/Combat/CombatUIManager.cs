using UnityEngine;
using TMPro;

public class CombatUIManager : MonoBehaviour
{
    [SerializeField] private GameObject crosshairPanel; 
    [SerializeField] private TextMeshProUGUI arrowCountText; 

    private void OnEnable()
    {
        GameEvents.OnCrosshairVisibilityChanged += ToggleCrosshair;
        GameEvents.OnArrowCountChanged += UpdateArrowCount;
    }

    private void OnDisable()
    {
        GameEvents.OnCrosshairVisibilityChanged -= ToggleCrosshair;
        GameEvents.OnArrowCountChanged -= UpdateArrowCount;
    }

    private void Start()
    {
        ToggleCrosshair(false);
    }

    private void ToggleCrosshair(bool isVisible)
    {
        if (crosshairPanel != null) crosshairPanel.SetActive(isVisible);
    }

    private void UpdateArrowCount(int count)
    {
        if (arrowCountText != null) arrowCountText.text = count.ToString();
    }
}