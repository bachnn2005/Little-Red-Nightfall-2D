using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    [Header("1. Mục tiêu theo dõi")]
    public Transform target;       // Kéo nhân vật vào đây
    public float smoothSpeed = 0.125f; // Độ mượt (0.1 -> 1)
    public Vector3 offset = new Vector3(0, 0, -10); // Khoảng cách (Z luôn phải âm)

    [Header("2. Giới hạn Map (Kéo Collider xanh vào)")]
    public BoxCollider2D mapBounds;

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Tính toán kích thước Camera
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    // Dùng LateUpdate để chạy sau cùng, tránh rung lắc
    void LateUpdate()
    {
        // --- BƯỚC 1: DI CHUYỂN THEO NHÂN VẬT ---
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            // Lerp giúp camera trôi mượt mà thay vì giật cục
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }

        // --- BƯỚC 2: KẸP VỊ TRÍ LẠI (CLAMP) ---
        if (mapBounds != null)
        {
            // Cập nhật lại kích thước cam (đề phòng đổi độ phân giải lúc chơi)
            camHalfHeight = cam.orthographicSize;
            camHalfWidth = camHalfHeight * cam.aspect;

            // Tính toán vùng an toàn
            // (Lấy biên của BoxCollider - một nửa kích thước Camera)
            float minX = mapBounds.bounds.min.x + camHalfWidth;
            float maxX = mapBounds.bounds.max.x - camHalfWidth;
            float minY = mapBounds.bounds.min.y + camHalfHeight;
            float maxY = mapBounds.bounds.max.y - camHalfHeight;

            // Lấy vị trí vừa tính được ở Bước 1
            Vector3 currentPos = transform.position;

            // Kẹp chặt nó lại
            // Mathf.Clamp(giá trị, thấp nhất, cao nhất)
            // Logic bảo vệ: Nếu Map bé hơn Camera (min > max) thì ưu tiên lấy tâm map
            if (minX > maxX) currentPos.x = mapBounds.bounds.center.x;
            else currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);

            if (minY > maxY) currentPos.y = mapBounds.bounds.center.y;
            else currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

            // Gán ngược lại vào Camera
            transform.position = currentPos;
        }
    }

    // Vẽ khung đỏ trong Editor để dễ nhìn vùng Camera được phép đi
    void OnDrawGizmos()
    {
        if (mapBounds != null && cam != null)
        {
            float h = cam.orthographicSize;
            float w = h * cam.aspect;

            float minX = mapBounds.bounds.min.x + w;
            float maxX = mapBounds.bounds.max.x - w;
            float minY = mapBounds.bounds.min.y + h;
            float maxY = mapBounds.bounds.max.y - h;

            Gizmos.color = Color.red;
            // Vẽ vùng tâm camera được phép di chuyển
            Gizmos.DrawLine(new Vector3(minX, minY), new Vector3(maxX, minY));
            Gizmos.DrawLine(new Vector3(maxX, minY), new Vector3(maxX, maxY));
            Gizmos.DrawLine(new Vector3(maxX, maxY), new Vector3(minX, maxY));
            Gizmos.DrawLine(new Vector3(minX, maxY), new Vector3(minX, minY));
        }
    }
}