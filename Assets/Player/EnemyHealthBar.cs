using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Slider slider;
    private EnemyHealth healthScript;

    void Start()
    {
        healthScript = GetComponent<EnemyHealth>();

        // Khởi tạo giá trị Slider
        if (slider != null && healthScript != null)
        {
            slider.maxValue = healthScript.maxHealth;
            slider.value = healthScript.currentHealth;
        }
    }

    void Update()
    {
        if (slider != null && healthScript != null)
        {
            // Cập nhật giá trị máu liên tục
            slider.value = healthScript.currentHealth;

            // Tự động ẩn thanh máu khi Boss chết
            if (healthScript.currentHealth <= 0)
            {
                slider.gameObject.SetActive(false);
            }
        }
    }

    // Giữ thanh máu luôn nhìn thẳng (không bị lật khi Boss xoay scale)
    void LateUpdate()
    {
        if (slider != null)
        {
            slider.transform.parent.localScale = new Vector3(Mathf.Abs(slider.transform.parent.localScale.x),
                                                             slider.transform.parent.localScale.y,
                                                             slider.transform.parent.localScale.z);
        }
    }
}