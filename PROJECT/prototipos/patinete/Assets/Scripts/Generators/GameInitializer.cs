using UnityEngine;


public class GameInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Player Spawn Settings")]
    [SerializeField] private float minSpawnDistanceFromEdges = 10f;
    [SerializeField] private float heightOffset = 1f; // Para asegurar que el jugador no se spawnée dentro del terreno

    private GameObject terrainInstance;
    private Terrain terrainComponent;

    void Start()
    {
        // Instanciar el terreno en (0,0,0)
        SpawnTerrain();

        // Instanciar al jugador en una posición aleatoria sobre el terreno
        SpawnPlayer();
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
        
        TerrainData clonedTerrainData = Instantiate(terrainComponent.terrainData);
        terrainComponent.terrainData = clonedTerrainData;

        if (terrainComponent == null)
        {
            Debug.LogError("El prefab del terreno no tiene un componente Terrain.");
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("No se ha asignado el prefab del jugador en el inspector.");
            return;
        }

        if (terrainComponent == null)
        {
            Debug.LogError("No hay terreno para spawnear al jugador.");
            return;
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

        // Instanciar al jugador
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}