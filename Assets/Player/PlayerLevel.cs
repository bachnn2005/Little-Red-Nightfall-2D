using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevel : MonoBehaviour
{
    [Header("--- UI ELEMENTS (Kinh nghiệm) ---")]
    public TextMeshProUGUI levelText;    // Text hiện số Level (VD: 1)
    public TextMeshProUGUI xpText;       // Text hiện số XP (VD: 50/100)
    public Slider xpSlider;              // Thanh trượt kinh nghiệm

    [Header("--- CẤU HÌNH LEVEL ---")]
    public int currentLevel = 1;
    public int currentXP = 0;

    [Header("--- CẤU HÌNH XP (KINH NGHIỆM) ---")]
    public int baseXPRequired = 100;
    public int xpIncreasePerLevel = 50;

    [Header("--- PHẦN THƯỞNG ---")]
    public int pointsPerLevelUp = 5;

    private PlayerStatUI statUI;

    void Awake()
    {
        // Lấy tham chiếu đến bảng UI Tab để cộng điểm tiềm năng
        statUI = FindFirstObjectByType<PlayerStatUI>();
    }

    void Start()
    {
        // Khởi tạo giao diện khi bắt đầu game
        UpdateXPUI();
    }

    void Update()
    {
        // Phím tắt Test nhanh (Bấm L nhận 50 XP)
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddXP(50);
        }
    }

    // Hàm gọi từ bên ngoài (ví dụ từ script Enemy) để cộng XP
    public void AddXP(int amount)
    {
        currentXP += amount;

        // Kiểm tra lên cấp (dùng while để xử lý trường hợp nhận cực nhiều XP một lúc)
        while (currentXP >= GetXPRequiredForNextLevel())
        {
            LevelUp();
        }

        UpdateXPUI();
    }

    public int GetXPRequiredForNextLevel()
    {
        // Công thức tính XP cần để lên cấp tiếp theo
        return baseXPRequired + (currentLevel - 1) * xpIncreasePerLevel;
    }

    void LevelUp()
    {
        currentXP -= GetXPRequiredForNextLevel();
        currentLevel++;

        // Cộng điểm tiềm năng vào Menu Stat (khi bấm Tab sẽ thấy)
        if (statUI != null)
        {
            statUI.pointsPerLevel += pointsPerLevelUp;
            statUI.UpdateOnlyNumbers(); // Cập nhật lại số liệu trong bảng Tab
        }

        Debug.Log("Chúc mừng! Bạn đã lên cấp " + currentLevel);
    }

    // Cập nhật giao diện XP
    public void UpdateXPUI()
    {
        int xpNeeded = GetXPRequiredForNextLevel();

        // Cập nhật số Level
        if (levelText != null) levelText.text = currentLevel.ToString();

        // Cập nhật text XP (Ví dụ: 50/100)
        if (xpText != null) xpText.text = currentXP + " / " + xpNeeded;

        // Cập nhật thanh Slider
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpNeeded;
            xpSlider.value = currentXP;
        }
    }
}