using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class AmeixaController : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip collectSound;

    private void Start()
    {

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource no encontrado!");
        }

        collectSound = Resources.Load<AudioClip>("Audio/FX/Coins");
        if (collectSound == null)
        {
            Debug.LogError("No se encontró el audio en Resources/Audio/FX/Coins");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAmeixa();
        }
        else
        {
            Debug.LogWarning("GameManager instance  no encontrada!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectAmeixa();
            }

            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            Destroy(gameObject, collectSound != null ? collectSound.length : 0.1f);
        }
    }
}