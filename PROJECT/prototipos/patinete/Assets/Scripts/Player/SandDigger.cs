using UnityEngine;

public class SandDigger : MonoBehaviour
{
    public GameObject holePrefab; // TODO Prefab opcional para hoyo visual
    [Header("Configuración de Escarbado")]
    [Range(1, 5)] public int digRadius = 1;       // Radio del hoyo
    [Range(0.01f, 0.2f)] public float digDepth = 0.05f;  // Profundidad del hoyo 
    [Range(0.01f, 1f)] public float digRate = 0.2f;       // Cadencia  
    [Range(0, 5)] public float digOffset = 1.0f; // Distancia hacia adelante donde se crea el hoyo

    private float lastDigTime;
    private Terrain terrain;
    private PlayerInput PI;

    void Start()
    {
        PI = GetComponent<PlayerInput>();
        terrain = Terrain.activeTerrain;
        lastDigTime = -digRate;
    }

    void Update()
    {
        if (PI.escarbando)
        {
            Vector3 digPosition = transform.position + transform.forward * digOffset;
            DigHoleAtPlayerPosition(digPosition);
        }

        if (Input.GetMouseButton(0)) // Botón izquierdo
        {
            DigWithMouse();
        }
    }

    private void DigWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int layerMask = ~0; // Inicialmente incluye todas las capas
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            DigHoleAtPlayerPosition(hit.point); // Reusa el mismo método
        }
    }

    public void DigHoleAtPlayerPosition(Vector3 position)
    {
        if (Time.time - lastDigTime < digRate) return;

        if (terrain != null)
        {
            lastDigTime = Time.time;
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
        }

        // Instanciar prefab de hoyo visual (opcional)
        if (holePrefab != null)
        {
            Instantiate(holePrefab, position, Quaternion.identity);
        }
    }
}