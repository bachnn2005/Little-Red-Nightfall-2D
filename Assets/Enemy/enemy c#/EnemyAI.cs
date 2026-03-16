using UnityEngine;

public class EnemyAI_Platformer : MonoBehaviour
{
    #region 1. CẤU HÌNH
    [Header("--- Loại Quái ---")]
    public bool canFly = false;

    [Header("--- Di Chuyển ---")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolRange = 4f;

    [Header("--- Tầm Nhìn & Tấn Công ---")]
    public float detectionRange = 5f;
    public float giveUpRange = 10f;   // Vòng Đỏ: Bức tường vô hình của quái

    // Khoảng cách dừng để AI bắt đầu kiểm tra đòn đánh
    public float stopAndAttackDistance = 1.2f;

    [Header("--- Nghỉ ---")]
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    #endregion

    #region 2. BIẾN
    private Vector2 startPos;
    private Vector2 patrolTarget;
    private float waitTimer;
    private bool isWaiting;

    public bool isChasing;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyAttack attackScript;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attackScript = GetComponent<EnemyAttack>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        startPos = transform.position;
        if (canFly) rb.gravityScale = 0; else rb.gravityScale = 1;

        PickNewPatrolPoint();
    }

    void OnValidate()
    {
        if (giveUpRange <= detectionRange) giveUpRange = detectionRange + 2f;
    }

    void Update()
    {
        if (player == null) return;

        if (attackScript != null && attackScript.isAttacking)
        {
            StopMoving();
            if (animator != null) animator.SetBool("IsMoving", false);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (isChasing)
        {
            // Thực hiện đuổi theo Player
            ChaseAndAttackBehavior(distToPlayer);

            // Nếu Player chạy quá xa thì ngừng đuổi
            if (distToPlayer > giveUpRange)
            {
                isChasing = false;
                PickNewPatrolPoint();
            }
        }
        else
        {
            // Chỉ đuổi theo (chưa đánh) khi lọt vào tầm mắt
            if (distToPlayer < detectionRange)
            {
                isChasing = true;
                isWaiting = false;
            }
            else
            {
                PatrolBehavior();
            }
        }

        UpdateAnimation();
    }

    // --- [SỬA ĐỔI LOGIC TẤN CÔNG] ---
    void ChaseAndAttackBehavior(float dist)
    {
        float actualAttackRange = 0f;

        if (attackScript != null)
        {
            // Lấy tầm đánh thực tế của kỹ năng đang sẵn sàng (ví dụ: 1m cho cận chiến)
            actualAttackRange = attackScript.GetOptimalStoppingDistance();
        }

        // CHỈ TẤN CÔNG khi khoảng cách <= tầm đánh thực tế
        if (dist <= actualAttackRange)
        {
            StopMoving();
            FaceTarget(player.position);

            if (attackScript != null)
            {
                attackScript.TryAttack();
            }
        }
        else
        {
            // Nếu ngoài tầm đánh -> CHỈ ĐUỔI THEO, không gọi lệnh đánh
            MoveToPoint(player.position, chaseSpeed);
        }
    }

    void PatrolBehavior()
    {
        float distToTarget = canFly ? Vector2.Distance(transform.position, patrolTarget) : Mathf.Abs(transform.position.x - patrolTarget.x);

        if (distToTarget < 0.2f)
        {
            StopMoving();
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    isWaiting = false;
                    PickNewPatrolPoint();
                }
            }
        }
        else
        {
            MoveToPoint(patrolTarget, moveSpeed);
        }
    }

    void MoveToPoint(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        float nextDistFromHome = Vector2.Distance(startPos, (Vector2)transform.position + direction * 0.1f);

        // Chặn di chuyển tại vòng đỏ (Logic bạn yêu cầu trước đó)
        if (nextDistFromHome > giveUpRange)
        {
            StopMoving();
            FaceTarget(player.position);

            // Ở biên giới vẫn có thể đánh nếu Player lọt vào attack range
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (isChasing && attackScript != null && distToPlayer <= attackScript.GetOptimalStoppingDistance())
            {
                attackScript.TryAttack();
            }
            return;
        }

        if (canFly)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            float xVel = Mathf.Sign(target.x - transform.position.x) * speed;
            rb.linearVelocity = new Vector2(xVel, rb.linearVelocity.y);
        }
        FaceTarget(target);
    }

    void StopMoving()
    {
        if (canFly) rb.linearVelocity = Vector2.zero;
        else rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void FaceTarget(Vector2 target)
    {
        if (target.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
        else if (target.x < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
    }

    void PickNewPatrolPoint()
    {
        if (Vector2.Distance(transform.position, startPos) > patrolRange)
        {
            patrolTarget = startPos;
            return;
        }
        if (canFly) patrolTarget = startPos + Random.insideUnitCircle * patrolRange;
        else patrolTarget = new Vector2(startPos.x + Random.Range(-patrolRange, patrolRange), transform.position.y);
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f || (canFly && rb.linearVelocity.magnitude > 0.1f);
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centerPos = Application.isPlaying ? (Vector3)startPos : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPos, giveUpRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(0.5f, 0, 0.5f, 1);
        Gizmos.DrawWireSphere(transform.position, stopAndAttackDistance);
    }
}