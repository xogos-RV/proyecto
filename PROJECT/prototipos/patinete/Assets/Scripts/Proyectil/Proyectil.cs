using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    public GameObject explosionParticlesPrefab;
    public GameObject dustEffectPrefab;
    public AudioClip impactSound;

    [Range(10, 500)] public int minParticles = 25;
    [Range(10, 500)] public int maxParticles = 200;
    [Range(0.1f, 10f)] public float minParticleSize = 0.5f;
    [Range(0.1f, 10f)] public float maxParticleSize = 5f;

    public bool isTrigger = false;
    private bool hasCollided = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!isTrigger && !ShouldIgnoreCollision(collision.gameObject))
        {
            HandleImpact(collision.contacts[0].point, collision.contacts[0].normal);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (isTrigger && !ShouldIgnoreCollision(other.gameObject))
        {
            Vector3 impactPoint = GetTriggerImpactPoint(other);
            Vector3 impactNormal = (transform.position - other.transform.position).normalized;
            HandleImpact(impactPoint, impactNormal);
        }
    }



    private bool ShouldIgnoreCollision(GameObject otherObject)
    {
        return otherObject.CompareTag("Player") ||
                otherObject.CompareTag("Floor") ||
                otherObject.CompareTag("Proyectil") ||
                hasCollided;
    }



    private Vector3 GetTriggerImpactPoint(Collider other)
    {
        // Opción 1: Usar el punto más cercano en el collider
        Vector3 closestPoint = other.ClosestPoint(transform.position);

        // Opción 2: Usar Raycast para mayor precisión
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f)) // TODO ajustar
        {
            return hit.point;
        }

        return closestPoint;
    }



    private void HandleImpact(Vector3 impactPoint, Vector3 impactNormal)
    {
        if (hasCollided) return;

        hasCollided = true;

        if (explosionParticlesPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionParticlesPrefab, impactPoint, Quaternion.LookRotation(impactNormal));
            ParticleSystem explosionParticles = explosionInstance.GetComponent<ParticleSystem>();

            var emission = explosionParticles.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0, Random.Range(minParticles, maxParticles)));

            var main = explosionParticles.main;
            main.startSize = new ParticleSystem.MinMaxCurve(minParticleSize, maxParticleSize);

            explosionParticles.Play();
            Destroy(explosionInstance, main.duration);
        }

        // Instanciar efecto de polvo
        if (dustEffectPrefab != null)
        {
            GameObject dustInstance = Instantiate(dustEffectPrefab, impactPoint, Quaternion.LookRotation(impactNormal));
            ParticleSystem dustParticles = dustInstance.GetComponent<ParticleSystem>();
            dustParticles.Play();
            Destroy(dustInstance, dustParticles.main.duration);
        }

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, impactPoint);
        }

        // Ocultar el proyectil
        //GetComponent<MeshRenderer>().enabled = false;
        //GetComponent<Collider>().enabled = false;

        /* if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        } */

        // Destruir el proyectil después de un breve tiempo
        // Destroy(gameObject, 0.1f);
    }
}
