using UnityEngine;

public class OscillateRotationY : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [Tooltip("Ángulo total de oscilación en grados (eje Y)")]
    [Range(30, 120)] public float rotationRange = 90f;

    [Tooltip("Tiempo que tarda en completar un ciclo completo (segundos)")]
    [Range(2, 10)] public float oscillationPeriod = 3f;

    private float oscillationTimer = 0f;

    void Start()
    {
        // Forzar a que mire hacia +Z al editar
        transform.rotation = Quaternion.identity;
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

        // Aplicar la rotación con el punto medio mirando hacia +Z
        transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

}