using UnityEngine;
using UnityEngine.Video;

public class VideoPingPong : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public float timeMargin = 1.5f; // Cắt 1.5 giây đầu/đuôi

    private double startTime;
    private double endTime;
    private double currentTime;
    private int direction = 1;
    private bool isReady = false;

    void Start()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        // Cài đặt thông số cơ bản
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Tắt tiếng
        videoPlayer.playOnAwake = false; // Tắt tự chạy
        videoPlayer.waitForFirstFrame = true; // Chờ load xong hình mới chạy
        videoPlayer.isLooping = false; // Tắt loop mặc định của Unity

        // Đăng ký sự kiện: Khi nào chuẩn bị xong thì báo tôi
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        Debug.Log("Đang chuẩn bị video...");
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video đã chuẩn bị xong! Độ dài gốc: " + vp.length + "s");

        // Tính toán thời gian
        startTime = timeMargin;
        endTime = vp.length - timeMargin;

        // KIỂM TRA LỖI VIDEO NGẮN
        if (endTime <= startTime)
        {
            Debug.LogError("LỖI: Video quá ngắn! (" + vp.length + "s). Cần tối thiểu " + (timeMargin * 2) + "s để cắt đầu đuôi.");
            return;
        }

        // Đặt thời gian bắt đầu
        currentTime = startTime;
        vp.time = currentTime;

        // Play để kích hoạt render
        vp.Play();

        // Khóa tốc độ về 0 để ta tự điều khiển (Manual Control)
        vp.playbackSpeed = 0;

        isReady = true;
        Debug.Log("Bắt đầu chạy Ping-Pong từ giây thứ: " + startTime + " đến " + endTime);
    }

    void Update()
    {
        // Nếu chưa sẵn sàng hoặc video lỡ bị tắt thì không làm gì cả
        if (!isReady) return;

        // [QUAN TRỌNG] Giữ cho Video luôn ở trạng thái Playing để render texture
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }

        // 1. Tự tính toán thời gian
        currentTime += Time.deltaTime * direction;

        // 2. Logic đảo chiều (Ping-Pong)
        if (currentTime >= endTime)
        {
            currentTime = endTime;
            direction = -1; // Quay ngược
        }
        else if (currentTime <= startTime)
        {
            currentTime = startTime;
            direction = 1; // Chạy xuôi
        }

        // 3. Ép video hiển thị đúng khung hình này
        videoPlayer.time = currentTime;
    }
}