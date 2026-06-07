using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BabyHungerUI : MonoBehaviour
{
    private Slider hungerSlider;

    private void Awake()
    {
        // Scripti doğrudan Slider'a attığımız için bileşeni otomatik bulacak
        hungerSlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // Eventi dinlemeye başla
        GameEvents.OnBabyHungerChanged += UpdateSlider;
    }

    private void OnDisable()
    {
        GameEvents.OnBabyHungerChanged -= UpdateSlider;
    }

    private void UpdateSlider(float current, float max)
    {
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = max;
            hungerSlider.value = current;
        }
    }
}