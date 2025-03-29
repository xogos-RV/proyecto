using UnityEngine;
using System.Collections;

public class ProyectilController : MonoBehaviour
{
    [Header("Prefab Configuration")]
    public GameObject projectilePrefab;
    public GameObject aimIndicatorPrefab;
    private PlayerInput playerInput;
    private GameObject aimIndicator;

    [Header("Factor Compensación")]
    [Range(-1f, 1f)]
    public float compensationFactor = 0f;

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
    public float maxLaunchDistance = 100f;

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

    [Header("Aim Direction Control")]
    public float initialHorizontalAngle = 0f;  // Ángulo inicial horizontal (en grados)
    public float initialVerticalAngle = 0f;    // Ángulo inicial vertical (en grados)
    public float horizontalSensitivity = 2f;   // Sensibilidad del control horizontal
    public float verticalSensitivity = 2f;     // Sensibilidad del control vertical
    private float currentHorizontalAngle;      // Ángulo actual horizontal
    private float currentVerticalAngle;        // Ángulo actual vertical
    private Vector2 mouseLook;                 // Para almacenar el input del ratón
    public float maxHorizontalAngle = 60f;     // Ángulo máximo horizontal (positivo y negativo)
    public float maxVerticalAngle = 60f;       // Ángulo máximo vertical (positivo y negativo)
    public bool invertY = true;


    [Header("Indicator Colors")]
    public Color surfaceHitColor = Color.green;
    public Color noHitColor = Color.blue;
    public Color chargeStartColor = Color.green;
    public Color chargeEndColor = Color.red;

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

        if (aimIndicatorPrefab != null)
        {
            aimIndicator = Instantiate(aimIndicatorPrefab);
            aimIndicator.SetActive(false);
        }

        // Inicializar los ángulos de apuntado
        currentHorizontalAngle = initialHorizontalAngle;
        currentVerticalAngle = initialVerticalAngle;
    }


    void Update()
    {
        fire = playerInput.fire;
        mouseLook = playerInput.look;
        if (invertY == true)
        {
            mouseLook.y *= -1;
        }

        UpdateAimAngles(); // Actualizar el ángulo de apuntado basado en el scroll

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
            Color chargeColor = Color.Lerp(chargeStartColor, chargeEndColor, chargeProgress);
            aimIndicator.GetComponent<Renderer>().material.color = chargeColor;
        }
    }


    void UpdateAimAngles()
    {
        // Ajustar el ángulo horizontal basado en el movimiento horizontal del ratón
        currentHorizontalAngle += mouseLook.x * horizontalSensitivity * Time.deltaTime;

        // Ajustar el ángulo vertical basado en el movimiento vertical del ratón
        currentVerticalAngle -= mouseLook.y * verticalSensitivity * Time.deltaTime; // Invertido para que sea intuitivo

        // Limitar los ángulos dentro de los rangos permitidos
        currentHorizontalAngle = Mathf.Clamp(currentHorizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, -maxVerticalAngle, maxVerticalAngle);
    }


    void UpdateAimIndicator()
    {
        if (aimIndicator == null) return;

        // Calcular la dirección de apuntado basada en los ángulos actuales
        Vector3 aimDirection = CalculateAimDirection();

        // Lanzar un rayo desde la posición de disparo en la dirección calculada
        Vector3 spawnPosition = transform.position +
                              (-transform.right * leftOffset) +
                              (Vector3.up * heightOffset);

        Ray aimRay = new Ray(spawnPosition, aimDirection);
        RaycastHit hit;

        // Obtener el renderer del indicador para cambiar su color
        Renderer indicatorRenderer = aimIndicator.GetComponent<Renderer>();

        // Configurar una máscara de capas para ignorar la capa de proyectiles
        int layerMask = ~0; // Inicialmente incluye todas las capas

        // Buscar un impacto válido (que no sea un proyectil)
        bool validHit = false;

        // Intentamos encontrar un impacto válido
        if (Physics.Raycast(aimRay, out hit, maxLaunchDistance, layerMask))
        {
            // Verificar si el objeto impactado tiene la etiqueta "Proyectil"
            if (hit.collider.CompareTag("Proyectil"))
            {
                // Es un proyectil, intentamos encontrar otro objeto detrás
                RaycastHit[] hits = Physics.RaycastAll(aimRay, maxLaunchDistance, layerMask);

                // Ordenar los hits por distancia
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                // Buscar el primer hit que no sea un proyectil
                foreach (RaycastHit potentialHit in hits)
                {
                    if (!potentialHit.collider.CompareTag("Proyectil"))
                    {
                        hit = potentialHit;
                        validHit = true;
                        break;
                    }
                }
            }
            else
            {
                validHit = true;
            }
        }

        if (validHit)
        {
            // El rayo impactó con una superficie válida
            aimIndicator.SetActive(true);

            // Colocar el indicador exactamente en el punto de impacto
            aimIndicator.transform.position = hit.point;

            // Orientar el indicador según la normal de la superficie
            aimIndicator.transform.up = hit.normal;

            // Restaurar el color original o usar un color para superficies
            if (isCharging)
            {
                // Si está cargando, mantener el color según la carga
                float chargeProgress = (Time.time - chargeStartTime) / chargeTime;
                chargeProgress = Mathf.Clamp01(chargeProgress);
                indicatorRenderer.material.color = Color.Lerp(chargeStartColor, chargeEndColor, chargeProgress);
            }
            else
            {
                // Color por defecto para cuando hay impacto pero no está cargando
                indicatorRenderer.material.color = surfaceHitColor;
            }
        }
        else
        {
            // El rayo no impactó con ninguna superficie válida
            aimIndicator.SetActive(true);
            Vector3 targetPoint = aimRay.GetPoint(maxLaunchDistance);
            aimIndicator.transform.position = targetPoint;
            aimIndicator.transform.up = Vector3.up; // Orientación por defecto
            indicatorRenderer.material.color = noHitColor;
        }
    }


    Vector3 CalculateAimDirection()
    {
        // Obtener la dirección base (hacia adelante desde la perspectiva del jugador)
        Vector3 baseDirection = transform.forward;

        // Crear una rotación que combine ambos ángulos
        Quaternion horizontalRotation = Quaternion.Euler(0, currentHorizontalAngle, 0);
        Quaternion verticalRotation = Quaternion.Euler(currentVerticalAngle, 0, 0);
        Quaternion combinedRotation = horizontalRotation * verticalRotation;

        // Aplicar la rotación combinada a la dirección base
        Vector3 direction = combinedRotation * baseDirection;

        // Ya no forzamos Y=0 para permitir apuntar hacia arriba/abajo
        direction.Normalize();

        return direction;
    }


    IEnumerator CooldownRoutine()
    {
        canFire = false;
        yield return new WaitForSeconds(cooldownTime);
        canFire = true;
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

        // Usar la dirección calculada para el lanzamiento
        Vector3 launchDirection = CalculateAimDirection();

        // Añadir componente vertical para la trayectoria
        launchDirection += Vector3.up * (upwardForce / force);
        launchDirection.Normalize();

        Quaternion projectileRotation = Quaternion.LookRotation(launchDirection);
        projectileRotation *= Quaternion.Euler(180, 0, 0);

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, projectileRotation);
        projectile.transform.localScale *= projectileScale;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
            rb = projectile.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.useGravity = useGravity;
        rb.AddForce(launchDirection * force, ForceMode.Impulse);

        if (audioSource != null)
            audioSource.Play();

        Destroy(projectile, projectileLifetime);
    }



    void OnDestroy()
    {
        if (aimIndicator != null)
            Destroy(aimIndicator);
    }
}