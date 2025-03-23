using UnityEngine;
using System.Collections;

public class ProyectilController : MonoBehaviour
{
    [Header("Prefab Configuration")]
    public GameObject projectilePrefab; // El prefab que se lanzará
    private PlayerInput playerInput;

    [Header("Spawn Position")]
    [Range(0f, 2f)]
    public float leftOffset = 1f; // Distancia a la izquierda del jugador (entre 0 y 1)
    public float heightOffset = 3f; // Altura desde el centro del jugador

    [Header("Launch Parameters")]
    [Range(1f, 100f)]
    public float launchForce = 1f; // Fuerza de lanzamiento
    public float upwardForce = 0.6f; // Fuerza hacia arriba para crear la parábola
    public float forwardTilt = 1f; // Componente hacia adelante (+Z) (0-1)

    [Header("Cooldown")]
    public float cooldownTime = 0.2f; // Tiempo de espera entre lanzamientos
    private bool canFire = true;

    [Header("Projectile Settings")]
    public float projectileLifetime = 10f; // Tiempo de vida del proyectil en segundos
    public bool useGravity = true; // Si el proyectil debe usar gravedad
    public float projectileScale = 1f; // Escala del proyectil

    [Header("Effects")]
    public ParticleSystem launchEffect; // Efecto opcional al lanzar
    public AudioClip launchSound; // Sonido opcional al lanzar
    private AudioSource audioSource;
    private float fire;

    void Start()
    {
        // Verificar si hay un prefab asignado
        if (projectilePrefab == null)
        {
            Debug.LogError("No se ha asignado un prefab de proyectil.");
        }

        // Configurar el audio source si hay un sonido de lanzamiento
        if (launchSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = launchSound;
        }
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {

        fire = playerInput.fire;
        if (fire > 0 && canFire && projectilePrefab != null)
        {
            LaunchProjectile();
            StartCoroutine(CooldownRoutine());
        }
    }

void LaunchProjectile()
{
    // Calcular la posición de spawn (ligeramente a la izquierda del jugador)
    Vector3 spawnPosition = transform.position +
                           (-transform.right * leftOffset) + // A la izquierda del jugador
                           (Vector3.up * heightOffset);      // Ajuste de altura

    // Calcular la dirección de lanzamiento (hacia la izquierda)
    Vector3 launchDirection = -transform.right; // Dirección -X (izquierda relativa al jugador)
    launchDirection += Vector3.up * (upwardForce / launchForce); // Componente hacia arriba
    launchDirection += transform.forward * forwardTilt; // Componente hacia adelante (+Z)
    launchDirection.Normalize();

    // Calcular la rotación del proyectil para que mire en la dirección de lanzamiento
    Quaternion projectileRotation = Quaternion.LookRotation(launchDirection);

    // Invertir el eje X de la rotación
    projectileRotation *= Quaternion.Euler(180, 0, 0);

    // Crear el proyectil en el punto calculado con la rotación adecuada
    GameObject projectile = Instantiate(projectilePrefab, spawnPosition, projectileRotation);

    // Aplicar escala
    projectile.transform.localScale *= projectileScale;

    // Obtener o añadir un Rigidbody
    Rigidbody rb = projectile.GetComponent<Rigidbody>();
    if (rb == null)
    {
        rb = projectile.AddComponent<Rigidbody>();
    }

    // Configurar el Rigidbody
    rb.useGravity = useGravity;
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    // Aplicar la fuerza
    rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);

    // Reproducir efectos
    if (launchEffect != null)
    {
        Instantiate(launchEffect, spawnPosition, Quaternion.identity);
    }

    if (audioSource != null)
    {
        audioSource.Play();
    }

    // Destruir el proyectil después de un tiempo
    Destroy(projectile, projectileLifetime);
}
    IEnumerator CooldownRoutine()
    {
        canFire = false;
        yield return new WaitForSeconds(cooldownTime);
        canFire = true;
    }
}