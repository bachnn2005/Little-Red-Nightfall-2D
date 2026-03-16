using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    #region 1. CONFIGURATION
    [Header("--- 1. Di Chuyển & Nhảy ---")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    public int maxJumpCount = 2;
    public float jumpCooldown = 0.2f;

    [Header("--- 2. Vật Lý ---")]
    public float acceleration = 20f;
    public float deceleration = 30f;
    public float slowFallGravity = 0.5f;
    public float slopeStickForce = 5f;

    [Header("--- 3. Ngưỡng Animation ---")]
    public float turnAnimThreshold = 1f;
    public float brakeEntrySpeed = 6f;
    public float brakeExitSpeed = 1.2f;

    [Header("--- 4. Check Đất & Dốc ---")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask solidGroundLayer;
    public LayerMask oneWayLayer;
    public float oneWayIgnoreTime = 0.3f;
    public Transform docCheckPoint;
    public float docCheckRadius = 0.2f;
    public LayerMask slopeLayer;

    [Header("--- 5. Hiệu Ứng Nước ---")]
    public float waterZDepth = 1.0f;
    public float waterZSpeed = 2f;
    public float minRotSpeed = 30f;
    public float maxRotSpeed = 120f;

    [Header("--- 6. Hiệu Ứng (Effects) ---")]
    public GameObject doubleJumpEffectPrefab;

    [Header("--- 7. Âm Thanh Bước Chân (Run Sounds) ---")]
    [Tooltip("Kéo 4 file âm thanh vào đây")]
    public AudioClip[] footstepSounds;

    [Range(0f, 1f)] public float footstepVolume = 0.5f;

    [Tooltip("Khoảng cách di chuyển (mét) để phát 1 tiếng")]
    public float footstepDistance = 2.5f;

    [Tooltip("Tốc độ tối thiểu để bắt đầu phát tiếng chạy (Tránh kêu khi nhích nhẹ)")]
    public float minRunSpeed = 1.0f; // <-- MỚI: Tốc độ phải > 1.0 mới kêu
    #endregion

    #region 2. STATE VARIABLES
    [HideInInspector] public bool isMovementLocked = false;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isOnSlope;
    [HideInInspector] public bool isDrowning = false;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerAttack attackScript;
    private PlayerStamina stamina;
    private AudioSource audioSource;

    private float horizontalInput;
    private float jumpStartTime;
    private int jumpsRemaining;
    private float defaultGravityScale;
    private float originalZ;
    private float currentRandomRotSpeed;
    private float currentTargetAngle;

    private float accumulatedDistance;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attackScript = GetComponent<PlayerAttack>();
        stamina = GetComponent<PlayerStamina>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        defaultGravityScale = rb.gravityScale;
        originalZ = transform.position.z;
    }

    void Update()
    {
        if (isDrowning) return;

        CheckEnvironment();
        HandleInput();
        HandleJump();
        UpdateAnimation();

        HandleFootsteps();
    }

    void FixedUpdate()
    {
        if (isDrowning)
        {
            ApplyDrowningPhysics();
            HandleZAxisMovement();
            HandleWaterRotation();
            return;
        }
        ApplyPhysicsMovement();
        HandleZAxisMovement();
    }

    // --- LOGIC ÂM THANH RUN ĐÃ SỬA ---
    void HandleFootsteps()
    {
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);

        // TRƯỜNG HỢP 1: ĐANG Ở DƯỚI ĐẤT
        if (isGrounded)
        {
            // Nếu đang chạy (Tốc độ > min)
            if (currentSpeed > minRunSpeed)
            {
                accumulatedDistance += currentSpeed * Time.deltaTime;

                if (accumulatedDistance >= footstepDistance)
                {
                    PlayRandomFootstep();
                    accumulatedDistance = 0f;
                }
            }
            // Nếu đang đứng yên (Tốc độ <= min)
            else
            {
                // Đứng yên thì nạp đầy bộ đếm
                // Để lát nữa vừa nhích chân chạy là KÊU LUÔN
                accumulatedDistance = footstepDistance;
            }
        }
        // TRƯỜNG HỢP 2: ĐANG Ở TRÊN KHÔNG (NHẢY/RƠI)
        else
        {
            // Khi đang bay, ta reset quãng đường về 0.
            // TẠI SAO? 
            // Để khi vừa tiếp đất, nó KHÔNG kêu tiếng bước chân ngay lập tức (tránh đè lên tiếng Land Sound).
            // Nhân vật sẽ phải chạy thêm 1 đoạn (footstepDistance) thì mới kêu tiếng bước chân đầu tiên sau khi đáp đất.
            accumulatedDistance = 0f;
        }
    }

    void PlayRandomFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(footstepSounds[randomIndex], footstepVolume);
    }
    // ---------------------------------

    void HandleJump()
    {
        if (isMovementLocked) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float cost = (jumpsRemaining == maxJumpCount) ? 10f : 5f;
            if (stamina != null && !stamina.HasEnoughStamina(cost)) return;

            if (isGrounded || isOnSlope)
            {
                if (stamina != null) stamina.UseStamina(cost);
                PerformJump();
            }
            else if (jumpsRemaining > 0)
            {
                if (stamina != null) stamina.UseStamina(cost);
                PerformJump();
                SpawnDoubleJumpEffect();
            }
        }
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }

    void PerformJump()
    {
        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpStartTime = Time.time;
        isGrounded = false;
        isOnSlope = false;
        jumpsRemaining--;
    }

    public void TriggerDrowning()
    {
        if (isDrowning) return;
        isDrowning = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.None;
        currentTargetAngle = (Random.Range(0, 2) == 0) ? -120f : 120f;
        currentRandomRotSpeed = Random.Range(minRotSpeed, maxRotSpeed);
        if (animator != null) animator.enabled = false;
        if (attackScript != null) attackScript.enabled = false;
    }

    void HandleWaterRotation() { float currentAngle = transform.eulerAngles.z; float newAngle = Mathf.MoveTowardsAngle(currentAngle, currentTargetAngle, currentRandomRotSpeed * Time.fixedDeltaTime); rb.MoveRotation(newAngle); }
    void HandleZAxisMovement() { float targetZ = isDrowning ? waterZDepth : originalZ; float currentZ = transform.position.z; if (Mathf.Abs(currentZ - targetZ) > 0.01f) { float newZ = Mathf.MoveTowards(currentZ, targetZ, waterZSpeed * Time.fixedDeltaTime); transform.position = new Vector3(transform.position.x, transform.position.y, newZ); } }
    void ApplyDrowningPhysics() { float stopSpeed = Mathf.MoveTowards(rb.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime); rb.linearVelocity = new Vector2(stopSpeed, rb.linearVelocity.y); }
    void CheckEnvironment() { isOnSlope = Physics2D.OverlapCircle(docCheckPoint.position, docCheckRadius, slopeLayer); bool touchingSolid = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, solidGroundLayer); if (touchingSolid) { isGrounded = true; ResetJumpCount(); return; } if (Time.time < jumpStartTime + oneWayIgnoreTime) { isGrounded = false; return; } bool touchingOneWay = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, oneWayLayer); if (touchingOneWay) { if (rb.linearVelocity.y <= 0.1f) { isGrounded = true; ResetJumpCount(); } else isGrounded = false; } else isGrounded = false; }
    void ResetJumpCount() { if (rb.linearVelocity.y <= 0.1f) jumpsRemaining = maxJumpCount; }
    void HandleInput() { horizontalInput = isMovementLocked ? 0 : Input.GetAxisRaw("Horizontal"); }
    void ApplyPhysicsMovement() { if (isOnSlope) { rb.gravityScale = 0f; if (horizontalInput == 0) rb.linearVelocity = Vector2.zero; else { float targetSpeed = horizontalInput * moveSpeed; float currentSpeed = rb.linearVelocity.x; float accelRate = (Mathf.Abs(targetSpeed) < 0.01f) ? deceleration : acceleration; float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime); rb.linearVelocity = new Vector2(newSpeed, -slopeStickForce); } return; } rb.gravityScale = defaultGravityScale; float flatTargetSpeed = horizontalInput * moveSpeed; float flatCurrentSpeed = rb.linearVelocity.x; float flatAccelRate = (Mathf.Abs(flatTargetSpeed) < 0.01f) ? deceleration : acceleration; float flatNewSpeed = Mathf.MoveTowards(flatCurrentSpeed, flatTargetSpeed, flatAccelRate * Time.fixedDeltaTime); rb.linearVelocity = new Vector2(flatNewSpeed, rb.linearVelocity.y); }
    void UpdateAnimation() { if (isMovementLocked) { animator.SetFloat("speed", 0); animator.SetBool("IsJumping", false); return; } if (horizontalInput != 0) transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1); animator.SetFloat("speed", Mathf.Abs(rb.linearVelocity.x)); animator.SetBool("IsJumping", !isGrounded); }
    public void StopImmediately() { rb.linearVelocity = Vector2.zero; horizontalInput = 0; }
    public void RestoreGravity() { rb.gravityScale = defaultGravityScale; }
    public void LungeForward(float force) { rb.linearVelocity = Vector2.zero; rb.AddForce(new Vector2(transform.localScale.x * force, 0), ForceMode2D.Impulse); }
    void SpawnDoubleJumpEffect() { if (doubleJumpEffectPrefab != null) Instantiate(doubleJumpEffectPrefab, groundCheckPoint.position, Quaternion.identity); }
}