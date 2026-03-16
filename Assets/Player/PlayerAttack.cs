using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    // Class con để gom nhóm Hiệu ứng + Vị trí
    [System.Serializable]
    public class ParryEffectItem
    {
        public string name;
        public GameObject effectPrefab;
        public Transform customPosition;
    }

    #region 1. CONFIGURATION

    [Header("--- TẤN CÔNG (COMBO) ---")]
    public float attackDelay = 0.2f;
    public float comboCooldown = 1.0f;
    public float comboResetTime = 1.0f;

    [Header("--- LỰC ĐẨY (LUNGE) ---")]
    public float groundLunge1 = 5f;
    public float groundLunge2 = 5f;
    public float groundLunge3 = 8f;
    public float airLunge1 = 3f;
    public float airLunge2 = 3f;
    public float airLunge3 = 5f;

    [Header("--- HITBOX & SÁT THƯƠNG ---")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;

    [Tooltip("Sát thương cộng thêm từ trang bị hoặc buff")]
    public int bonusAttackDamage = 0;

    public int damage1 = 20;
    public int damage2 = 30;
    public int damage3 = 50;

    [Header("--- VISUAL EFFECT (ATTACK) ---")]
    public GameObject hitEffectPrefab;
    public float hitSpreadRadius = 0.5f;
    public float hitAngleVariance = 20f;

    [Header("--- PARRY (ĐỠ ĐÒN) ---")]
    public string parryAnimName = "Parry";
    public float parryWindow = 0.2f;
    public float parryDuration = 0.6f;
    public float parryCooldown = 1.0f;
    public float parryEffectLifetime = 0.5f;
    public ParryEffectItem[] parryEffectsList;

    // --- AUDIO SECTION (ĐÃ NÂNG CẤP) ---
    [Header("--- AUDIO (ÂM THANH) ---")]
    public AudioSource audioSource;

    [Header("1. Âm thanh Tấn công (Swing)")]
    [Tooltip("Danh sách tiếng vung kiếm (Sẽ random mỗi lần chém)")]
    public AudioClip[] attackSwingSounds; // Tiếng vung kiếm
    [Range(0f, 1f)] public float swingVolume = 0.8f;

    [Header("2. Âm thanh Trúng quái (Hit)")]
    [Tooltip("Tiếng khi chém trúng thịt/quái")]
    public AudioClip[] attackHitSounds;   // Tiếng chém trúng
    [Range(0f, 1f)] public float hitVolume = 1f;

    [Header("3. Âm thanh Parry")]
    public AudioClip parryStartSound;
    public AudioClip[] parrySuccessSounds;
    public AudioClip parryMissSound;

    #endregion

    #region 2. STATE VARIABLES

    private int comboStep = 0;
    private float nextAttackTime = 0f;
    private float lastAttackTime = 0f;
    private float nextParryTime = 0f;

    private bool isAttacking = false;
    private bool inputBuffered = false;
    private bool airComboFinished = false;

    [HideInInspector] public bool isParrying = false;
    private bool hasParriedHit = false;

    private Animator animator;
    private PlayerMovement movement;
    private PlayerStamina stamina;

    #endregion

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        stamina = GetComponent<PlayerStamina>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (movement.isMovementLocked && !isAttacking && !isParrying) return;

        if (movement.isGrounded) airComboFinished = false;

        if (Time.time - lastAttackTime > comboResetTime && comboStep != 0 && !isAttacking)
        {
            comboStep = 0;
        }

        if (isAttacking || isParrying)
        {
            float h = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(h) > 0.1f) transform.localScale = new Vector3(Mathf.Sign(h), 1, 1);
        }

        // --- 2. PARRY (Chuột Phải) ---
        if (Input.GetMouseButtonDown(1) && !isAttacking && !isParrying && Time.time >= nextParryTime && movement.isGrounded)
        {
            if (stamina != null && stamina.HasEnoughStamina(10f))
            {
                StartCoroutine(PerformParry());
            }
        }

        // --- 3. TẤN CÔNG (Chuột Trái) ---
        if (Input.GetMouseButtonDown(0) && !isParrying)
        {
            if (!movement.isGrounded && airComboFinished) return;

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                if (stamina != null && stamina.HasEnoughStamina(10f))
                {
                    PerformAttack();
                }
            }
            else
            {
                if (isAttacking) inputBuffered = true;
            }
        }
    }

    #region 3. PARRY LOGIC (GIỮ NGUYÊN)

    IEnumerator PerformParry()
    {
        if (stamina != null) stamina.UseStamina(10f);

        isParrying = true;
        hasParriedHit = false;
        nextParryTime = Time.time + parryCooldown;

        // Phát tiếng Parry bắt đầu
        PlaySound(parryStartSound);

        movement.isMovementLocked = true;
        movement.StopImmediately();

        if (animator != null) animator.Play(parryAnimName);

        yield return new WaitForSeconds(parryWindow);

        isParrying = false;

        if (!hasParriedHit)
        {
            // Phát tiếng Parry trượt
            PlaySound(parryMissSound);
        }

        float remainingTime = parryDuration - parryWindow;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        movement.isMovementLocked = false;
        if (animator != null) animator.Play("Player_Idle");
    }

    public void OnParrySuccess()
    {
        hasParriedHit = true;

        if (stamina != null) stamina.RestoreStamina(20f);

        // Phát tiếng Parry thành công (Random)
        PlayRandomSound(parrySuccessSounds);

        if (parryEffectsList != null && parryEffectsList.Length > 0)
        {
            foreach (ParryEffectItem item in parryEffectsList)
            {
                if (item.effectPrefab != null)
                {
                    Vector3 spawnPos = item.customPosition != null ? item.customPosition.position : transform.position;
                    GameObject spark = Instantiate(item.effectPrefab, spawnPos, Quaternion.identity);
                    StartCoroutine(StopSparkGently(spark, parryEffectLifetime));
                }
            }
        }
    }

    IEnumerator StopSparkGently(GameObject sparkObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sparkObj != null)
        {
            var ps = sparkObj.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
        }
    }

    public void ForceStopAttack()
    {
        StopAllCoroutines();
        isAttacking = false;
        isParrying = false;
        inputBuffered = false;
    }

    #endregion

    #region 4. ATTACK LOGIC (ĐÃ THÊM AUDIO)

    void PerformAttack()
    {
        if (stamina != null) stamina.UseStamina(10f);

        // --- PHÁT TIẾNG VUNG KIẾM (SWING) ---
        // Random tiếng vung kiếm để nghe sinh động hơn
        PlayRandomSound(attackSwingSounds, swingVolume, true);

        inputBuffered = false;
        isAttacking = true;
        lastAttackTime = Time.time;

        comboStep++;
        if (comboStep > 3) comboStep = 1;

        if (comboStep == 3) nextAttackTime = Time.time + comboCooldown;
        else nextAttackTime = Time.time + attackDelay;

        // TÍNH DAMAGE
        int finalDamage = 0;
        if (comboStep == 1) finalDamage = damage1 + bonusAttackDamage;
        else if (comboStep == 2) finalDamage = damage2 + bonusAttackDamage;
        else if (comboStep == 3) finalDamage = damage3 + bonusAttackDamage;

        Vector2 pos = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(pos, attackRange, enemyLayer);

        bool hitAnything = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth eHealth = enemy.GetComponent<EnemyHealth>();
            if (eHealth != null)
            {
                eHealth.TakeDamage(finalDamage);
                hitAnything = true; // Đánh dấu là đã trúng ít nhất 1 con

                // Hiệu ứng hình ảnh Hit Effect
                if (hitEffectPrefab != null)
                {
                    Vector2 enemyCenter = enemy.bounds.center;
                    Vector2 randomSpread = Random.insideUnitCircle * hitSpreadRadius;
                    Vector2 finalPos = enemyCenter + randomSpread;
                    float angle1 = Random.Range(0f, 360f);
                    float variance = Random.Range(-hitAngleVariance, hitAngleVariance);
                    float angle2 = angle1 + 90f + variance;
                    Instantiate(hitEffectPrefab, finalPos, Quaternion.Euler(0, 0, angle1));
                    Instantiate(hitEffectPrefab, finalPos, Quaternion.Euler(0, 0, angle2));
                }
            }
        }

        // --- PHÁT TIẾNG CHÉM TRÚNG (HIT) ---
        // Chỉ phát nếu hitAnything == true
        if (hitAnything)
        {
            PlayRandomSound(attackHitSounds, hitVolume, true);
        }

        movement.isMovementLocked = true;
        movement.RestoreGravity();

        if (movement.isGrounded)
        {
            if (comboStep == 1) movement.LungeForward(groundLunge1);
            else if (comboStep == 2) movement.LungeForward(groundLunge2);
            else if (comboStep == 3) movement.LungeForward(groundLunge3);
        }
        else
        {
            if (comboStep == 1) movement.LungeForward(airLunge1);
            else if (comboStep == 2) movement.LungeForward(airLunge2);
            else if (comboStep == 3) movement.LungeForward(airLunge3);
        }

        if (comboStep == 1) animator.Play("Player_Attack1");
        else if (comboStep == 2) animator.Play("Player_Attack2");
        else if (comboStep == 3) animator.Play("Player_Attack3");
    }

    public void EndAttack()
    {
        isAttacking = false;
        movement.isMovementLocked = false;
        movement.RestoreGravity();

        if (comboStep == 3)
        {
            comboStep = 0;
            inputBuffered = false;
            if (!movement.isGrounded) airComboFinished = true;
            animator.Play("Player_Idle");
            return;
        }

        if (inputBuffered)
        {
            inputBuffered = false;
            if (Time.time >= nextAttackTime)
            {
                if (stamina != null && stamina.HasEnoughStamina(10f)) PerformAttack();
                else animator.Play("Player_Idle");
            }
            else StartCoroutine(WaitForNextAttack());
        }
        else
        {
            if (!movement.isGrounded) airComboFinished = true;
            animator.Play("Player_Idle");
        }
    }

    IEnumerator WaitForNextAttack()
    {
        isAttacking = true;
        while (Time.time < nextAttackTime)
        {
            float h = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(h) > 0.1f) transform.localScale = new Vector3(Mathf.Sign(h), 1, 1);
            yield return null;
        }

        if (stamina != null && stamina.HasEnoughStamina(10f)) PerformAttack();
        else { isAttacking = false; animator.Play("Player_Idle"); }
    }

    #endregion

    // --- CÁC HÀM HỖ TRỢ ÂM THANH MỚI ---
    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    void PlayRandomSound(AudioClip[] clips, float volume = 1f, bool randomPitch = false)
    {
        if (clips != null && clips.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, clips.Length);
            if (clips[index] != null)
            {
                // Random Pitch nhẹ (0.9 - 1.1) để tiếng nghe tự nhiên hơn
                audioSource.pitch = randomPitch ? Random.Range(0.9f, 1.1f) : 1f;
                audioSource.PlayOneShot(clips[index], volume);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}