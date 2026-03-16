using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    // Thời gian tồn tại (giây)
    public float delay = 0.5f;

    void Start()
    {
        // Tự sát sau khoảng thời gian delay
        Destroy(gameObject, delay);
    }
}