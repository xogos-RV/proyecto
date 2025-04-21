using UnityEngine;
using System.Collections;

public class GeneradorAlrededorJugador : MonoBehaviour
{
    [Header("Configuración del Jugador")]
    [Tooltip("Tag del objeto jugador")]
    public string playerTag = "Player";

    [Header("Configuración de Generación")]
    [Tooltip("Prefab a generar")]
    public GameObject prefabToSpawn;

    [Tooltip("Intervalo entre generaciones (segundos)")]
    [Range(0.1f, 60f)]
    public float spawnInterval = 5f;

    [Tooltip("Radio mínimo desde el jugador")]
    [Range(1f, 50f)]
    public float minRadius = 5f;

    [Tooltip("Radio máximo desde el jugador")]
    [Range(1f, 100f)]
    public float maxRadius = 15f;

    [Header("Configuración de Terreno")]
    [Tooltip("Nombre de la capa de terreno objetivo")]
    public string targetTextureName = "Sand_TerrainLayer"; // "AmeixasLayer";

    [Tooltip("Umbral de textura para generación")]
    [Range(0.1f, 1f)]
    public float textureThreshold = 0.5f;

    [Tooltip("Profundidad bajo la superficie")]
    [Range(0.01f, 1f)]
    public float spawnDepth = 0.1f;

    private const string NameObject = "AmeixasColection";
    private const string GeneratePath = "Generate";
    private Terrain terrain;
    private TerrainData terrainData;
    private int targetTextureIndex = -1;
    private GameObject player;
    private Coroutine spawningCoroutine;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError($"No se encontró objeto con tag '{playerTag}'");
            return;
        }

        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No se encontró terreno activo!");
            return;
        }

        terrainData = terrain.terrainData;
        FindTargetTextureIndex();

        spawningCoroutine = StartCoroutine(SpawnObjectsContinuously());
    }

    private void OnDisable()
    {
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
        }
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
            Debug.LogError($"Capa de terreno '{targetTextureName}' no encontrada!");
        }
    }

    private IEnumerator SpawnObjectsContinuously()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (player == null || prefabToSpawn == null) continue;

            TrySpawnObjectAroundPlayer();
        }
    }

    private void TrySpawnObjectAroundPlayer()
    {
        Vector3 playerPos = player.transform.position;
        int attempts = 0;
        int maxAttempts = 50; // Para evitar bucles infinitos

        while (attempts < maxAttempts)
        {
            attempts++;

            // Calcular posición aleatoria en círculo alrededor del jugador
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
            Vector3 spawnPos = new Vector3(
                playerPos.x + randomCircle.x,
                0,
                playerPos.z + randomCircle.y);

            // Verificar si está en la textura correcta
            if (IsPositionOnTargetTexture(spawnPos))
            {
                GameObject generateParent = GameObject.Find(GeneratePath);
                if (generateParent == null)
                {
                    generateParent = new GameObject(GeneratePath);
                }

                GameObject parentObject = GameObject.Find(NameObject);
                if (parentObject == null)
                {
                    parentObject = new GameObject(NameObject);
                    parentObject.transform.parent = generateParent.transform;
                }

                float surfaceHeight = terrain.SampleHeight(spawnPos) + terrain.transform.position.y;
                Vector3 finalPos = new Vector3(
                    spawnPos.x,
                    surfaceHeight - spawnDepth,
                    spawnPos.z);

                GameObject newObject = Instantiate(prefabToSpawn, finalPos, Quaternion.identity);
                newObject.transform.parent = parentObject.transform;
                break; // Objeto generado, salir del bucle
            }
        }
    }

    private bool IsPositionOnTargetTexture(Vector3 worldPos)
    {
        if (targetTextureIndex == -1) return true; //TODO no deberiamos! Si no se encontró la textura, generamos igual

        Vector3 terrainLocalPos = worldPos - terrain.transform.position;
        Vector2 normalizedPos = new Vector2(
            terrainLocalPos.x / terrainData.size.x,
            terrainLocalPos.z / terrainData.size.z);

        float[,,] alphamap = terrainData.GetAlphamaps(
            (int)(normalizedPos.x * terrainData.alphamapWidth),
            (int)(normalizedPos.y * terrainData.alphamapHeight),
            1, 1);

        return alphamap[0, 0, targetTextureIndex] >= textureThreshold;
    }
}