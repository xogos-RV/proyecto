using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    public ParticleSystem exposionParticles;
    public ParticleSystem dustEffect;
    public AudioClip impactSound;

    [Range(10, 50)] public int minStones = 15;
    [Range(10, 50)] public int maxStones = 25;
    [Range(0.05f, 0.3f)] public float minStoneSize = 0.1f;
    [Range(0.05f, 0.3f)] public float maxStoneSize = 0.2f;

    private bool hasCollided = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Floor") || hasCollided)
        {
            return;
        }
        
        hasCollided = true;
        // Configurar sistema principal de piedras
        var emission = exposionParticles.emission;
        emission.SetBurst(0, new ParticleSystem.Burst(0, Random.Range(minStones, maxStones)));

        var main = exposionParticles.main;
        main.startSize = new ParticleSystem.MinMaxCurve(minStoneSize, maxStoneSize);

        // Posicionar y activar efectos
        Vector3 impactPoint = collision.contacts[0].point;
        Quaternion impactRotation = Quaternion.LookRotation(collision.contacts[0].normal);

        exposionParticles.transform.position = impactPoint;
        exposionParticles.transform.rotation = impactRotation;
        exposionParticles.Play();

        if (dustEffect != null)
        {
            dustEffect.transform.position = impactPoint;
            dustEffect.Play();
        }

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, impactPoint);
        }

        // Ocultar el proyectil
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        // Destruir después de que terminen los efectos
        float destroyDelay = Mathf.Max(exposionParticles.main.duration, dustEffect != null ? dustEffect.main.duration : 0);
        Destroy(gameObject, destroyDelay);
    }

}