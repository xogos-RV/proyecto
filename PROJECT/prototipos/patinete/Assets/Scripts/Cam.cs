using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Transform liebre;

    [Header("Configuración de Posición")]
    [Tooltip("Offset en Z respecto al jugador")]
    public float zOffset = -8f;
    [Tooltip("Altura de la cámara respecto al suelo")]
    public float cameraHeight = 12f;
    [Tooltip("Distancia de la cámara al jugador en X")]
    public float xDistance = 6f;

    [Header("Configuración de LookAt")]
    [Tooltip("Offset adicional en Z para el punto de mira")]
    public float lookAtZOffset = 15f;
    [Tooltip("Multiplicador de altura para el punto de mira")]
    [Range(0f, 3f)]
    public float lookAtHeightMultiplier = 0.33f;
    [Tooltip("Suavizado del movimiento (0 = sin suavizado)")]
    [Range(0f, 1f)]
    public float smoothSpeed = 1f;

    [Header("Configuración de Seguimiento")]
    [Tooltip("bloquear seguir al jugador en el eje X")]
    public bool followLiebre = false;

    private Vector3 initialLookAtOffset; // Offset inicial para el LookAt

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("¡Error! No se ha asignado un jugador a la cámara.");
            enabled = false;
            return;
        }

        // Posicionamos la cámara según los parámetros configurados
        Transform target = followLiebre ? liebre : player;
        UpdateCameraPosition(true, target);

        // Calculamos el offset inicial para el LookAt
        initialLookAtOffset = CalculateLookAtOffset();
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (liebre != null)
        {
            Vector3 nuevaPosicion = liebre.position;
            nuevaPosicion.z = player.position.z;
            liebre.position = nuevaPosicion;
        }

        Transform target = followLiebre ? liebre : player;

        UpdateCameraPosition(false, target);
    }

    private void UpdateCameraPosition(bool immediate, Transform target)
    {
        // Calculamos la nueva posición de la cámara (solo en el eje Z)
        Vector3 targetPosition = new Vector3(
            target.position.x - xDistance, // Distancia configurable en X
            cameraHeight,                // Altura configurable
            target.position.z + zOffset  // Seguimos al objetivo en el eje Z con offset
        );

        // Aplicamos la nueva posición a la cámara (con o sin suavizado)
        if (immediate || smoothSpeed <= 0)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        }

        // Calculamos el punto de LookAt
        Vector3 lookAtPosition = new Vector3(
            target.position.x,
            target.position.y + (initialLookAtOffset.y * lookAtHeightMultiplier),
            target.position.z + lookAtZOffset
        );

        // Hacemos que la cámara mire hacia el punto calculado
        transform.LookAt(lookAtPosition);
    }

    private Vector3 CalculateLookAtOffset()
    {
        // Calculamos el offset inicial para el LookAt
        return player.position - transform.position;
    }

    // Método para actualizar la configuración en tiempo de ejecución
    public void UpdateCameraSettings(float newZOffset, float newHeight, float newXDistance,
                                    float newLookAtZOffset, float newHeightMultiplier)
    {
        zOffset = newZOffset;
        cameraHeight = newHeight;
        xDistance = newXDistance;
        lookAtZOffset = newLookAtZOffset;
        lookAtHeightMultiplier = newHeightMultiplier;

        // Recalculamos el offset del LookAt
        initialLookAtOffset = CalculateLookAtOffset();
    }
}