using UnityEngine;
using UnityEngine.SceneManagement; // Cần cái này để chuyển cảnh

public class MainMenu : MonoBehaviour
{
    // Hàm này sẽ gán vào nút PLAY
    public void PlayGame()
    {
        // LoadScene dùng để chuyển cảnh. 
        // Số 1 ở đây là Index của màn chơi trong Build Settings (sẽ chỉnh ở bước 4)
        // Hoặc bạn có thể điền tên màn chơi: SceneManager.LoadScene("Level1");
        SceneManager.LoadScene(1);
    }

    // Hàm này sẽ gán vào nút QUIT
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!"); // Hiện dòng này để test trong Unity Editor
        Application.Quit(); // Lệnh thoát game thật (chỉ chạy khi đã build ra file .exe)
    }
}