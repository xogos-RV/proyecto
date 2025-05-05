using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Stats")]
    public int totalAmeixas = 0;
    public int collectedAmeixas = 0;

    public TMP_Text statsText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        statsText.text = collectedAmeixas.ToString();
    }

    public void RegisterAmeixa()
    {
        totalAmeixas++;
    }

    public void CollectAmeixa()
    {
        collectedAmeixas++;
        Debug.Log($"Ameixas recolectadas: {collectedAmeixas}/{totalAmeixas}");

        // Opcional: Verificar si se recolectaron todas
        if (collectedAmeixas >= totalAmeixas)
        {
            Debug.Log("¡Todas las Ameixas recolectadas!");
        }
    }

    public void DestroyAmeixa()
    {
        collectedAmeixas--;
        Debug.Log($"Ameixa destruida: {collectedAmeixas}/{totalAmeixas}");
    }
}