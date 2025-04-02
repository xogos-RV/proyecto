using UnityEngine;

public class ProyectilController : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    public GameObject explosionParticlesPrefab;
    public GameObject dustEffectPrefab;
    public AudioClip impactSound;

    [Range(10, 500)] public int minParticles = 25;
    [Range(10, 500)] public int maxParticles = 200;
    [Range(0.1f, 10f)] public float minParticleSize = 0.5f;
    [Range(0.1f, 10f)] public float maxParticleSize = 5f;

    [Header("Configuración de Penetración")]
    [Tooltip("Qué porcentaje de la velocidad se convierte en penetración")]
    [Range(0f, 1f)] public float penetrationFactor = 0.3f;
    [Tooltip("Profundidad mínima de penetración en metros")]
    public float minPenetrationDepth = 0.05f;
    [Tooltip("Profundidad máxima de penetración en metros")]
    public float maxPenetrationDepth = 0.5f;
    [Tooltip("Offset adicional para ajustar la posición clavada")]
    public float positionOffset = 0.02f;

    private bool hasCollided = false;
    private Rigidbody rb;
    private Collider projectileCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        if (!ShouldIgnoreCollision(collision.gameObject))
        {
            hasCollided = true;
            HandleImpact(collision);
        }
    }

    private bool ShouldIgnoreCollision(GameObject otherObject)
    {
        return otherObject.CompareTag("Player") ||
                otherObject.CompareTag("Floor") ||
                otherObject.CompareTag("Proyectil");
    }

    private void HandleImpact(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 impactPoint = contact.point;
        Vector3 impactNormal = contact.normal;
        GameObject hitObject = collision.gameObject;

        // Calcular dirección de impacto (dirección de la velocidad)
        Vector3 impactDirection = rb.linearVelocity.normalized;

        // Calcular profundidad de penetración basada en la fuerza
        float penetrationDepth = CalculatePenetrationDepth(rb.linearVelocity.magnitude);

        // Desactivar física y collider antes de mover
        DisablePhysics();

        // Posicionar y rotar la flecha
        PositionArrow(impactPoint, impactDirection, penetrationDepth);

        // Hacer que la flecha sea hija del objeto impactado
        AttachToHitObject(hitObject);

        // Efectos de impacto
        SpawnImpactEffects(impactPoint, impactNormal);

        // Opcional: Desactivar script para mejorar rendimiento
        enabled = false;
    }

    private float CalculatePenetrationDepth(float impactSpeed)
    {
        float calculatedDepth = impactSpeed * penetrationFactor;
        return Mathf.Clamp(calculatedDepth, minPenetrationDepth, maxPenetrationDepth);
    }

    private void DisablePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
           // rb.detectCollisions = false;
        }

        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }
    }

    private void PositionArrow(Vector3 impactPoint, Vector3 impactDirection, float penetrationDepth)
    {
        // Mover la flecha hacia dentro del objeto
        transform.position = impactPoint + impactDirection * (penetrationDepth - positionOffset);

        // Rotar la flecha para que apunte en la dirección del impacto
        if (impactDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(impactDirection);
        }
    }

    private void AttachToHitObject(GameObject hitObject)
    {
        // Crear un GameObject intermedio para evitar problemas de escala
        GameObject attachmentParent = new GameObject("ProjectileAttachment");
        attachmentParent.transform.position = transform.position;
        attachmentParent.transform.rotation = transform.rotation;
        attachmentParent.transform.SetParent(hitObject.transform, true);

        // Hacer el proyectil hijo del objeto intermedio
        transform.SetParent(attachmentParent.transform, true);
    }

    private void SpawnImpactEffects(Vector3 impactPoint, Vector3 impactNormal)
    {
        // Efecto de explosión
        if (explosionParticlesPrefab != null)
        {
            GameObject explosionInstance = Instantiate(
                explosionParticlesPrefab,
                impactPoint,
                Quaternion.LookRotation(impactNormal)
            );

            ConfigureParticleSystem(explosionInstance);
        }

        // Efecto de polvo
        if (dustEffectPrefab != null)
        {
            GameObject dustInstance = Instantiate(
                dustEffectPrefab,
                impactPoint,
                Quaternion.LookRotation(impactNormal)
            );

            ConfigureParticleSystem(dustInstance);
        }

        // Sonido de impacto
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, impactPoint);
        }
    }

    private void ConfigureParticleSystem(GameObject particleInstance)
    {
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(minParticleSize, maxParticleSize);

            var emission = ps.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0, Random.Range(minParticles, maxParticles)));

            ps.Play();
            Destroy(particleInstance, main.duration);
        }
    }
}