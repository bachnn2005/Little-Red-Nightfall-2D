using UnityEngine;

public class WaterZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                // Nếu player chưa chết thì mới xử lý
                if (health.currentHealth > 0)
                {
                    // Chết ngay lập tức
                    health.currentHealth = 0;
                    health.Die();
                }
            }
        }
    }
}