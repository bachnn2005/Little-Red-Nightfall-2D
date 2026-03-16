using UnityEngine;
using UnityEngine.SceneManagement; // Dùng nếu muốn load lại menu chính
#if UNITY_EDITOR
using UnityEditor; // Dùng để test nút Quit trong Editor
#endif

public class GamePauseUI : MonoBehaviour
{
    [Header("--- UI REFERENCES ---")]
    [Tooltip("Kéo Panel chính (PauseMenu_Panel) vào đây")]
    public GameObject pausePanel;

    [Tooltip("Kéo Panel xác nhận (ConfirmPanel) vào đây")]
    public GameObject confirmPanel;

    private bool isPaused = false;

    void Start()
    {
        // Đảm bảo khi game bắt đầu thì menu ẩn đi và game chạy bình thường
        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // Bấm ESC để bật/tắt Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- CÁC HÀM CHỨC NĂNG (Gán vào Button) ---

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Dừng hoàn toàn thời gian trong game (vật lý, animation)

        if (pausePanel != null) pausePanel.SetActive(true);
        if (confirmPanel != null) confirmPanel.SetActive(false); // Ẩn bảng xác nhận nếu nó đang mở

        // Hiện chuột để bấm nút
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Trả lại thời gian bình thường

        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // Ẩn chuột đi (nếu game FPS/RPG cần khóa chuột)
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked; 
    }

    // Hàm cho nút QUIT (Hiện bảng cảnh báo)
    public void OnClick_QuitButton()
    {
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    // Hàm cho nút NO (Tắt bảng cảnh báo)
    public void OnClick_NoButton()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    // Hàm cho nút YES (Thoát thật)
    public void OnClick_YesButton()
    {
        Debug.Log("Đang thoát game...");

        // Thoát game khi Build ra file .exe
        Application.Quit();

        // Đoạn này để test thoát game ngay trong Unity Editor
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}