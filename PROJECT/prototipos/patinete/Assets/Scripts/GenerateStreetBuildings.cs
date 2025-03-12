using UnityEngine;
using System.Collections.Generic;

public class GenerateStreetBuildings : MonoBehaviour
{
    public Transform liegreCam; // Referencia al jugador (la bola)

    // Configuración para la generación de objetos
    [Header("Configuración de Objetos")]
    public GameObject[] buildingPrefabs; // Array de prefabs de edificios

    [Header("Configuración de Árboles y Papeleras")]
    public GameObject[] arboles_papeleras; // Nuevo array para árboles y papeleras
    public float distanciaUniformeArbolesPapeleras = 10f; // Distancia uniforme entre árboles/papeleras

    [Header("Configuración General")]
    public float umbralGeneracion = 20f;

    [Header("Configuración de Posición Z")]
    public float posicionFijaZ = 20f;
    public float posicionPasilloZ = 15f; // Posición Z para los pasillos de árboles/papeleras

    [Header("Configuración de Escala")]
    [Range(1, 3)]
    public float escala = 2f;
    [Range(0.5f, 2f)]
    public float escalaArbolesPapeleras = 1.5f; // Escala específica para árboles/papeleras

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

    // Para árboles y papeleras
    private float ultimaPosicionArbolPapeleraPositivaZ = 0f;
    private float ultimaPosicionArbolPapeleraNegativaZ = 0f;

    private List<GameObject> calleNegativaZ = new List<GameObject>(); // Lista de objetos en -z
    private List<GameObject> callePositivaZ = new List<GameObject>(); // Lista de objetos en +z
    private List<GameObject> arbolesPapelerasPositivaZ = new List<GameObject>(); // Lista de árboles/papeleras en +z
    private List<GameObject> arbolesPapelerasNegativaZ = new List<GameObject>(); // Lista de árboles/papeleras en -z

    // Para controlar el último prefab usado en cada calle
    private int ultimoPrefabUsadoPositivaZ = -1;
    private int ultimoPrefabUsadoNegativaZ = -1;
    private int ultimoArbolPapeleraUsadoPositivaZ = -1;
    private int ultimoArbolPapeleraUsadoNegativaZ = -1;

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

        if (arboles_papeleras == null || arboles_papeleras.Length == 0)
        {
            Debug.LogWarning("No se han asignado prefabs de árboles/papeleras.");
        }

        GenerarElementoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ, 1, ref ultimoPrefabUsadoPositivaZ, ref ultimoEspacioVacioPositivaZ);
        GenerarElementoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ, -1, ref ultimoPrefabUsadoNegativaZ, ref ultimoEspacioVacioNegativaZ);

        // Inicializar la generación de árboles y papeleras
        if (arboles_papeleras != null && arboles_papeleras.Length > 0)
        {
            GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraPositivaZ, posicionPasilloZ, arbolesPapelerasPositivaZ, -1, ref ultimoArbolPapeleraUsadoPositivaZ);
            GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraNegativaZ, -posicionPasilloZ, arbolesPapelerasNegativaZ, 1, ref ultimoArbolPapeleraUsadoNegativaZ);
        }
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

        // Árboles y papeleras
        if (arboles_papeleras != null && arboles_papeleras.Length > 0)
        {
            // Pasillo +Z (lado positivo)
            if (liegreCam.position.x > ultimaPosicionArbolPapeleraPositivaZ - umbralGeneracion)
            {
                GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraPositivaZ, posicionPasilloZ, arbolesPapelerasPositivaZ, 1, ref ultimoArbolPapeleraUsadoPositivaZ);
            }
            // Pasillo -Z (lado negativo)
            if (liegreCam.position.x > ultimaPosicionArbolPapeleraNegativaZ - umbralGeneracion)
            {
                GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraNegativaZ, -posicionPasilloZ, arbolesPapelerasNegativaZ, 1, ref ultimoArbolPapeleraUsadoNegativaZ);
            }
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

    void GenerarArbolPapelera(ref float ultimaPosicionX, float posicionZ, List<GameObject> lista, int mirror, ref int ultimoPrefabUsado)
    {
        if (arboles_papeleras.Length == 0) return;

        // Avanzar la posición X según la distancia uniforme configurada
        ultimaPosicionX += distanciaUniformeArbolesPapeleras;

        // Seleccionar un prefab aleatorio diferente al último usado
        int indicePrefab;
        if (arboles_papeleras.Length > 1)
        {
            do
            {
                indicePrefab = Random.Range(0, arboles_papeleras.Length);
            } while (indicePrefab == ultimoPrefabUsado);
        }
        else
        {
            indicePrefab = 0;
        }

        ultimoPrefabUsado = indicePrefab;
        GameObject prefabSeleccionado = arboles_papeleras[indicePrefab];

        // Calcular la posición para el nuevo objeto
        float posX = ultimaPosicionX;
        float posY = 0;
        Vector3 posicionObjeto = new Vector3(posX, posY, posicionZ);

        // Instanciar el nuevo objeto respetando la rotación original
        GameObject nuevoObjeto = Instantiate(prefabSeleccionado, posicionObjeto, prefabSeleccionado.transform.rotation);

        // Aplicar escala específica para árboles/papeleras
        Vector3 escalaOriginal = prefabSeleccionado.transform.localScale;
        nuevoObjeto.transform.localScale = new Vector3(
            escalaOriginal.x * escalaArbolesPapeleras,
            escalaOriginal.y * escalaArbolesPapeleras * mirror,
            escalaOriginal.z * escalaArbolesPapeleras
        );

        lista.Add(nuevoObjeto);
    }

    void DestruirObjetosAntiguos()
    {
        float distanciaDestruccion = umbralGeneracion * 2;

        DestruirObjetosAntiguosDeLista(calleNegativaZ, distanciaDestruccion);
        DestruirObjetosAntiguosDeLista(callePositivaZ, distanciaDestruccion);
        DestruirObjetosAntiguosDeLista(arbolesPapelerasNegativaZ, distanciaDestruccion);
        DestruirObjetosAntiguosDeLista(arbolesPapelerasPositivaZ, distanciaDestruccion);
    }

    void DestruirObjetosAntiguosDeLista(List<GameObject> lista, float distanciaDestruccion)
    {
        for (int i = lista.Count - 1; i >= 0; i--)
        {
            if (lista[i].transform.position.x < liegreCam.position.x - distanciaDestruccion)
            {
                Destroy(lista[i]);
                lista.RemoveAt(i);
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