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
    [Range(10, 30)]
    public float posicionFijaZ = 20f;

    [Header("Configuración de Escala")]
    [Range(1, 3)]
    public float escala = 2f;

    private float ultimaPosicionGeneracionXNegativaZ = 0f; // Última posición X en la calle -z
    private float ultimaPosicionGeneracionXPositivaZ = 0f; // Última posición X en la calle +z
    private List<GameObject> calleNegativaZ = new List<GameObject>(); // Lista de objetos en -z
    private List<GameObject> callePositivaZ = new List<GameObject>(); // Lista de objetos en +z

    // Para controlar el último prefab usado en cada calle
    private int ultimoPrefabUsadoPositivaZ = -1;
    private int ultimoPrefabUsadoNegativaZ = -1;

    void Start()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogError("No se han asignado prefabs de edificios.");
            return;
        }

        GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ, 1, ref ultimoPrefabUsadoPositivaZ);
        GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ, -1, ref ultimoPrefabUsadoNegativaZ);
    }

    void Update()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

        // Calle +Z
        if (liegreCam.position.x > ultimaPosicionGeneracionXPositivaZ - umbralGeneracion)
        {
            GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ, 1, ref ultimoPrefabUsadoPositivaZ);
        }
        // Calle -Z
        if (liegreCam.position.x > ultimaPosicionGeneracionXNegativaZ - umbralGeneracion)
        {
            GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ, -1, ref ultimoPrefabUsadoNegativaZ);
        }

        DestruirObjetosAntiguos();
    }

    void GenerarObjetoEnCalle(ref float ultimaPosicionX, float posicionZ, List<GameObject> calle, int mirror, ref int ultimoPrefabUsado)
    {
        if (buildingPrefabs.Length == 0) return;

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
        float posX = ultimaPosicionX + escala * size.x;
        float posY = 0;
        Vector3 posicionObjeto = new Vector3(posX, posY, posicionZ + (mirror > 0 ? escala * size.z : -escala * size.z));

        // Instanciar el nuevo objeto
        GameObject nuevoObjeto = Instantiate(prefabSeleccionado, posicionObjeto, Quaternion.identity);
        Rigidbody rb = nuevoObjeto.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = nuevoObjeto.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        // Actualizar la última posición y añadir a la lista
        ultimaPosicionX = posX;
        nuevoObjeto.transform.localScale = new Vector3(escala, escala, escala * mirror);
        calle.Add(nuevoObjeto);
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