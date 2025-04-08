using UnityEngine;

public class AdvancedCameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [Tooltip("Objeto que la cámara seguirá (normalmente el personaje)")]
    public Transform target;

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

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialOffset;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }
        else
        {
            Debug.LogWarning("No hay objetivo asignado para la cámara.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calcular nueva posición con factores independientes para X y Z
        float targetX = target.position.x + initialOffset.x;
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref velocityXZ.x, smoothTime);

        float targetZ = target.position.z + initialOffset.z;
        float newZ = Mathf.SmoothDamp(transform.position.z, targetZ, ref velocityXZ.z, smoothTime);

        // Mantener la posición inicial en Y (o podrías añadir otro factor si lo necesitas)
        Vector3 newPosition = new Vector3(
            newX,
            initialPosition.y,
            newZ
        );

        // Aplicar la nueva posición
        transform.position = newPosition;

        // Mantener rotación inicial si está activado
        if (maintainInitialRotation)
        {
            transform.rotation = initialRotation;
        }
    }

    //TODO Método para reiniciar la cámara
    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }
    }

    //TODO Método para actualizar manualmente el offset (útil si cambias la posición inicial en tiempo de ejecución)
    public void UpdateInitialOffset()
    {
        if (target != null)
        {
            initialOffset = transform.position - target.position;
        }
    }
}