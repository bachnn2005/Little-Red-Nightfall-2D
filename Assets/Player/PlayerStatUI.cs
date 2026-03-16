using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    [Header("--- UI PANELS ---")]
    public GameObject statPanel;

    [Header("--- NUMERIC TEXTS ---")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI regenText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI pointsText;

    [Header("--- ATTRIBUTE VALUES (Mỗi lần cộng) ---")]
    public int pointsPerLevel = 5;
    public int hpIncrease = 10;
    public float staminaIncrease = 10f;
    public int regenIncrease = 1;
    public int atkIncrease = 2;

    [Header("--- REFERENCES ---")]
    private PlayerHealth health;
    private PlayerAttack attack;
    private PlayerStamina stamina;

    // Biến lưu trữ số điểm tạm thời đang cộng (Chưa confirm)
    private int tempHP, tempRegen, tempAtk;
    private float tempStamina;
    private int tempPointsSpent = 0;

    private bool isOpen = false;

    void Start()
    {
        FindReferences();
        if (statPanel != null) statPanel.SetActive(false);
    }

    // Hàm bổ trợ để tìm lại Player nếu bị mất tham chiếu (Sửa lỗi Null)
    private bool FindReferences()
    {
        if (health != null && attack != null && stamina != null) return true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            health = p.GetComponent<PlayerHealth>();
            attack = p.GetComponent<PlayerAttack>();
            stamina = p.GetComponent<PlayerStamina>();
            return true;
        }
        return false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
            statPanel.SetActive(isOpen);

            if (isOpen)
            {
                ResetTempStats(); // Reset lại các con số tạm thời khi vừa mở menu
                UpdateOnlyNumbers();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void ResetTempStats()
    {
        tempHP = 0; tempStamina = 0; tempRegen = 0; tempAtk = 0;
        tempPointsSpent = 0;
    }

    public void UpdateOnlyNumbers()
    {
        if (!FindReferences()) return; // Thoát nếu không tìm thấy Player (Tránh lỗi Null)

        // Hiển thị: Giá trị gốc của Player + Giá trị tạm thời đang cộng
        if (hpText != null) hpText.text = (health.maxHealth + tempHP).ToString();
        if (regenText != null) regenText.text = (health.regenAmount + tempRegen).ToString();
        if (staminaText != null) staminaText.text = Mathf.FloorToInt(stamina.maxStamina + tempStamina).ToString();
        if (atkText != null) atkText.text = (attack.damage1 + attack.bonusAttackDamage + tempAtk).ToString();

        // Điểm còn lại hiển thị = Điểm thật - Điểm tạm dùng
        if (pointsText != null) pointsText.text = (pointsPerLevel - tempPointsSpent).ToString();
    }

    #region --- LOGIC CỘNG/TRỪ TẠM THỜI ---

    // HP
    public void AddHP() { if (tempPointsSpent < pointsPerLevel) { tempHP += hpIncrease; tempPointsSpent++; UpdateOnlyNumbers(); } }
    public void SubHP() { if (tempHP > 0) { tempHP -= hpIncrease; tempPointsSpent--; UpdateOnlyNumbers(); } }

    // STAMINA
    public void AddStamina() { if (tempPointsSpent < pointsPerLevel) { tempStamina += staminaIncrease; tempPointsSpent++; UpdateOnlyNumbers(); } }
    public void SubStamina() { if (tempStamina > 0) { tempStamina -= staminaIncrease; tempPointsSpent--; UpdateOnlyNumbers(); } }

    // REGEN
    public void AddRegen() { if (tempPointsSpent < pointsPerLevel) { tempRegen += regenIncrease; tempPointsSpent++; UpdateOnlyNumbers(); } }
    public void SubRegen() { if (tempRegen > 0) { tempRegen -= regenIncrease; tempPointsSpent--; UpdateOnlyNumbers(); } }

    // ATK
    public void AddAtk() { if (tempPointsSpent < pointsPerLevel) { tempAtk += atkIncrease; tempPointsSpent++; UpdateOnlyNumbers(); } }
    public void SubAtk() { if (tempAtk > 0) { tempAtk -= atkIncrease; tempPointsSpent--; UpdateOnlyNumbers(); } }

    #endregion

    // --- NÚT CONFIRM XÁC NHẬN ---
    public void ConfirmUpgrades()
    {
        if (!FindReferences() || tempPointsSpent == 0) return;

        // Áp dụng vĩnh viễn vào Player
        health.maxHealth += tempHP;
        health.currentHealth += tempHP; // Hồi máu thêm từ phần cộng
        health.regenAmount += tempRegen;

        stamina.maxStamina += tempStamina;
        stamina.currentStamina += tempStamina;

        attack.bonusAttackDamage += tempAtk;

        // Trừ điểm thật
        pointsPerLevel -= tempPointsSpent;

        // Reset bộ đếm tạm thời
        ResetTempStats();
        UpdateOnlyNumbers();

        Debug.Log("Đã xác nhận nâng cấp!");
    }
}