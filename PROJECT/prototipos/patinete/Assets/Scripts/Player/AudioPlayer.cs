using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip loadedClip;
    private string currentlyPlayingClipName; // Track del clip actual

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
    }

    // Carga un clip desde Resources/Audio/FX/
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

    // Reproduce el clip cargado
    public void Play()
    {
        if (loadedClip == null)
        {
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

    // Reproduce un clip específico directamente (carga y reproduce)
    public void PlayOneShot(string clipName)
    {
        string path = "Audio/FX/" + clipName;
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip != null)
        {
            // Para OneShot no verificamos duplicados porque es por diseño
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"Clip no encontrado: {path}");
        }
    }

    // Detiene la reproducción
    public void Stop()
    {
        audioSource.Stop();
        currentlyPlayingClipName = null; // Resetear el track
    }

    // Pausa/Continúa la reproducción
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

    // Método adicional para verificar si un clip específico se está reproduciendo
    public bool IsPlayingClip(string clipName)
    {
        return audioSource.isPlaying && currentlyPlayingClipName == clipName;
    }
}