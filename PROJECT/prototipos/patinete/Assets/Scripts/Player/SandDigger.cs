using UnityEngine;

public class SandDigger : MonoBehaviour
{
    public GameObject holePrefab; // Prefab opcional para hoyo visual
    public GameObject digParticlesPrefab;  // Sistema de partículas para el efecto de arena

    private const string ParentFolderName = "GeneratePath";
    private const string HolesFolderName = "SandHoles";
    private const string ParticlesFolderName = "DigParticles";


    [Header("Configuración de Escarbado")]
    [Range(1, 5)] public int digRadius = 1;       // Radio del hoyo
    [Range(0.01f, 0.2f)] public float digDepth = 0.05f;  // Profundidad del hoyo 
    [Range(0.01f, 1f)] public float digRate = 0.2f;       // Cadencia  
    [Range(0, 5)] public float digOffset = 1.0f; // Distancia hacia adelante donde se crea el hoyo

    [Header("Configuración de Partículas")]
    [Range(0.1f, 2f)] public float minParticleSize = 0.3f;
    [Range(0.1f, 2f)] public float maxParticleSize = 0.6f;
    [Range(10, 100)] public int minParticles = 20;
    [Range(10, 100)] public int maxParticles = 40;
    [Range(-180, 180)] public float particleYRotation = 180f; // Rotación en eje Y
    [Range(0, 2)] public float backwardOffset = 0.5f;
    [Range(0, 2)] public float yOffset = 0.5f;

    [Header("Configuración de Textura")]
    public string targetTextureName = "Sand_TerrainLayer"; // Nombre de la textura donde se puede excavar

    private float lastDigTime;
    private Terrain terrain;
    private PlayerInput PI;
    private TerrainData terrainData;
    private int alphamapWidth;
    private int alphamapHeight;
    private float[,,] splatmapData;
    private int textureIndex = -1;

    private GameObject holesParent;
    private GameObject particlesParent;

    void Start()
    {
        InitializeHierarchyFolders();
        PI = GetComponent<PlayerInput>();
        terrain = Terrain.activeTerrain;
        terrainData = terrain.terrainData;
        alphamapWidth = terrainData.alphamapWidth;
        alphamapHeight = terrainData.alphamapHeight;
        splatmapData = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
        lastDigTime = -digRate;

        // Encontrar el índice de la textura objetivo
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (terrainData.terrainLayers[i].name == targetTextureName)
            {
                textureIndex = i;
                break;
            }
        }

        if (textureIndex == -1)
        {
            Debug.LogWarning($"No se encontró la textura con nombre: {targetTextureName}");
        }
    }

    void Update()
    {
        if (PI.escarbando)
        {
            Vector3 digPosition = transform.position + transform.forward * digOffset;
            DigHoleAtPlayerPosition(digPosition);
        }

        if (Input.GetMouseButton(0))
        {
            DigWithMouse();
        }
    }

    private void InitializeHierarchyFolders()
    {
        // Crear o encontrar el padre principal "Generate"
        GameObject generateParent = GameObject.Find(ParentFolderName) ?? new GameObject(ParentFolderName);

        // Crear carpeta para hoyos
        holesParent = GameObject.Find(HolesFolderName);
        if (holesParent == null)
        {
            holesParent = new GameObject(HolesFolderName);
            holesParent.transform.parent = generateParent.transform;
        }

        // Crear carpeta para partículas
        particlesParent = GameObject.Find(ParticlesFolderName);
        if (particlesParent == null)
        {
            particlesParent = new GameObject(ParticlesFolderName);
            particlesParent.transform.parent = generateParent.transform;
        }
    }
    private void DigWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int layerMask = ~0; // Inicialmente incluye todas las capas
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            DigHoleAtPlayerPosition(hit.point);
        }
    }

    public void DigHoleAtPlayerPosition(Vector3 position)
    {
        if (Time.time - lastDigTime < digRate) return;
        if (terrain == null || textureIndex == -1) return;

        // Verificar si estamos sobre la textura correcta
        if (!IsPositionOnTargetTexture(position))
        {
            return;
        }

        lastDigTime = Time.time;

        // Efecto de partículas
        SpawnDigParticles(position);

        // Convertir posición mundial a coordenadas del terreno
        Vector3 terrainPos = position - terrain.transform.position;
        Vector3 normalizedPos = new Vector3(terrainPos.x / terrain.terrainData.size.x, 0, terrainPos.z / terrain.terrainData.size.z);
        // Obtener datos de altura
        int x = (int)(normalizedPos.x * terrain.terrainData.heightmapResolution);
        int z = (int)(normalizedPos.z * terrain.terrainData.heightmapResolution);

        // Obtener y modificar heights
        float[,] heights = terrain.terrainData.GetHeights(x - digRadius, z - digRadius, digRadius * 2, digRadius * 2);

        for (int i = 0; i < digRadius * 2; i++)
        {
            for (int j = 0; j < digRadius * 2; j++)
            {
                float distance = Vector2.Distance(new Vector2(i, j), new Vector2(digRadius, digRadius));
                if (distance <= digRadius)
                {
                    // Reducir altura (suavizado con función de distancia)
                    float reduction = digDepth * (1 - distance / digRadius);
                    heights[i, j] = Mathf.Max(0, heights[i, j] - reduction / terrain.terrainData.size.y);
                }
            }
        }

        terrain.terrainData.SetHeights(x - digRadius, z - digRadius, heights);

        // Instanciar prefab de hoyo visual
        if (holePrefab != null)
        {
            GameObject hole = Instantiate(holePrefab, position, Quaternion.identity);
            hole.transform.parent = holesParent.transform;
        }
    }

    private bool IsPositionOnTargetTexture(Vector3 position)
    {
        // Convertir posición mundial a coordenadas del alphamap
        Vector3 terrainPos = position - terrain.transform.position;
        Vector2 normalizedPos = new Vector2(
            terrainPos.x / terrain.terrainData.size.x,
            terrainPos.z / terrain.terrainData.size.z
        );

        int x = (int)(normalizedPos.x * alphamapWidth);
        int y = (int)(normalizedPos.y * alphamapHeight);

        // Asegurarse de que las coordenadas estén dentro de los límites
        x = Mathf.Clamp(x, 0, alphamapWidth - 1);
        y = Mathf.Clamp(y, 0, alphamapHeight - 1);

        // Obtener el valor de mezcla para la textura objetivo
        float textureStrength = splatmapData[y, x, textureIndex];

        // Considerar que está en la textura si su fuerza es mayor que 0.5 (ajustable)
        return textureStrength > 0.5f;
    }

    private void SpawnDigParticles(Vector3 position)
    {
        if (digParticlesPrefab != null)
        {
            float terrainHeightAtDigPoint = terrain.SampleHeight(position);
            Vector3 spawnPosition = position + (-transform.forward * backwardOffset);
            spawnPosition.y = terrainHeightAtDigPoint + yOffset;

            // Crear rotación basada en la rotación del jugador con ajuste en Y
            Quaternion yRotation = Quaternion.Euler(0, particleYRotation, 0);
            Quaternion finalRotation = transform.rotation * yRotation;

            GameObject particlesInstance = Instantiate(
                    digParticlesPrefab,
                    spawnPosition,
                    finalRotation
                );

            particlesInstance.transform.parent = particlesParent.transform;
            ConfigureParticleSystem(particlesInstance);
        }
    }

    private void ConfigureParticleSystem(GameObject particleInstance)
    {
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(minParticleSize, maxParticleSize);

            var emission = ps.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0, Random.Range(minParticles, maxParticles)));

            ps.Play();
            Destroy(particleInstance, main.duration + 1f);
        }
    }
}