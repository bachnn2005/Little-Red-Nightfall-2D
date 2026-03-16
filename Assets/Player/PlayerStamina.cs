using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Cấu hình Thể lực")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float regenRate = 20f;
    public float regenDelay = 2f;

    [Header("UI")]
    public Slider staminaSlider;

    private float lastStaminaUseTime;

    void Start()
    {
        currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    void Update()
    {
        if (Time.time - lastStaminaUseTime >= regenDelay && currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            UpdateUI();
        }
    }

    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0);
        lastStaminaUseTime = Time.time;
        UpdateUI();
    }

    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (staminaSlider != null) staminaSlider.value = currentStamina;
    }
}