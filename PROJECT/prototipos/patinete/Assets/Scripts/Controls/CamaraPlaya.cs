using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [Tooltip("Objeto que la cámara seguirá (normalmente el personaje)")]
    Transform target;

    [Header("Factores de Seguimiento")]
    [Tooltip("Factor de movimiento en el eje X (0 = estático, 1 = sigue completamente)")]
    [Range(0f, 1f)]
    public float followFactorX = 0.5f;

    [Tooltip("Factor de movimiento en el eje Z (0 = estático, 1 = sigue completamente)")]
    [Range(0f, 1f)]
    public float followFactorZ = 0.3f;

    private Vector3 velocityXZ = Vector3.zero;
    [SerializeField] private float smoothTime = 0.3f;

    [Header("Opciones")]
    [Tooltip("Mantener rotación inicial constantemente")]
    public bool maintainInitialRotation = true;

    [Header("Posición relativa")]
    [Tooltip("Distancia horizontal desde el objetivo")]
    [Range(5f, 150f)]
    public float initialDistance = 5f;
    [Tooltip("Ángulo horizontal alrededor del objetivo (en grados)")]
    [Range(0f, 360f)]
    public float initialAngle = 45f;
    [Tooltip("Altura de la cámara respecto al objetivo")]
    [Range(2f, 50f)]
    public float initialHeight = 2f;
    [Tooltip("Ángulo vertical (picado) de la cámara")]
    [Range(0f, 60f)]
    public float initialPitch = 15f;

    // Variables para detectar cambios en tiempo de ejecución
    private float lastDistance;
    private float lastAngle;
    private float lastHeight;
    private float lastPitch;
    private Vector3 calculatedPosition;
    private Quaternion calculatedRotation;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("No hay objetivo asignado para la cámara.");
        }

        // Guardar valores iniciales para detectar cambios
        lastDistance = initialDistance;
        lastAngle = initialAngle;
        lastHeight = initialHeight;
        lastPitch = initialPitch;

        // Calcular posición y rotación inicial
        CalculateCameraPositionAndRotation();
        transform.position = calculatedPosition;
        transform.rotation = calculatedRotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Verificar si los parámetros de posición han cambiado
        if (HasCameraPositionChanged() || true)
        {
            CalculateCameraPositionAndRotation();
        }

        // Calcular nueva posición con factores independientes para X y Z
        float targetX = calculatedPosition.x;
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref velocityXZ.x, smoothTime);

        float targetZ = calculatedPosition.z;
        float newZ = Mathf.SmoothDamp(transform.position.z, targetZ, ref velocityXZ.z, smoothTime);

        // Mantener la altura calculada en Y
        Vector3 newPosition = new Vector3(
            newX,
            calculatedPosition.y,
            newZ
        );

        // Aplicar la nueva posición
        transform.position = newPosition;

        // Mantener rotación calculada si está activado
        if (maintainInitialRotation)
        {
            transform.rotation = calculatedRotation;
        }
    }

    // Calcula la posición y rotación de la cámara basada en los parámetros actuales
    private void CalculateCameraPositionAndRotation()
    {
        // Convertir ángulo a radianes
        float angleRad = initialAngle * Mathf.Deg2Rad;

        // Calcular posición relativa
        float xOffset = initialDistance * Mathf.Sin(angleRad);
        float zOffset = initialDistance * Mathf.Cos(angleRad);

        calculatedPosition = target.position + new Vector3(xOffset, initialHeight, zOffset);

        // Aplicar pitch adicional
        calculatedRotation = Quaternion.Euler(initialPitch, initialAngle - 180, 0f); // TODO -180 corrige: el mapa esta al reves....

        // Actualizar valores guardados
        lastDistance = initialDistance;
        lastAngle = initialAngle;
        lastHeight = initialHeight;
        lastPitch = initialPitch;
    }


    // Verifica si los parámetros de posición han cambiado
    private bool HasCameraPositionChanged()
    {
        return !Mathf.Approximately(lastDistance, initialDistance) ||
               !Mathf.Approximately(lastAngle, initialAngle) ||
               !Mathf.Approximately(lastHeight, initialHeight) ||
               !Mathf.Approximately(lastPitch, initialPitch);
    }

}