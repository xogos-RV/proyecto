using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip loadedClip;
    private string currentlyPlayingClipName; // Track del clip actual

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void LoadClip(string clipName)
    {
        // Si ya está cargado y es el mismo, no hacer nada
        if (loadedClip != null && currentlyPlayingClipName == clipName)
        {
            return;
        }

        string path = "Audio/FX/" + clipName;
        loadedClip = Resources.Load<AudioClip>(path);

        if (loadedClip == null)
        {
            Debug.LogError($"Clip no encontrado: {path}");
        }
        else
        {
            currentlyPlayingClipName = clipName; // Actualizar el track
        }
    }

    public void Play(bool loop)
    {
        if (loadedClip == null)
        {
            audioSource.loop = loop;
            Debug.LogWarning("No hay clip cargado para reproducir");
            return;
        }

        // Si ya se está reproduciendo el mismo clip, no hacer nada
        if (audioSource.isPlaying && audioSource.clip == loadedClip)
        {
            return;
        }

        audioSource.clip = loadedClip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        currentlyPlayingClipName = null; // Resetear el track
    }

    public void TogglePause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }

}