using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Cấu hình Máu")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Cấu hình Hồi máu")]
    public bool canRegenerate = true;
    public int regenAmount = 5;
    public float regenInterval = 1f;
    public float timeBeforeRegen = 10f;

    [Header("Cấu hình Hiệu ứng Nhấp nháy")]
    public Material flashMaterial;
    public Color flashColor = Color.magenta;
    public float flashDuration = 0.1f;

    [Header("Cấu hình Cái Chết")]
    public string deathAnimName = "Death";
    public bool useExtraDelay = true;
    public float extraDelayValue = 0.5f;

    [Header("Cấu hình Rơi XP")]
    public GameObject xpOrbPrefab;
    public float scatterForce = 5f;

    // --- MỚI: Biến lưu số XP Orb rơi thêm ---
    private int extraXPOrbs = 0;

    private float calculatedDestroyDelay = 0f;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer[] allSpriteRenderers;
    private Material[] originalMaterials;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private bool isDead = false;

    private float lastDamageTime;
    private float nextRegenTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

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
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || !canRegenerate || currentHealth >= maxHealth) return;

        if (Time.time >= lastDamageTime + timeBeforeRegen)
        {
            if (Time.time >= nextRegenTime)
            {
                currentHealth += regenAmount;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                nextRegenTime = Time.time + regenInterval;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || currentHealth <= 0) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        if (allSpriteRenderers.Length > 0 && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashEffect()
    {
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            if (allSpriteRenderers[i] != null)
            {
                allSpriteRenderers[i].material = flashMaterial;
                allSpriteRenderers[i].color = flashColor;
            }
        }
        yield return new WaitForSeconds(flashDuration);
        ResetAppearance();
        flashCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        SpawnXP();

        if (GetComponent<EnemyAI_Platformer>() != null)
            GetComponent<EnemyAI_Platformer>().enabled = false;

        EnemyAttack attackScript = GetComponent<EnemyAttack>();
        if (attackScript != null)
        {
            attackScript.StopAttackImmediately();
            attackScript.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        if (animator != null)
        {
            animator.Play(deathAnimName);
            calculatedDestroyDelay = GetAnimationLength(deathAnimName);
            if (calculatedDestroyDelay == 0) calculatedDestroyDelay = 1f;
        }
        else
        {
            calculatedDestroyDelay = 0.5f;
        }

        float extraTime = useExtraDelay ? extraDelayValue : 0f;
        Destroy(gameObject, calculatedDestroyDelay + extraTime);
    }

    void SpawnXP()
    {
        if (xpOrbPrefab != null)
        {
            // --- LOGIC MỚI: Spawn 1 cục gốc + số lượng thêm ---
            int totalToSpawn = 1 + extraXPOrbs;

            for (int i = 0; i < totalToSpawn; i++)
            {
                // Thêm chút offset random để các cục XP không chồng khít lên nhau
                Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.2f, 0.8f), 0);

                GameObject orb = Instantiate(xpOrbPrefab, transform.position + randomOffset, Quaternion.identity);
                Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
                if (orbRb != null)
                {
                    Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
                    orbRb.AddForce(randomDir * scatterForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    void ResetAppearance()
    {
        if (allSpriteRenderers == null) return;
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            if (allSpriteRenderers[i] != null && originalMaterials[i] != null)
            {
                allSpriteRenderers[i].material = originalMaterials[i];
                allSpriteRenderers[i].color = originalColors[i];
            }
        }
    }

    float GetAnimationLength(string animName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0;
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animName) return clip.length;
        }
        return 0;
    }

    // --- MỚI: HÀM NHẬN BUFF TỪ SPAWNER ---
    public void ApplyBuffs(int bonusHealth, int extraOrbs)
    {
        maxHealth += bonusHealth;
        currentHealth = maxHealth; // Hồi đầy theo max mới
        extraXPOrbs = extraOrbs;
    }
}