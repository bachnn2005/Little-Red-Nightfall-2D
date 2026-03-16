using UnityEngine;
using System.Collections;

public class PlayerOneWayPlatform : MonoBehaviour
{
    private GameObject currentOneWayPlatform;

    // SỬA Ở ĐÂY: Đổi từ BoxCollider2D thành CapsuleCollider2D
    [SerializeField] private CapsuleCollider2D playerCollider;

    void Update()
    {
        // Kiểm tra nếu bấm S hoặc Mũi tên xuống
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentOneWayPlatform != null)
            {
                StartCoroutine(DisableCollision());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nhớ đặt Tag cho cái cầu là "OneWayPlatform"
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
        }
    }

    private IEnumerator DisableCollision()
    {
        // Lấy Collider của cái cầu (Tilemap Collider hoặc Composite Collider)
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

        // 1. Tạm thời tắt va chạm giữa Player và Cầu
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        // 2. Chờ 0.5 giây cho nhân vật rơi qua
        yield return new WaitForSeconds(0.5f);

        // 3. Bật lại va chạm
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}