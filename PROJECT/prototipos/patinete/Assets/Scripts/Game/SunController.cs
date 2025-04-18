using UnityEngine;

public class SunSimulator : MonoBehaviour
{
    [Header("Configuración")]
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

    [ColorUsage(false, true)]
    public Color raioVerde = new Color(0f, 1f, 0f);

    [Header("Referencias")]
    public Light sunLight;

    private TimeManager TimeManager;
    private float currentRotationX;
    private float targetRotationX;

    private void Start()
    {
        TimeManager = GetComponent<TimeManager>();
        if (sunLight != null)
        {
            UpdateTargetRotation();
            currentRotationX = targetRotationX;
            sunLight.transform.eulerAngles = new Vector3(currentRotationX, 0, 0);
            UpdateSunColor();
        }
    }

    private void Update()
    {
        UpdateSunRotation();
        UpdateSunColor();
    }

    private void UpdateTargetRotation()
    {
        float sunset = dawnTime + sunTime;
        float nightTime = 24f - sunTime;
        float currentTime = TimeManager.currentTime;
        if (currentTime >= dawnTime && currentTime <= sunset)
        {
            // Durante el día: 180° a 0° (pasando por 90° al mediodía)
            float dayProgress = (currentTime - dawnTime) / sunTime;
            targetRotationX = 180f - (dayProgress * 180f);
        }
        else
        {
            if (currentTime > sunset)
            {
                // Después del atardecer: de 0° a -180° (360°)
                float nightProgress = (currentTime - sunset) / nightTime;
                targetRotationX = 0f - (180f * nightProgress);
            }
            else if (currentTime < dawnTime)
            {
                // Antes del amanecer: de 360° a 180°
                float nightProgress = (24f - sunset + currentTime) / nightTime;
                targetRotationX = 360 - (180f * nightProgress);
            }
        }

    }

    private void UpdateSunRotation()
    {
        if (sunLight == null) return;

        UpdateTargetRotation();
        currentRotationX = targetRotationX;
        sunLight.transform.eulerAngles = new Vector3(currentRotationX, 0, 0);
    }

    private void UpdateSunColor()
    {
        if (sunLight == null) return;

        float effectiveAngle = currentRotationX;
        if (effectiveAngle < 0)
            effectiveAngle += 360f;

        float currentTime = TimeManager.currentTime;

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

            // TODO raioVerde
        }
        else // Noche
        {
            if (effectiveAngle > 180f && effectiveAngle < 195f)
            {
                float transitionProgress = (effectiveAngle - 180f) / 15f;
                sunLight.color = Color.Lerp(horizonColor, Color.black, transitionProgress);
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

}