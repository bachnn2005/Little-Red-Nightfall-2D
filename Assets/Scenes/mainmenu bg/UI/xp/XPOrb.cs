using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("--- CẤU HÌNH XP ---")]
    public int xpAmount = 50;

    [Header("--- HÚT VÀO PLAYER ---")]
    public float attractionRange = 3f;
    public float attractionSpeed = 8f;
    public float pickUpDistance = 0.5f;
    [Tooltip("Thời gian sau khi sinh ra mới có thể nhặt được")]
    public float delayBeforePickup = 2f;

    [Header("--- HIỆU ỨNG BAY BỔNG ---")]
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 2f;

    private Transform playerTransform;
    private PlayerLevel playerLevel;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private bool isAttracted = false;
    private float spawnTime;
    private Vector3 startScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        startScale = transform.localScale;
        spawnTime = Time.time; // Ghi nhận thời gian sinh ra

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerLevel = player.GetComponent<PlayerLevel>();
        }
    }

    void Update()
    {
        if (playerTransform == null || playerLevel == null) return;

        // KIỂM TRA ĐIỀU KIỆN 2 GIÂY: Nếu chưa đủ thời gian thì không làm gì cả
        if (Time.time < spawnTime + delayBeforePickup)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Logic bắt đầu hút
        if (!isAttracted && distanceToPlayer <= attractionRange)
        {
            isAttracted = true;
            if (myCollider != null) myCollider.isTrigger = true; // Chuyển sang Trigger để bay mượt
            if (rb != null) rb.gravityScale = 0; // Tắt trọng lực khi đang bay
        }

        if (isAttracted)
        {
            MoveTowardsPlayer();
            // Khoảng cách nhặt được
            if (distanceToPlayer <= pickUpDistance)
            {
                Collect();
            }
        }
        else
        {
            ApplyFloatingEffect();
        }
    }

    void MoveTowardsPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);

        // Hiệu ứng thu nhỏ nhẹ khi lại gần
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        float scaleRatio = dist / attractionRange;
        transform.localScale = startScale * Mathf.Clamp(scaleRatio, 0.2f, 1f);
    }

    void ApplyFloatingEffect()
    {
        float newY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.Translate(Vector3.up * newY * Time.deltaTime);
    }

    void Collect()
    {
        if (playerLevel != null)
        {
            playerLevel.AddXP(xpAmount);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickUpDistance);
    }
}