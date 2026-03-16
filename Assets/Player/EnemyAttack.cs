using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAttack : MonoBehaviour
{
    [System.Serializable]
    public class AttackData
    {
        public string attackName = "Normal Attack";
        public string animName = "Attack";
        [Header("Thông số Quan trọng")]
        public float range = 1.0f; // Dùng cho hình tròn
        public int damage = 10;

        [Header("Cấu hình Tầm đánh (Hình chữ nhật)")]
        public bool isRanged = false; // Nếu Tick, sẽ dùng BoxSize thay vì Range
        public Vector2 boxSize = new Vector2(2f, 1f); // Kích thước hình chữ nhật (Dài, Cao)

        public float damageDelay = 0.4f;
        public float duration = 1.0f;
    }

    [System.Serializable]
    public class BossAttackData
    {
        public string attackName = "Boss Special";
        public string animName = "ChargeAnim";
        public float range = 2.0f;
        public int damage = 30;
    }

    [Header("1. Cấu hình Chung")]
    public Transform attackPoint;
    public LayerMask playerLayer;
    public string idleAnimName = "Main";

    [Header("Thời gian hồi chiêu (Random)")]
    public float minCooldown = 1.0f;
    public float maxCooldown = 2.0f;

    [Header("2. Cấu hình Đòn Đánh Thường")]
    public AttackData attack1;
    public bool useSecondAttack = false;
    public AttackData attack2;

    [Header("3. Cấu hình Boss Attack (Tụ lực)")]
    public bool boss1Attack = false;
    public BossAttackData attack3;
    [Header("Thời gian tụ lực (Random)")]
    public float minChargeTime = 1.0f;
    public float maxChargeTime = 2.5f;

    [HideInInspector] public bool isAttacking = false;
    private float nextAttackTime = 0f;
    private Animator animator;
    private EnemyHealth myHealth;
    private Transform playerTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        myHealth = GetComponent<EnemyHealth>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    public void TryAttack()
    {
        if (myHealth != null && myHealth.currentHealth <= 0) return;
        if (playerTransform == null) return;

        if (Time.time >= nextAttackTime && !isAttacking)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            List<int> availableAttacks = new List<int>();

            // Logic tầm xa để quyết định có đánh không (vẫn dùng range để đo khoảng cách kích hoạt)
            if (dist <= attack1.range) availableAttacks.Add(1);
            if (useSecondAttack && dist <= attack2.range) availableAttacks.Add(2);
            if (boss1Attack && dist <= attack3.range) availableAttacks.Add(3);

            if (availableAttacks.Count > 0)
            {
                int choice = availableAttacks[Random.Range(0, availableAttacks.Count)];
                if (choice == 1) StartCoroutine(PerformAttack(attack1));
                else if (choice == 2) StartCoroutine(PerformAttack(attack2));
                else if (choice == 3) StartCoroutine(PerformBossChargeAttack());

                nextAttackTime = Time.time + Random.Range(minCooldown, maxCooldown);
            }
        }
    }

    public void StopAttackImmediately() { StopAllCoroutines(); isAttacking = false; if (animator != null) animator.SetBool("Ready", false); }

    IEnumerator PerformAttack(AttackData data)
    {
        isAttacking = true;
        if (animator != null) animator.Play(data.animName);
        yield return new WaitForSeconds(data.damageDelay);

        // --- GỌI HÀM GÂY SÁT THƯƠNG MỚI ---
        // Truyền đầy đủ thông tin để biết dùng Box hay Circle
        DealDamage(data.range, data.damage, data.isRanged, data.boxSize);

        yield return new WaitForSeconds(data.duration - data.damageDelay);
        if (animator != null) animator.Play(idleAnimName);
        isAttacking = false;
    }

    IEnumerator PerformBossChargeAttack()
    {
        isAttacking = true;
        if (animator != null) animator.SetBool("Ready", false);
        if (animator != null) animator.Play(attack3.animName);

        float chargeTime = Random.Range(minChargeTime, maxChargeTime);
        yield return new WaitForSeconds(chargeTime);

        if (myHealth != null && myHealth.currentHealth <= 0) yield break;

        if (animator != null)
        {
            animator.SetBool("Ready", true);
            yield return null;
        }

        // Boss Attack mặc định vẫn dùng hình tròn (false, zero)
        DealDamage(attack3.range, attack3.damage, false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        if (animator != null)
        {
            animator.SetBool("Ready", false);
            animator.Play(idleAnimName);
        }
        isAttacking = false;
    }

    // --- HÀM GÂY DAMAGE ĐÃ ĐƯỢC NÂNG CẤP ---
    void DealDamage(float range, int damage, bool useBox, Vector2 boxSize)
    {
        if (myHealth != null && myHealth.currentHealth <= 0) return;

        Collider2D playerHit = null;

        if (useBox)
        {
            // 
            // Dùng hình chữ nhật nếu tick isRanged
            // Góc xoay 0f vì thường đánh thẳng
            playerHit = Physics2D.OverlapBox(attackPoint.position, boxSize, 0f, playerLayer);
        }
        else
        {
            // 
            // Dùng hình tròn mặc định
            playerHit = Physics2D.OverlapCircle(attackPoint.position, range, playerLayer);
        }

        if (playerHit != null)
        {
            PlayerHealth pHealth = playerHit.GetComponent<PlayerHealth>();
            if (pHealth != null) pHealth.TakeDamage(damage, transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // Vẽ Attack 1
        Gizmos.color = Color.red;
        if (attack1.isRanged)
            Gizmos.DrawWireCube(attackPoint.position, attack1.boxSize);
        else
            Gizmos.DrawWireSphere(attackPoint.position, attack1.range);

        // Vẽ Attack 2
        if (useSecondAttack)
        {
            Gizmos.color = Color.yellow;
            if (attack2.isRanged)
                Gizmos.DrawWireCube(attackPoint.position, attack2.boxSize);
            else
                Gizmos.DrawWireSphere(attackPoint.position, attack2.range);
        }

        // Vẽ Attack Boss (Mặc định tròn)
        if (boss1Attack)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attack3.range);
        }
    }

    public float GetOptimalStoppingDistance()
    {
        float maxRange = attack1.range;
        if (useSecondAttack) maxRange = Mathf.Max(maxRange, attack2.range);
        if (boss1Attack) maxRange = Mathf.Max(maxRange, attack3.range);
        return maxRange;
    }

    public void ApplyDamageBuff(int bonusDamage)
    {
        attack1.damage += bonusDamage;
        if (useSecondAttack) attack2.damage += bonusDamage;
        if (boss1Attack) attack3.damage += bonusDamage;
    }
}