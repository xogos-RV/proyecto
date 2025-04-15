using UnityEngine;
using TMPro;

public class SunSimulator : MonoBehaviour
{
    [Header("Configuración")]
    [Range(0f, 23.99f)] public float currentTime = 8f; // 0.00 a 23.59 en formato decimal
    public float timeSpeed = 1f; // Velocidad de simulación (1 = tiempo real)
    [Range(0.1f, 5f)] public float rotationSmoothness = 1f; // Suavizado de rotación
    [Range(0f, 23.99f)] public float dawnTime = 7.5f; // Hora del día cuando el sol está a X: 0 (amanecer)

    [Header("Referencias")]
    public Light sunLight; // Luz direccional del sol
    public TextMeshProUGUI timeDisplay; // Componente TextMeshPro para mostrar la hora
    
    private float targetRotationX;
    private Vector3 currentEulerAngles;

    private void Start()
    {
        if (sunLight != null)
        {
            currentEulerAngles = sunLight.transform.eulerAngles;
        }
    }

    private void Update()
    {
        UpdateTime();
        UpdateSunRotation();
        UpdateTimeDisplay();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeSpeed / 60f;
        if (currentTime >= 24f)
        {
            currentTime = 0f;
        }
    }

    private void UpdateSunRotation()
    {
        if (sunLight == null) return;
        
        // Calcular rotación basada en la hora del amanecer
        float normalizedTime = Mathf.Repeat(currentTime - dawnTime, 24f);
        targetRotationX = 360f - (normalizedTime / 24f * 360f);
        
        currentEulerAngles.x = Mathf.LerpAngle(
            currentEulerAngles.x, 
            targetRotationX, 
            Time.deltaTime * rotationSmoothness
        );
        
        sunLight.transform.eulerAngles = currentEulerAngles;
    }

    private void UpdateTimeDisplay()
    {
        if (timeDisplay != null)
        {
            timeDisplay.text = GetFormattedTime();
        }
    }

    private string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return $"{hours:00}:{minutes:00}";
    }
}