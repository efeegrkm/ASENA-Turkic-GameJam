using System.Collections;
using UnityEngine;
using TMPro;

public class HintUIManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Hazýrladýðýn Hint Text (TMP) Prefabýný buraya sürükle")]
    [SerializeField] private GameObject hintTextPrefab;
    [Tooltip("Metinlerin Canvas içinde nerede belirmesini istiyorsan o boþ objeyi (veya Layout Group'u) sürükle")]
    [SerializeField] private Transform hintContainer;

    private void OnEnable()
    {
        GameEvents.OnShowHint += ShowHint;
    }

    private void OnDisable()
    {
        GameEvents.OnShowHint -= ShowHint;
    }

    private void ShowHint(string message, float duration)
    {
        if (hintTextPrefab == null || hintContainer == null) return;

        GameObject hintObj = Instantiate(hintTextPrefab, hintContainer);

        if (hintObj.TryGetComponent<TMP_Text>(out TMP_Text textComponent))
        {
            textComponent.text = message;
        }

        Destroy(hintObj, duration);
    }
}