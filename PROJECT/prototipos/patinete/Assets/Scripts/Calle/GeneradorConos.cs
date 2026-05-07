using UnityEngine;
using System.Collections.Generic;

public class GenerateWaveObjects : MonoBehaviour
{
    public Transform playerCamera; // Referencia a la cámara o jugador
    public ObjectPool objectPool; // Referencia al GenericObjectPool

    [Header("Prefabs")]
    public GameObject[] objectPrefabs; // Array de prefabs a generar

    [Header("Wave Configuration")]
    public float waveAmplitude = 3f; // Amplitud de la onda (entre -3 y 3 en X)
    public float waveFrequency = 0.5f; // Frecuencia de la onda
    public float randomFactor = 0.5f; // Factor de aleatoriedad (0 = sin aleatoriedad, 1 = muy aleatorio)

    [Header("Generation Settings")]
    public float objectSpacing = 1f; // Espacio entre objetos en Z
    public float generationThreshold = 20f; // Distancia de generación adelante del jugador
    public float destructionDistance = 20f; // Distancia de destrucción detrás del jugador

    [Header("Object Scale")]
    [Range(0.5f, 3f)]
    public float objectScale = 1f; // Escala de los objetos

    private float lastGeneratedZ = 0f; // Última posición Z donde se generó un objeto
    private List<GameObject> generatedObjects = new List<GameObject>(); // Lista de objetos generados
    private int lastPrefabIndex = -1; // Índice del último prefab usado


    void Start()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogError("No se han asignado prefabs de objetos.");
            return;
        }

        // Inicializar los pools para cada prefab
        foreach (GameObject prefab in objectPrefabs)
        {
            objectPool.CreatePool(prefab, 10); // Crear un pool de 10 objetos para cada prefab
        }

        // Generar objetos iniciales
        for (int i = 0; i < 20; i++) // Generar algunos objetos iniciales
        {
            GenerateNextObject();
        }
    }

    void Update()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0) return;

        // Generar nuevos objetos cuando el jugador se acerca al límite
        while (playerCamera.position.z > lastGeneratedZ - generationThreshold)
        {
            GenerateNextObject();
        }

        // Destruir objetos antiguos
        DestroyOldObjects();
    }

    void GenerateNextObject()
    {
        // Calcular la siguiente posición Z
        lastGeneratedZ += objectSpacing;

        // Calcular la posición X usando una función sinusoidal con componente aleatoria
        float baseX = waveAmplitude * Mathf.Sin(waveFrequency * lastGeneratedZ);
        float randomOffset = Random.Range(-randomFactor, randomFactor) * waveAmplitude;
        float posX = baseX + randomOffset;

        // Asegurarse de que no exceda los límites
        posX = Mathf.Clamp(posX, -waveAmplitude, waveAmplitude);

        // Seleccionar un prefab aleatorio diferente al último usado
        int prefabIndex;
        if (objectPrefabs.Length > 1)
        {
            do
            {
                prefabIndex = Random.Range(0, objectPrefabs.Length);
            } while (prefabIndex == lastPrefabIndex);
        }
        else
        {
            prefabIndex = 0;
        }
        lastPrefabIndex = prefabIndex;

        // Obtener el objeto del pool
        GameObject newObject = objectPool.GetObject(objectPrefabs[prefabIndex]);

        // Configurar la posición y rotación del objeto
        Vector3 position = new Vector3(posX, 0, lastGeneratedZ);
        newObject.transform.position = position;
        newObject.transform.rotation = objectPrefabs[prefabIndex].transform.rotation;

        // Aplicar escala
        Vector3 originalScale = objectPrefabs[prefabIndex].transform.localScale;
        newObject.transform.localScale = originalScale * objectScale;

        // Añadir rigidbody si es necesario
        Rigidbody rb = newObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = newObject.AddComponent<Rigidbody>();
        }

        // Añadir a la lista de objetos generados
        generatedObjects.Add(newObject);
    }

    void DestroyOldObjects()
    {
        for (int i = generatedObjects.Count - 1; i >= 0; i--)
        {
            if (generatedObjects[i] != null &&
                generatedObjects[i].transform.position.z < playerCamera.position.z - destructionDistance)
            {
                // Devolver el objeto al pool en lugar de destruirlo
                objectPool.ReturnObject(generatedObjects[i]);
                generatedObjects.RemoveAt(i);
            }
        }
    }
}