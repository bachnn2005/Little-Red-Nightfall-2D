using UnityEngine;

public class ParryEffectSound : MonoBehaviour
{
    [Header("Parry Success Sounds")]
    public AudioClip[] parrySounds;

    [Range(0f, 1f)]
    public float volume = 1f;

    void Start()
    {
        if (parrySounds == null || parrySounds.Length == 0) return;

        int index = Random.Range(0, parrySounds.Length);
        AudioClip clip = parrySounds[index];

        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }
}
