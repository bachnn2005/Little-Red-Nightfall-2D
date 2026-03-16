using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("--- CẤU HÌNH SPAWN ---")]
    [Tooltip("Danh sách các Prefab quái sẽ spawn ngẫu nhiên")]
    public List<GameObject> enemyPrefabs;
    [Tooltip("Vị trí spawn (Để trống sẽ lấy vị trí GameObject này)")]
    public Transform spawnPoint;

    [Header("--- THỜI GIAN ---")]
    public float respawnDelay = 10f;      // Thời gian chờ sau khi quái chết

    [Header("--- TĂNG TIẾN SỨC MẠNH (SCALING) ---")]
    public int spawnsToScale = 3;         // Cứ mỗi 3 con thì tăng sức mạnh
    public int bonusHealthPerScale = 20;  // Máu cộng thêm mỗi mốc
    public int bonusDamagePerScale = 5;   // Damage cộng thêm mỗi mốc
    public int extraXPOrbPerScale = 1;    // Số ngọc rơi thêm mỗi mốc

    [Header("--- THEO DÕI (Read Only) ---")]
    public int totalSpawns = 0;           // Tổng số quái đã spawn
    public GameObject currentEnemy;       // Con quái hiện tại đang sống

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Chưa gán Prefab quái vào Spawner!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // Vòng lặp vô tận để spawn liên tục
        while (true)
        {
            // 1. CHỜ: Đảm bảo không có quái nào đang sống trước khi spawn con mới
            // (Phòng trường hợp logic nào đó spawn chồng chéo)
            if (currentEnemy != null)
            {
                yield return null;
                continue;
            }

            // 2. CHỌN & SINH QUÁI
            GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            currentEnemy = Instantiate(randomPrefab, spawnPoint.position, Quaternion.identity);

            // Tăng số đếm
            totalSpawns++;

            // 3. TÍNH TOÁN BUFF
            // Công thức: (Lần spawn hiện tại / Mốc). Ví dụ spawn 1,2,3 -> Scale 1 (nếu tính >=). 
            // Logic của bạn: "mỗi 3 lần spawn sẽ spawn ra quái có thêm...".
            // Tức là con thứ 1,2,3: Thường. Con thứ 4,5,6: +1 Scale. 
            // Hoặc ý bạn là con thứ 3 đã được buff? Tôi sẽ dùng công thức (totalSpawns / spawnsToScale).
            // Ví dụ SpawnsToScale = 3.
            // Spawn 1: 1/3 = 0.
            // Spawn 2: 2/3 = 0.
            // Spawn 3: 3/3 = 1 -> Bắt đầu buff từ con thứ 3 trở đi.

            int scaleLevel = totalSpawns / spawnsToScale;

            if (scaleLevel > 0)
            {
                int healthBuff = scaleLevel * bonusHealthPerScale;
                int dmgBuff = scaleLevel * bonusDamagePerScale;
                int xpBuff = scaleLevel * extraXPOrbPerScale;

                // Áp dụng Buff Máu & XP
                EnemyHealth hpScript = currentEnemy.GetComponent<EnemyHealth>();
                if (hpScript != null)
                {
                    hpScript.ApplyBuffs(healthBuff, xpBuff);
                }

                // Áp dụng Buff Damage
                EnemyAttack atkScript = currentEnemy.GetComponent<EnemyAttack>();
                if (atkScript != null)
                {
                    atkScript.ApplyDamageBuff(dmgBuff);
                }

                // Debug để bạn tiện theo dõi
                // Debug.Log($"Spawner: Con thứ {totalSpawns} (Scale Lv {scaleLevel}). Buff: +{healthBuff} HP, +{dmgBuff} DMG, +{xpBuff} XP.");
            }

            // 4. CHỜ QUÁI CHẾT
            // Kiểm tra mỗi frame xem quái còn tồn tại không
            while (currentEnemy != null)
            {
                yield return null;
            }

            // 5. SAU KHI QUÁI CHẾT -> CHỜ 10 GIÂY
            // Debug.Log($"Quái đã bị hạ. Chờ {respawnDelay}s để spawn tiếp...");
            yield return new WaitForSeconds(respawnDelay);
        }
    }
}