using UnityEngine;

public class SandDigger : MonoBehaviour
{
    public Terrain terrain;
    public float digRadius = 1f;
    public float digDepth = 0.2f;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Al hacer clic izquierdo
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == terrain.gameObject)
            {
                DigHole(hit.point);
            }
        }
    }
    
    void DigHole(Vector3 worldPos)
    {
        // Convertir posición mundial a coordenadas del terreno
        Vector3 terrainPos = worldPos - terrain.transform.position;
        Vector3 normalizedPos = new Vector3(
            terrainPos.x / terrain.terrainData.size.x,
            0,
            terrainPos.z / terrain.terrainData.size.z);
        
        // Obtener datos de altura
        int x = (int)(normalizedPos.x * terrain.terrainData.heightmapResolution);
        int z = (int)(normalizedPos.z * terrain.terrainData.heightmapResolution);
        
        // Radio en píxeles del heightmap
        int radius = (int)(digRadius * terrain.terrainData.heightmapResolution / terrain.terrainData.size.x);
        
        // Obtener y modificar heights
        float[,] heights = terrain.terrainData.GetHeights(x - radius, z - radius, radius * 2, radius * 2);
        
        for (int i = 0; i < radius * 2; i++)
        {
            for (int j = 0; j < radius * 2; j++)
            {
                float distance = Vector2.Distance(new Vector2(i, j), new Vector2(radius, radius));
                if (distance <= radius)
                {
                    // Reducir altura (suavizado con función de distancia)
                    float reduction = digDepth * (1 - distance / radius);
                    heights[i, j] = Mathf.Max(0, heights[i, j] - reduction / terrain.terrainData.size.y);
                }
            }
        }
        
        // Aplicar cambios
        terrain.terrainData.SetHeights(x - radius, z - radius, heights);
    }
}