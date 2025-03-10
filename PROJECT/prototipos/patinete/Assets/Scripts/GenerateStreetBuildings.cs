using UnityEngine;
using System.Collections.Generic;

public class GenerateStreetBuildings : MonoBehaviour
{
    public Transform liegreCam; // Referencia al jugador (la bola)

    // Configuración para la generación de objetos
    [Header("Configuración de Objetos")]
    public GameObject[] buildingPrefabs; // Array de prefabs de edificios
    public float umbralGeneracion = 20f;

    [Header("Configuración de Posición Z")]
    public float posicionFijaZ = 20f;

    [Header("Configuración de Escala")]
    [Range(1, 3)]
    public float escala = 2f;

    [Header("Configuración de Espacios")]
    [Range(2, 5)]
    public float espacioMinimoEntrePrefabs = 2f;
    [Range(2, 10)]
    public float espacioMaximoEntrePrefabs = 5f;

    [Header("Probabilidad de Generación")]
    [Range(0.5f, 1f)]
    public float probabilidadGeneracion = 0.7f; // Probabilidad de generar un edificio

    private float ultimaPosicionGeneracionXNegativaZ = 0f; // Última posición X en la calle -z
    private float ultimaPosicionGeneracionXPositivaZ = 0f; // Última posición X en la calle +z
    private List<GameObject> calleNegativaZ = new List<GameObject>(); // Lista de objetos en -z
    private List<GameObject> callePositivaZ = new List<GameObject>(); // Lista de objetos en +z
    
    // Para controlar el último prefab usado en cada calle
    private int ultimoPrefabUsadoPositivaZ = -1;
    private int ultimoPrefabUsadoNegativaZ = -1;
    
    // Para controlar si el último espacio fue vacío
    private bool ultimoEspacioVacioPositivaZ = false;
    private bool ultimoEspacioVacioNegativaZ = false;

    void Start()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogError("No se han asignado prefabs de edificios.");
            return;
        }

        GenerarElementoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ, 1, ref ultimoPrefabUsadoPositivaZ, ref ultimoEspacioVacioPositivaZ);
        GenerarElementoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ, -1, ref ultimoPrefabUsadoNegativaZ, ref ultimoEspacioVacioNegativaZ);
    }

    void Update()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

        // Calle +Z
        if (liegreCam.position.x > ultimaPosicionGeneracionXPositivaZ - umbralGeneracion)
        {
            GenerarElementoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ, 1, ref ultimoPrefabUsadoPositivaZ, ref ultimoEspacioVacioPositivaZ);
        }
        // Calle -Z
        if (liegreCam.position.x > ultimaPosicionGeneracionXNegativaZ - umbralGeneracion)
        {
            GenerarElementoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ, -1, ref ultimoPrefabUsadoNegativaZ, ref ultimoEspacioVacioNegativaZ);
        }

        DestruirObjetosAntiguos();
    }

    void GenerarElementoEnCalle(ref float ultimaPosicionX, float posicionZ, List<GameObject> calle, int mirror, ref int ultimoPrefabUsado, ref bool ultimoEspacioVacio)
    {
        if (buildingPrefabs.Length == 0) return;

        // Añadir un espacio aleatorio entre edificios
        float espacioAleatorio = Random.Range(espacioMinimoEntrePrefabs, espacioMaximoEntrePrefabs);
        ultimaPosicionX += espacioAleatorio;

        // Decidir si generar un edificio o dejar un espacio vacío
        bool generarEdificio = true;
        
        // Si el último espacio fue vacío, forzar la generación de un edificio
        if (!ultimoEspacioVacio)
        {
            generarEdificio = Random.value <= probabilidadGeneracion;
        }

        if (generarEdificio)
        {
            // Seleccionar un prefab aleatorio diferente al último usado
            int indicePrefab;
            if (buildingPrefabs.Length > 1)
            {
                do
                {
                    indicePrefab = Random.Range(0, buildingPrefabs.Length);
                } while (indicePrefab == ultimoPrefabUsado);
            }
            else
            {
                indicePrefab = 0;
            }
            
            ultimoPrefabUsado = indicePrefab;
            GameObject prefabSeleccionado = buildingPrefabs[indicePrefab];
            
            // Obtener el tamaño del prefab seleccionado
            Vector3 size = getSize(prefabSeleccionado);
            Debug.Log("Prefab seleccionado: " + prefabSeleccionado.name + ", SIZE: " + size.ToString());

            // Calcular la posición para el nuevo objeto
            float posX = ultimaPosicionX;
            float posY = 0;
            Vector3 posicionObjeto = new Vector3(posX, posY, posicionZ + (mirror > 0 ? escala * size.z : -escala * size.z));
            
            // Instanciar el nuevo objeto RESPETANDO LA ROTACIÓN ORIGINAL DEL PREFAB
            GameObject nuevoObjeto = Instantiate(prefabSeleccionado, posicionObjeto, prefabSeleccionado.transform.rotation);
            
            Rigidbody rb = nuevoObjeto.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = nuevoObjeto.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            
            // Actualizar la última posición y añadir a la lista
            ultimaPosicionX = posX + escala * size.x;
            
            // Aplicar escala pero mantener la rotación original
            Vector3 escalaOriginal = prefabSeleccionado.transform.localScale;
            nuevoObjeto.transform.localScale = new Vector3(
                escalaOriginal.x * escala, 
                escalaOriginal.y * escala, 
                escalaOriginal.z * escala * mirror
            );
            
            calle.Add(nuevoObjeto);
            
            ultimoEspacioVacio = false;
        }
        else
        {
            // Si no generamos un edificio, solo actualizamos la posición
            ultimaPosicionX += 5f; // Un espacio vacío estándar
            ultimoEspacioVacio = true;
            Debug.Log("Espacio vacío generado en posición X: " + ultimaPosicionX);
        }
    }

    void DestruirObjetosAntiguos()
    {
        float distanciaDestruccion = umbralGeneracion * 2;
        
        for (int i = calleNegativaZ.Count - 1; i >= 0; i--)
        {
            if (calleNegativaZ[i].transform.position.x < liegreCam.position.x - distanciaDestruccion)
            {
                Destroy(calleNegativaZ[i]);
                calleNegativaZ.RemoveAt(i);
            }
        }
        
        for (int i = callePositivaZ.Count - 1; i >= 0; i--)
        {
            if (callePositivaZ[i].transform.position.x < liegreCam.position.x - distanciaDestruccion)
            {
                Destroy(callePositivaZ[i]);
                callePositivaZ.RemoveAt(i);
            }
        }
    }

    Vector3 getSize(GameObject obj)
    {
        Vector3 tamañoMasGrande = Vector3.zero;
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 tamaño = renderer.bounds.size;
            Debug.Log("Nombre del objeto: " + obj.name + ", Tamaño: " + tamaño);
            if (tamaño.magnitude > tamañoMasGrande.magnitude)
            {
                tamañoMasGrande = tamaño;
            }
        }
        
        foreach (Transform hijo in obj.transform)
        {
            Vector3 tamañoHijo = getSize(hijo.gameObject);
            if (tamañoHijo.magnitude > tamañoMasGrande.magnitude)
            {
                tamañoMasGrande = tamañoHijo;
            }
        }
        
        return tamañoMasGrande;
    }
}