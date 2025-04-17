using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [Range(0f, 23.99f)]
    public float currentTime = 0f;
    public float timeSpeed = 1f;

    [Header("UI")]
    public TextMeshProUGUI timeDisplay;

    private void Update()
    {
        UpdateTime();
        UpdateTimeDisplay();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeSpeed / 60f;
        if (currentTime >= 24f)
            currentTime = 0f;
    }

    private void UpdateTimeDisplay()
    {
        if (timeDisplay != null)
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);

            int hora_dec = hours / 10;
            int hora_uni = hours % 10;
            int min_dec = minutes / 10;
            int min_uni = minutes % 10;

            timeDisplay.text = $"<mspace=0.6em>{hora_dec}{hora_uni}:{min_dec}{min_uni}</mspace>";
        }
    }

}