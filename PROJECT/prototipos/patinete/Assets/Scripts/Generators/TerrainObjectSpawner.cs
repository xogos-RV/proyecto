using UnityEngine;
using System.Collections;

public class TerrainObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab to spawn on the terrain")]
    public GameObject prefabToSpawn;
    
    [Tooltip("Number of instances to spawn")]
    [Range(1, 10000)]
    public int spawnCount = 100;
    
    [Header("Depth Settings")]
    [Tooltip("Minimum depth below terrain surface")]
    [Range(0.1f, 10f)]
    public float minDepth = 0.5f;
    
    [Tooltip("Maximum depth below terrain surface")]
    [Range(0.1f, 10f)]
    public float maxDepth = 2f;
    
    [Header("Terrain Settings")]
    [Tooltip("Delay before spawning to ensure terrain is ready")]
    [Range(0.1f, 5f)]
    public float spawnDelay = 1f;
    
    [Tooltip("Minimum distance from terrain edges")]
    [Range(1f, 50f)]
    public float edgePadding = 5f;

    private void Start()
    {
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        
        Terrain terrain = Terrain.activeTerrain;
        
        if (terrain == null)
        {
            Debug.LogError("No terrain found in the scene!");
            yield break;
        }
        
        if (prefabToSpawn == null)
        {
            Debug.LogError("No prefab assigned to spawn!");
            yield break;
        }
        
        if (minDepth > maxDepth)
        {
            Debug.LogError("minDepth cannot be greater than maxDepth!");
            yield break;
        }
        
        SpawnObjects(terrain);
    }

    private void SpawnObjects(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;
        Vector3 terrainPos = terrain.transform.position;
        
        for (int i = 0; i < spawnCount; i++)
        {
            // Calcular posición aleatoria dentro de los límites del terreno
            float randomX = Random.Range(edgePadding, terrainSize.x - edgePadding);
            float randomZ = Random.Range(edgePadding, terrainSize.z - edgePadding);
            
            // Obtener altura del terreno en esta posición (corregido)
            Vector3 samplePos = new Vector3(
                randomX + terrainPos.x,
                0,
                randomZ + terrainPos.z);
            
            float surfaceHeight = terrain.SampleHeight(samplePos) + terrainPos.y;
            
            // Calcular profundidad aleatoria
            float randomDepth = Random.Range(minDepth, maxDepth);
            
            // Crear vector de posición (bajo la superficie del terreno)
            Vector3 spawnPos = new Vector3(
                samplePos.x,
                surfaceHeight - randomDepth,
                samplePos.z);
            
            // Instanciar el prefab
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }
    }
}