using TMPro;
using UnityEngine;

public class SunSimulator : MonoBehaviour
{
    [Header("Configuración")]
    [Range(0f, 23.99f)]
    public float currentTime = 0f;
    public float timeSpeed = 1f;

    [Range(0.1f, 5f)]
    public float rotationSmoothness = 1f;

    [Range(0f, 23.99f)]
    public float dawnTime = 7.5f;

    [Range(9f, 16f)]
    public float sunTime = 14f;

    [Header("Daylight Colors")]
    [ColorUsage(false, true)]
    public Color middayColor = Color.white;

    [ColorUsage(false, true)]
    public Color sunsetColor = new Color(1f, 0.8f, 0f);

    [ColorUsage(false, true)]
    public Color horizonColor = new Color(1f, 0.3f, 0f);

    [Header("Referencias")]
    public Light sunLight;
    public TextMeshProUGUI timeDisplay;

    private float currentRotationX;
    private float targetRotationX;

    private void Start()
    {
        if (sunLight != null)
        {
            // Inicializar con la rotación correcta basada en el tiempo actual
            UpdateTargetRotation();
            currentRotationX = targetRotationX;
            sunLight.transform.eulerAngles = new Vector3(currentRotationX, 0, 0);
            UpdateSunColor();
        }
    }

    private void Update()
    {
        UpdateTime();
        UpdateSunRotation();
        UpdateSunColor();
        UpdateTimeDisplay();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeSpeed / 60f;
        if (currentTime >= 24f)
            currentTime = 0f;
    }

    private void UpdateTargetRotation()
    {
        if (currentTime >= dawnTime && currentTime <= dawnTime + sunTime)
        {
            // Durante el día: 180° a 0° (pasando por 90° al mediodía)
            float dayProgress = (currentTime - dawnTime) / sunTime;
            targetRotationX = 180f - (dayProgress * 180f);
        }
        else if (currentTime > dawnTime + sunTime)
        {
            // Después del atardecer: de 0° a -90° (270°)
            float nightProgress = (currentTime - (dawnTime + sunTime)) / (24f - sunTime);
            targetRotationX = 0f - (nightProgress * 90f);
        }
        else if (currentTime < dawnTime)
        {
            // Antes del amanecer: de 270° a 180°
            float nightProgress = currentTime / dawnTime;
            targetRotationX = 270 - (90f * nightProgress);
        }

    }

    private void UpdateSunRotation()
    {
        if (sunLight == null)
            return;

        UpdateTargetRotation();

        // Manejar la transición suave entre ángulos cercanos a 0°/360°
        /*   float angleDifference = Mathf.DeltaAngle(currentRotationX, targetRotationX); */
        currentRotationX = targetRotationX; /* * Time.deltaTime * rotationSmoothness; */

        // Asegurarse de que la rotación esté en el rango -180° a 180°
        /*      if (currentRotationX > 180f)
                 currentRotationX -= 360f;

             if (currentRotationX < -180f)
                 currentRotationX += 360f; */

        sunLight.transform.eulerAngles = new Vector3(currentRotationX, 0, 0);
    }

    private void UpdateSunColor()
    {
        if (sunLight == null)
            return;

        // Usar el ángulo actual para determinar el color
        float effectiveAngle = currentRotationX;
        if (effectiveAngle < 0)
            effectiveAngle += 360f;

        // Durante el día
        if (currentTime >= dawnTime && currentTime < dawnTime + sunTime)
        {
            // Amanecer (180°-165°) - Rojo a Amarillo
            if (effectiveAngle > 165f && effectiveAngle <= 180f)
            {
                float transitionProgress = (effectiveAngle - 165f) / 15f;
                sunLight.color = Color.Lerp(sunsetColor, horizonColor, transitionProgress);
            }
            // Mañana/Tarde (155°-115°) - Amarillo a Blanco
            else if (effectiveAngle > 115f && effectiveAngle <= 165f)
            {
                float transitionProgress = (effectiveAngle - 115f) / 50f;
                sunLight.color = Color.Lerp(middayColor, sunsetColor, transitionProgress);
            }
            // Mediodía (115°-65°) - Blanco puro
            else if (effectiveAngle > 65f && effectiveAngle <= 115f)
            {
                sunLight.color = middayColor;
            }
            // Tarde (65°-15°) - Blanco a Amarillo
            else if (effectiveAngle > 15f && effectiveAngle <= 65f)
            {
                float transitionProgress = (effectiveAngle - 15f) / 50f;
                sunLight.color = Color.Lerp(middayColor, sunsetColor, 1f - transitionProgress);
            }
            // Atardecer (15°-0°) - Amarillo a Rojo
            else if (effectiveAngle <= 15f)
            {
                float transitionProgress = effectiveAngle / 15f;
                sunLight.color = Color.Lerp(horizonColor, sunsetColor, transitionProgress);
            }
        }
        else // Noche
        {
            if (effectiveAngle > 180f && effectiveAngle < 195f)
            {
                float transitionProgress = (effectiveAngle - 180f) / 15f;
                sunLight.color = Color.Lerp( horizonColor, Color.black,transitionProgress);
            }
            else if (effectiveAngle < 0f && effectiveAngle > -15f)
            {
                float transitionProgress = -effectiveAngle / 15f;
                sunLight.color = Color.Lerp(horizonColor, Color.black, transitionProgress);
            }
            else
            {
                sunLight.color = Color.black;
            }
        }
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

        int hora_dec = hours / 10;
        int hora_uni = hours % 10;
        int min_dec = minutes / 10;
        int min_uni = minutes % 10;

        return $"<mspace=0.6em>{hora_dec}{hora_uni}:{min_dec}{min_uni}</mspace>";
    }
}
