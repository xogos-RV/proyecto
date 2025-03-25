using UnityEngine;
using System.Collections;

public class ProyectilController : MonoBehaviour
{
    [Header("Prefab Configuration")]
    public GameObject projectilePrefab;
    public GameObject aimIndicatorPrefab;
    private PlayerInput playerInput;
    private GameObject aimIndicator;

    [Header("Spawn Position")]
    [Range(0f, 2f)]
    public float leftOffset = 1f;
    public float heightOffset = 3f;

    [Header("Launch Parameters")]
    [Range(1f, 100f)]
    public float minLaunchForce = 10f; // Fuerza mínima al disparar rápido
    public float maxLaunchForce = 50f; // Fuerza máxima al cargar
    public float chargeTime = 2f; // Tiempo en segundos para alcanzar fuerza máxima
    public float upwardForce = 0.6f;
    public float maxLaunchDistance = 50f;

    [Header("Cooldown")]
    public float cooldownTime = 0.2f;
    private bool canFire = true;
    private bool isCharging = false;
    private float chargeStartTime;
    private float currentLaunchForce;

    [Header("Projectile Settings")]
    public float projectileLifetime = 10f;
    public bool useGravity = true;
    public float projectileScale = 1f;

    [Header("Effects")]
    public ParticleSystem launchEffect;
    public AudioClip launchSound;
    public ParticleSystem chargeEffect; // Efecto mientras se carga
    private AudioSource audioSource;
    private float fire;

    private Camera mainCamera;

    void Start()
    {
        if (projectilePrefab == null)
            Debug.LogError("No se ha asignado un prefab de proyectil.");

        if (launchSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = launchSound;
        }

        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;

        if (aimIndicatorPrefab != null)
        {
            aimIndicator = Instantiate(aimIndicatorPrefab);
            aimIndicator.SetActive(false);
        }
    }

    void Update()
    {
        fire = playerInput.fire;

        // Actualizar puntería
        UpdateAimIndicator();

        // Iniciar carga si se presiona el botón y puede disparar
        if (fire > 0 && canFire && !isCharging)
        {
            StartCharging();
        }

        // Si está cargando, actualizar fuerza
        if (isCharging)
        {
            ContinueCharging();
        }

        // Si se suelta el botón y estaba cargando, disparar
        if (fire <= 0 && isCharging)
        {
            FireProjectile();
        }
    }

    void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        currentLaunchForce = minLaunchForce;

        // Activar efecto de carga (opcional)
        if (chargeEffect != null)
            chargeEffect.Play();
    }

    void ContinueCharging()
    {
        // Calcular progreso de carga (0 a 1)
        float chargeProgress = (Time.time - chargeStartTime) / chargeTime;
        chargeProgress = Mathf.Clamp01(chargeProgress); // Asegurar que no pase de 1

        // Ajustar fuerza según carga
        currentLaunchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, chargeProgress);

        // (Opcional) Cambiar color del indicador según carga)
        if (aimIndicator != null)
        {
            Color chargeColor = Color.Lerp(Color.green, Color.red, chargeProgress);
            aimIndicator.GetComponent<Renderer>().material.color = chargeColor;
        }
    }

    void FireProjectile()
    {
        isCharging = false;

        // Desactivar efecto de carga
        if (chargeEffect != null)
            chargeEffect.Stop();

        // Lanzar proyectil con la fuerza calculada
        LaunchProjectile(currentLaunchForce);

        // Iniciar cooldown
        StartCoroutine(CooldownRoutine());
    }

    void LaunchProjectile(float force)
    {
        Vector3 spawnPosition = transform.position +
                              (-transform.right * leftOffset) +
                              (Vector3.up * heightOffset);

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPosition;

        if (Physics.Raycast(ray, out hit, maxLaunchDistance))
            targetPosition = hit.point;
        else
            targetPosition = ray.GetPoint(maxLaunchDistance);

        Vector3 cameraOffsetCompensation = mainCamera.transform.right * (leftOffset * 0.5f);
        Vector3 launchDirection = (targetPosition - spawnPosition + cameraOffsetCompensation).normalized;
        launchDirection += Vector3.up * (upwardForce / force);
        launchDirection.Normalize();

        Quaternion projectileRotation = Quaternion.LookRotation(launchDirection);
        projectileRotation *= Quaternion.Euler(180, 0, 0);

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, projectileRotation);
        projectile.transform.localScale *= projectileScale;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
            rb = projectile.AddComponent<Rigidbody>();

        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(launchDirection * force, ForceMode.Impulse);

        // Efectos
        if (launchEffect != null)
            Instantiate(launchEffect, spawnPosition, Quaternion.identity);

        if (audioSource != null)
            audioSource.Play();

        Destroy(projectile, projectileLifetime);
    }

    void UpdateAimIndicator()
    {
        if (aimIndicator == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxLaunchDistance))
        {
            aimIndicator.SetActive(true);
            aimIndicator.transform.position = hit.point;
        }
        else
        {
            aimIndicator.SetActive(true);
            aimIndicator.transform.position = ray.GetPoint(maxLaunchDistance);
        }
    }

    IEnumerator CooldownRoutine()
    {
        canFire = false;
        yield return new WaitForSeconds(cooldownTime);
        canFire = true;
    }

    void OnDestroy()
    {
        if (aimIndicator != null)
            Destroy(aimIndicator);
    }
}