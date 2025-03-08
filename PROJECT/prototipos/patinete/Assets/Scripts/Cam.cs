using UnityEngine;

public class CameraFollowX : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Configuración de Posición")]
    [Tooltip("Offset en X respecto al jugador")]
    public float xOffset = -8f;
    [Tooltip("Altura de la cámara respecto al suelo")]
    public float cameraHeight = 12f;
    [Tooltip("Distancia de la cámara al jugador en Z")]
    public float zDistance = 6f;

    [Header("Configuración de LookAt")]
    [Tooltip("Offset adicional en X para el punto de mira")]
    public float lookAtXOffset = 15f;
    [Tooltip("Multiplicador de altura para el punto de mira")]
    [Range(0f, 3f)]
    public float lookAtHeightMultiplier = 0.33f;
    [Tooltip("Suavizado del movimiento (0 = sin suavizado)")]
    [Range(0f, 1f)]
    public float smoothSpeed =1f;

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
        UpdateCameraPosition(true);
        
        // Calculamos el offset inicial para el LookAt
        initialLookAtOffset = CalculateLookAtOffset();
    }

    void LateUpdate()
    {
        if (player == null) return;
        
        UpdateCameraPosition(false);
    }
    
    private void UpdateCameraPosition(bool immediate)
    {
        // Calculamos la nueva posición de la cámara (solo en el eje X)
        Vector3 targetPosition = new Vector3(
            player.position.x + xOffset, // Seguimos al jugador en el eje X con offset
            cameraHeight,                // Altura configurable
            player.position.z - zDistance // Distancia configurable en Z
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
            player.position.x + lookAtXOffset,
            player.position.y + (initialLookAtOffset.y * lookAtHeightMultiplier),
            player.position.z
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
    public void UpdateCameraSettings(float newXOffset, float newHeight, float newZDistance, 
                                    float newLookAtXOffset, float newHeightMultiplier)
    {
        xOffset = newXOffset;
        cameraHeight = newHeight;
        zDistance = newZDistance;
        lookAtXOffset = newLookAtXOffset;
        lookAtHeightMultiplier = newHeightMultiplier;
        
        // Recalculamos el offset del LookAt
        initialLookAtOffset = CalculateLookAtOffset();
    }
}