using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("1. Cấu hình Máu")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;

    [Header("1.1 Cấu hình Hồi máu")]
    public bool canRegenerate = true;
    public int regenAmount = 1;
    public float regenInterval = 1f;

    [Header("1.2 Cấu hình Hồi sinh")]
    [Tooltip("Thời gian chờ trước khi nhân vật xuất hiện lại")]
    public float respawnDelay = 1.5f;
    private Vector3 startPosition; // Lưu điểm hồi sinh đầu tiên

    [Header("2. Hiệu ứng Nhấp nháy")]
    public Material flashMaterial;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("3. Hiệu ứng khi Chết")]
    public GameObject deathEffectPrefab;
    public Vector3 deathEffectOffset = Vector3.zero;

    [Header("4. Vật lý & iframe")]
    public float iframeDuration = 0.5f;
    public float hurtStunDuration = 0.3f;
    public float knockbackForce = 10f;
    public float knockbackUpRatio = 0.5f;

    private bool isInvulnerable = false;
    private bool isDead = false;

    private Animator animator;
    private PlayerMovement movementScript;
    private Rigidbody2D rb;
    private PlayerAttack attackScript;

    private SpriteRenderer[] allSpriteRenderers;
    private Material[] originalMaterials;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private float regenTimer;

    void Start()
    {
        currentHealth = maxHealth;
        // Lưu vị trí ban đầu làm điểm hồi sinh
        startPosition = transform.position;

        if (healthSlider != null) { healthSlider.maxValue = maxHealth; healthSlider.value = currentHealth; }

        animator = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        attackScript = GetComponent<PlayerAttack>();

        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalMaterials = new Material[allSpriteRenderers.Length];
        originalColors = new Color[allSpriteRenderers.Length];

        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            if (allSpriteRenderers[i] != null)
            {
                originalMaterials[i] = allSpriteRenderers[i].material;
                originalColors[i] = allSpriteRenderers[i].color;
            }
        }
    }

    void Update()
    {
        if (isDead) return;
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(10, null);
        HandleRegeneration();
    }

    private void HandleRegeneration()
    {
        if (!canRegenerate || currentHealth >= maxHealth || currentHealth <= 0) return;
        regenTimer += Time.deltaTime;
        if (regenTimer >= regenInterval)
        {
            currentHealth += regenAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            if (healthSlider != null) healthSlider.value = currentHealth;
            regenTimer = 0f;
        }
    }

    public void TakeDamage(int damage, Transform source)
    {
        if (isInvulnerable || currentHealth <= 0 || isDead) return;

        if (attackScript != null && attackScript.isParrying)
        {
            if (source != null && Mathf.Sign(source.position.x - transform.position.x) == Mathf.Sign(transform.localScale.x))
            {
                attackScript.OnParrySuccess();
                return;
            }
        }

        currentHealth -= damage;
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (allSpriteRenderers.Length > 0 && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator != null) animator.SetTrigger("Hurt");
        ApplyHurtPhysics(source);
        StartCoroutine(RecoverFromStun());
        StartCoroutine(InvulnerabilityTimer());
    }

    private void ApplyHurtPhysics(Transform source)
    {
        if (movementScript != null)
        {
            movementScript.StopImmediately();
            movementScript.isMovementLocked = true;
        }
        if (attackScript != null) attackScript.ForceStopAttack();

        if (source != null && rb != null)
        {
            Vector2 direction = (transform.position - source.position).normalized;
            Vector2 knockbackDir = new Vector2(direction.x, knockbackUpRatio).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private IEnumerator FlashEffect()
    {
        foreach (var sr in allSpriteRenderers)
        {
            if (sr != null) { sr.material = flashMaterial; sr.color = flashColor; }
        }
        yield return new WaitForSeconds(flashDuration);
        ResetAppearance();
        flashCoroutine = null;
    }

    void ResetAppearance()
    {
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            if (allSpriteRenderers[i] != null)
            {
                allSpriteRenderers[i].material = originalMaterials[i];
                allSpriteRenderers[i].color = originalColors[i];
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (healthSlider != null) healthSlider.value = 0;

        // Hiệu ứng chết
        if (deathEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + deathEffectOffset;
            Instantiate(deathEffectPrefab, spawnPosition, transform.rotation);
        }

        // Bắt đầu quy trình hồi sinh thay vì xóa Object
        StartCoroutine(RespawnProcess());
    }

    private IEnumerator RespawnProcess()
    {
        // 1. Ẩn nhân vật và khóa logic
        SetPlayerActive(false);

        // 2. Chờ thời gian delay
        yield return new WaitForSeconds(respawnDelay);

        // 3. Đưa về vị trí bắt đầu
        transform.position = startPosition;

        // 4. Khôi phục máu và trạng thái
        currentHealth = maxHealth;
        isDead = false;
        if (healthSlider != null) healthSlider.value = currentHealth;
        ResetAppearance();

        // 5. Hiện lại nhân vật
        SetPlayerActive(true);

        // Cấp iframe ngắn sau khi hồi sinh để tránh chết ngay lập tức
        StartCoroutine(InvulnerabilityTimer());
    }

    private void SetPlayerActive(bool active)
    {
        // Ẩn/Hiện hình ảnh
        foreach (var sr in allSpriteRenderers) if (sr != null) sr.enabled = active;

        // Khóa/Mở di chuyển và vật lý
        if (movementScript != null)
        {
            movementScript.StopImmediately();
            movementScript.enabled = active;
            movementScript.isMovementLocked = !active;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = active; // Tắt giả lập vật lý khi đang chờ hồi sinh
        }

        if (attackScript != null) attackScript.enabled = active;

        // Vô hiệu hóa Collider để quái không đẩy được xác
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = active;
    }

    IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(hurtStunDuration);
        if (movementScript != null) movementScript.isMovementLocked = false;
    }

    IEnumerator InvulnerabilityTimer()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(iframeDuration);
        isInvulnerable = false;
    }
}