using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Stats")]
    public int totalAmeixas = 0;
    public int collectedAmeixas = 0;

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