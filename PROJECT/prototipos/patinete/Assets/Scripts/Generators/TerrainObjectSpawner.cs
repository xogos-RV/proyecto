using UnityEngine;
using System.Collections;

public class TerrainObjectSpawner : MonoBehaviour
{
    [Header("Texture Filter")]
    [Tooltip("Name of the terrain layer where objects should spawn")]
    public string targetTextureName = "Sand_TerrainLayer";
    [Range(0.1f, 1f)] public float textureThreshold = 0.5f;

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
    private Terrain terrain;
    private TerrainData terrainData;
    private int targetTextureIndex = -1;

    private void Start()
    {
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);

        terrain = Terrain.activeTerrain;
        if (terrain == null || prefabToSpawn == null || minDepth > maxDepth)
        {
            Debug.LogError("Initialization error!");
            yield break;
        }

        terrainData = terrain.terrainData;
        FindTargetTextureIndex();

        SpawnObjects();
    }

    private void FindTargetTextureIndex()
    {
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (terrainData.terrainLayers[i].name == targetTextureName)
            {
                targetTextureIndex = i;
                break;
            }
        }

        if (targetTextureIndex == -1)
        {
            Debug.LogError($"Texture layer '{targetTextureName}' not found!");
        }
    }

    private void SpawnObjects()
    {
        Vector3 terrainSize = terrainData.size;
        Vector3 terrainPos = terrain.transform.position;
        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = spawnCount * 10; // Límite para evitar bucles infinitos

        while (spawnedCount < spawnCount && attempts < maxAttempts)
        {
            attempts++;

            float randomX = Random.Range(edgePadding, terrainSize.x - edgePadding);
            float randomZ = Random.Range(edgePadding, terrainSize.z - edgePadding);

            Vector3 samplePos = new Vector3(
                randomX + terrainPos.x,
                0,
                randomZ + terrainPos.z);

            float surfaceHeight = terrain.SampleHeight(samplePos) + terrainPos.y;

            // Verificar si está en la textura correcta
            if (IsPositionOnTargetTexture(samplePos))
            {
                float randomDepth = Random.Range(minDepth, maxDepth);
                Vector3 spawnPos = new Vector3(
                    samplePos.x,
                    surfaceHeight - randomDepth,
                    samplePos.z);

                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                spawnedCount++;
            }
        }

        if (spawnedCount < spawnCount)
        {
            Debug.LogWarning($"Only spawned {spawnedCount} objects (target: {spawnCount}). Not enough valid terrain area.");
        }
    }

    private bool IsPositionOnTargetTexture(Vector3 worldPos)
    {
        if (targetTextureIndex == -1) return true; // Si no se encontró la textura, spawnear igual

        // Convertir posición mundial a coordenadas de textura del terreno
        Vector3 terrainLocalPos = worldPos - terrain.transform.position;
        Vector2 normalizedPos = new Vector2(
            terrainLocalPos.x / terrainData.size.x,
            terrainLocalPos.z / terrainData.size.z);

        // Obtener mezcla de texturas en este punto
        float[,,] alphamap = terrainData.GetAlphamaps(
            (int)(normalizedPos.x * terrainData.alphamapWidth),
            (int)(normalizedPos.y * terrainData.alphamapHeight),
            1, 1);

        // Verificar si la textura objetivo tiene suficiente presencia
        return alphamap[0, 0, targetTextureIndex] >= textureThreshold;
    }
}