using UnityEngine;

public class TideSimulator : MonoBehaviour
{
    [Header("Configuración de Mareas")]
    [Tooltip("Objeto que representa el agua")]
    public Transform waterTransform;

    [Tooltip("Altura mínima de la marea")]
    [Range(-6f, -2f)]
    public float minTideHeight = -5f;

    [Tooltip("Altura máxima de la marea")]
    [Range(-2f, 2f)]
    public float maxTideHeight = 0f;

    [Tooltip("Duración del ciclo de marea en horas")]
    public float tideCycleDuration = 12.42f; // 12 horas y 25 minutos

    private TimeManager timeManager;

    private void Awake()
    {
        timeManager = GetComponent<TimeManager>();

        if (waterTransform == null)
        {
            Debug.LogError("No se ha asignado el objeto Water en el inspector", this);
        }
    }

    private void Update()
    {
        if (waterTransform == null || timeManager == null) return;
        // Calcular el progreso del ciclo de marea (0 a 1) considerando la hora actual absoluta
        float tideProgress = timeManager.currentTime / tideCycleDuration % 1f;

        // Calcular la posición Y usando una función senoidal (oscila entre 0 y 1)
        float normalizedHeight = (Mathf.Sin(tideProgress * 2f * Mathf.PI - Mathf.PI / 2f) + 1f) / 2f;

        // Mapear a nuestro rango absoluto de alturas
        float currentHeight = Mathf.Lerp(minTideHeight, maxTideHeight, normalizedHeight);

        // Aplicar la nueva posición al agua (manteniendo X y Z originales)
        waterTransform.position = new Vector3(
            waterTransform.position.x,
            currentHeight,
            waterTransform.position.z
        );
    }

    // Método para validar que maxTideHeight siempre sea mayor que minTideHeight
    private void OnValidate()
    {
        if (maxTideHeight < minTideHeight)
        {
            maxTideHeight = minTideHeight;
        }
    }
}