using UnityEngine;
using System.Collections.Generic;

public class AutoParallax : MonoBehaviour
{
    [Header("Cài đặt")]
    public GameObject cam;
    [Range(0f, 1f)] public float parallaxEffect;

    [Header("Tùy chọn Tự động trôi (MỚI)")]
    [Tooltip("Tick vào để hậu cảnh tự trôi mà không cần Camera di chuyển")]
    public bool isAutoScrolling = false;
    [Tooltip("Tốc độ tự trôi")]
    public float scrollSpeed = 0.5f;

    [Header("Tinh chỉnh")]
    [Tooltip("Khoảng cách chồng lấn giữa các ảnh (đơn vị World Unit). Tăng số này nếu thấy khe hở.")]
    public float overlapAmount = 0.1f;

    private float length;
    private float effectiveLength;
    private List<Transform> layers = new List<Transform>();
    private List<float> startPositions = new List<float>();

    // Biến lưu trữ khoảng cách tự trôi tích lũy
    private float autoScrollOffset = 0f;

    void Start()
    {
        if (cam == null) cam = Camera.main.gameObject;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        length = sr.bounds.size.x;
        effectiveLength = length - overlapAmount;

        AddLayer(this.transform, transform.position.x);
        SpawnClone(-effectiveLength);
        SpawnClone(effectiveLength);
    }

    void SpawnClone(float xOffset)
    {
        GameObject clone = Instantiate(this.gameObject);
        Destroy(clone.GetComponent<AutoParallax>());

        Vector3 pos = transform.position;
        pos.x += xOffset;
        clone.transform.position = pos;

        clone.name = gameObject.name + " (Clone)";
        clone.transform.parent = this.transform.parent;

        AddLayer(clone.transform, pos.x);
    }

    void AddLayer(Transform t, float startPos)
    {
        layers.Add(t);
        startPositions.Add(startPos);
    }

    void LateUpdate()
    {
        // Nếu bật tự trôi, tính toán khoảng cách cộng thêm theo thời gian
        if (isAutoScrolling)
        {
            autoScrollOffset += scrollSpeed * Time.deltaTime;
        }

        for (int i = 0; i < layers.Count; i++)
        {
            Transform layer = layers[i];
            float startPos = startPositions[i];

            // 1. Tính toán Parallax + Auto Scroll
            // dist: Vị trí hiển thị thực tế
            // temp: Vị trí giả lập để kiểm tra logic nhảy cóc (Infinite loop)
            float dist = (cam.transform.position.x * parallaxEffect) - autoScrollOffset;
            float temp = (cam.transform.position.x * (1 - parallaxEffect)) + autoScrollOffset;

            Vector3 newPos = layer.position;
            newPos.x = startPos + dist;
            layer.position = newPos;

            // 2. Logic Nhảy Cóc (Infinite)
            if (temp > startPos + length)
            {
                startPositions[i] += effectiveLength * 3;
            }
            else if (temp < startPos - length)
            {
                startPositions[i] -= effectiveLength * 3;
            }
        }
    }
}