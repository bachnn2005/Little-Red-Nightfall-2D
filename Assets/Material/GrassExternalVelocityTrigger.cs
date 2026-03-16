using System.Collections;
using UnityEngine;

public class GrassExternalVelocityTrigger : MonoBehaviour
{
    private GrassVelocityController _grassVelocityController;
    private Material _material;

    private bool _easeInCoroutineRunning;
    private bool _easeOutCoroutineRunning;
    private int _externalInfluence = Shader.PropertyToID("_ExternalInfluence");

    // Biến lưu giá trị gốc (Ví dụ: 1)
    private float _startingXVelocity;

    private void Start()
    {
        _grassVelocityController = GetComponentInParent<GrassVelocityController>();
        if (_grassVelocityController == null)
            _grassVelocityController = GetComponent<GrassVelocityController>();

        _material = GetComponent<SpriteRenderer>().material;

        // Lưu lại độ nghiêng mặc định
        _startingXVelocity = _material.GetFloat(_externalInfluence);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (_easeOutCoroutineRunning) StopCoroutine("EaseOut");

            // Nếu Player ở bên trái -> Đẩy hướng dương (+1). 
            // Nếu Player ở bên phải -> Đẩy hướng âm (-1).
            float direction = (collision.transform.position.x < transform.position.x) ? 1f : -1f;

            // Tính toán đích đến = Gốc + (Hướng * Sức mạnh)
            float targetValue = _startingXVelocity + (direction * _grassVelocityController.ExternalInfluenceStrength);

            StartCoroutine(EaseIn(targetValue));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (_easeInCoroutineRunning) StopCoroutine("EaseIn");

            // Trả về đúng vị trí gốc ban đầu
            StartCoroutine(EaseOut(_startingXVelocity));
        }
    }

    private IEnumerator EaseIn(float target)
    {
        _easeInCoroutineRunning = true;
        float elapsedTime = 0f;
        float startValue = _material.GetFloat(_externalInfluence);
        float duration = _grassVelocityController.EaseInTime;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, target, elapsedTime / duration);
            _grassVelocityController.InfluenceGrass(_material, newValue);
            yield return null;
        }

        _grassVelocityController.InfluenceGrass(_material, target);
        _easeInCoroutineRunning = false;
    }

    private IEnumerator EaseOut(float target)
    {
        _easeOutCoroutineRunning = true;
        float elapsedTime = 0f;
        float startValue = _material.GetFloat(_externalInfluence);
        float duration = _grassVelocityController.EaseOutTime;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, target, elapsedTime / duration);
            _grassVelocityController.InfluenceGrass(_material, newValue);
            yield return null;
        }

        _grassVelocityController.InfluenceGrass(_material, target);
        _easeOutCoroutineRunning = false;
    }
}