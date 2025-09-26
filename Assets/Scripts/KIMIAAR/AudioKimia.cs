using UnityEngine;

public class AudioKimia : MonoBehaviour
{
    public static AudioKimia Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;          // untuk musik latar
    public AudioSource[] sfxSources;       // isi banyak AudioSource SFX

    void Awake()
    {
        Instance = this;
    }

    // === BGM ===
    public void PlayBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // === SFX berdasarkan index ===
    public void PlaySFX(int index)
    {
        if (index >= 0 && index < sfxSources.Length)
        {
            sfxSources[index].Play();
        }
        else
        {
            Debug.LogWarning("Index SFX tidak valid!");
        }
    }
}
