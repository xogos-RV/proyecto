using UnityEngine;


public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject terrainPrefab;
    public GameObject playerPrefab;

    [Header("Player Spawn Settings")]
    public float minSpawnDistanceFromEdges = 100f;
    public float heightOffset = 1f;

    private GameObject terrainInstance;
    private Terrain terrainComponent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HidePreObject();
        SpawnTerrain();
        SpawnPlayer();
    }

    private void HidePreObject()
    {
        GameObject preObject = GameObject.Find("ParaInstanciar");
        if (preObject != null)
        {
            preObject.SetActive(false);
        }
        else
        {
            Debug.LogError("No se encontró un objeto llamado 'Pre' en la escena.");
        }
    }

    private void SpawnTerrain()
    {
        if (terrainPrefab == null)
        {
            Debug.LogError("No se ha asignado el prefab del terreno en el inspector.");
            return;
        }

        terrainInstance = Instantiate(terrainPrefab, Vector3.zero, Quaternion.identity);
        terrainComponent = terrainInstance.GetComponent<Terrain>();

        if (terrainComponent == null)
        {
            Debug.LogError("El prefab del terreno no tiene un componente Terrain.");
        }

        TerrainData clonedTerrainData = Instantiate(terrainComponent.terrainData);
        terrainComponent.terrainData = clonedTerrainData;

        TerrainCollider terrainCollider = terrainInstance.GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.terrainData = clonedTerrainData;
            return;
        }

        Debug.LogWarning("No se encontró un componente TerrainCollider en el prefab del terreno.");

    }

    private void SpawnPlayer()
    {
        SpawnPlayerAtRandomPosition();
    }

    public GameObject SpawnPlayerAtRandomPosition()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("No se ha asignado el prefab del jugador en el inspector.");
            return null;
        }

        if (terrainComponent == null)
        {
            Debug.LogError("No hay terreno para spawnear al jugador.");
            return null;
        }

        // Obtener las dimensiones del terreno
        TerrainData terrainData = terrainComponent.terrainData;
        Vector3 terrainSize = terrainData.size;
        Vector3 terrainPosition = terrainInstance.transform.position;

        // Calcular posición aleatoria dentro del terreno (evitando los bordes)
        float randomX = Random.Range(minSpawnDistanceFromEdges, terrainSize.x - minSpawnDistanceFromEdges);
        float randomZ = Random.Range(minSpawnDistanceFromEdges, terrainSize.z - minSpawnDistanceFromEdges);

        // Obtener la altura del terreno en esa posición
        float terrainHeight = terrainComponent.SampleHeight(new Vector3(randomX, 0, randomZ)) + terrainPosition.y;

        // Crear la posición final del jugador
        Vector3 spawnPosition = new Vector3(
            randomX,
            terrainHeight + heightOffset,
            randomZ
        );

        // Instanciar al jugador y devolverlo
        return Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}