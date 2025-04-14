using UnityEngine;

public class OscillateRotationY : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [Tooltip("Ángulo total de oscilación en grados (eje Y)")]
    [Range(30, 120)] public float rotationRange = 90f;
    
    [Tooltip("Tiempo que tarda en completar un ciclo completo (segundos)")]
    [Range(2, 10)] public float oscillationPeriod = 3f;

    private float oscillationTimer = 0f;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Actualizar el temporizador
        oscillationTimer += Time.deltaTime;
        
        // Calcular el progreso normalizado del ciclo (0 a 1)
        float cycleProgress = Mathf.PingPong(oscillationTimer, oscillationPeriod) / oscillationPeriod;
        
        // Convertir a un valor entre -1 y 1 para el movimiento de ida y vuelta
        float oscillationFactor = Mathf.Lerp(-1f, 1f, cycleProgress);
        
        // Calcular la rotación actual (-rotationRange/2 a +rotationRange/2)
        float currentRotation = oscillationFactor * (rotationRange / 2f);
        
        // Aplicar la rotación manteniendo las otras rotaciones originales
        transform.rotation = initialRotation * Quaternion.Euler(0f, currentRotation, 0f);
    }

    // Resetear la rotación inicial si se modifican los parámetros en el editor
    void OnValidate()
    {
        initialRotation = transform.rotation;
    }
}