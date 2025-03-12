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

    [Header("Configuración de Posición X")]
    public float posicionFijaX = 20f;
    public float posicionPasilloX = 15f; // Posición X para los pasillos de árboles/papeleras

    [Header("Configuración de Escala")]
    [Range(1, 3)]
    public float escala = 2f;
    [Range(0.5f, 2f)]
    public float escalaArbolesPapeleras = 1.5f; // Escala específica para árboles/papeleras

    [Header("Configuración de Espacios")]
    [Range(2, 5)]
    public float espacioMinimoEntrePrefabs = 2f;
    [Range(10, 20)]
    public float espacioMaximoEntrePrefabs = 5f;

    [Header("Probabilidad de Generación")]
    [Range(0.5f, 1f)]
    public float probabilidadGeneracion = 0.7f; // Probabilidad de generar un edificio

    private float ultimaPosicionGeneracionZNegativaX = 0f; // Última posición Z en la calle -x
    private float ultimaPosicionGeneracionZPositivaX = 0f; // Última posición Z en la calle +x

    // Para árboles y papeleras
    private float ultimaPosicionArbolPapeleraPositivaX = 0f;
    private float ultimaPosicionArbolPapeleraNegativaX = 0f;

    private List<GameObject> calleNegativaX = new List<GameObject>(); // Lista de objetos en -x
    private List<GameObject> callePositivaX = new List<GameObject>(); // Lista de objetos en +x
    private List<GameObject> arbolesPapelerasPositivaX = new List<GameObject>(); // Lista de árboles/papeleras en +x
    private List<GameObject> arbolesPapelerasNegativaX = new List<GameObject>(); // Lista de árboles/papeleras en -x

    // Para controlar el último prefab usado en cada calle
    private int ultimoPrefabUsadoPositivaX = -1;
    private int ultimoPrefabUsadoNegativaX = -1;
    private int ultimoArbolPapeleraUsadoPositivaX = -1;
    private int ultimoArbolPapeleraUsadoNegativaX = -1;

    // Para controlar si el último espacio fue vacío
    private bool ultimoEspacioVacioPositivaX = false;
    private bool ultimoEspacioVacioNegativaX = false;

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

        /* GenerarElementoEnCalle(ref ultimaPosicionGeneracionZPositivaX, posicionFijaX, callePositivaX, 1, ref ultimoPrefabUsadoPositivaX, ref ultimoEspacioVacioPositivaX);
        GenerarElementoEnCalle(ref ultimaPosicionGeneracionZNegativaX, -posicionFijaX, calleNegativaX, -1, ref ultimoPrefabUsadoNegativaX, ref ultimoEspacioVacioNegativaX);

        // Inicializar la generación de árboles y papeleras
        if (arboles_papeleras != null && arboles_papeleras.Length > 0)
        {
            GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraPositivaX, posicionPasilloX, arbolesPapelerasPositivaX, -1, ref ultimoArbolPapeleraUsadoPositivaX);
            GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraNegativaX, -posicionPasilloX, arbolesPapelerasNegativaX, 1, ref ultimoArbolPapeleraUsadoNegativaX);
        } */
    }

    void Update()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;
        // Calle L
        if (liegreCam.position.z > ultimaPosicionGeneracionZNegativaX - umbralGeneracion)
        {
            GenerarElementoEnCalle(ref ultimaPosicionGeneracionZNegativaX, -posicionFijaX, calleNegativaX, -1, ref ultimoPrefabUsadoNegativaX, ref ultimoEspacioVacioNegativaX);
        }
        // Calle R
        if (liegreCam.position.z > ultimaPosicionGeneracionZPositivaX - umbralGeneracion)
        {
            GenerarElementoEnCalle(ref ultimaPosicionGeneracionZPositivaX, posicionFijaX, callePositivaX, 1, ref ultimoPrefabUsadoPositivaX, ref ultimoEspacioVacioPositivaX);
        }


        // Árboles y papeleras
        if (arboles_papeleras != null && arboles_papeleras.Length > 0)
        {
            // Pasillo +X (lado positivo)
            if (liegreCam.position.z > ultimaPosicionArbolPapeleraPositivaX - umbralGeneracion)
            {
                GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraPositivaX, posicionPasilloX, arbolesPapelerasPositivaX, 1, ref ultimoArbolPapeleraUsadoPositivaX);
            }
            // Pasillo -X (lado negativo)
            if (liegreCam.position.z > ultimaPosicionArbolPapeleraNegativaX - umbralGeneracion)
            {
                GenerarArbolPapelera(ref ultimaPosicionArbolPapeleraNegativaX, -posicionPasilloX, arbolesPapelerasNegativaX, 1, ref ultimoArbolPapeleraUsadoNegativaX);
            }
        }

        DestruirObjetosAntiguos();
    }

    void GenerarElementoEnCalle(ref float ultimaPosicionZ, float posicionX, List<GameObject> calle, int mirror, ref int ultimoPrefabUsado, ref bool ultimoEspacioVacio)
    {
        if (buildingPrefabs.Length == 0) return;

        // Añadir un espacio aleatorio entre edificios
        float espacioAleatorio = Random.Range(espacioMinimoEntrePrefabs, espacioMaximoEntrePrefabs);
        ultimaPosicionZ += espacioAleatorio;

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
            float posZ = ultimaPosicionZ;
            float posY = 0;
            Vector3 posicionObjeto = new Vector3(posicionX + (mirror * escala * size.x), posY, posZ);
            Debug.Log("objetoooooo  " + prefabSeleccionado.name + ", SIZE: " + size.ToString() + ",  posicionObjeto" + posicionObjeto.ToString());
            // Instanciar el nuevo objeto RESPETANDO LA ROTACIÓN ORIGINAL DEL PREFAB
            GameObject nuevoObjeto = Instantiate(prefabSeleccionado, posicionObjeto, prefabSeleccionado.transform.rotation);

            Rigidbody rb = nuevoObjeto.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = nuevoObjeto.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;

            // Actualizar la última posición y añadir a la lista
            ultimaPosicionZ = posZ + escala * size.z;

            // Aplicar escala pero mantener la rotación original
            Vector3 escalaOriginal = prefabSeleccionado.transform.localScale;
            nuevoObjeto.transform.localScale = new Vector3(
                escalaOriginal.x * escala,
                escalaOriginal.y * escala,
                escalaOriginal.z * escala * mirror * -1
            );

            calle.Add(nuevoObjeto);

            ultimoEspacioVacio = false;
        }
        else
        {
            // Si no generamos un edificio, solo actualizamos la posición
            ultimaPosicionZ += 5f; // Un espacio vacío estándar
            ultimoEspacioVacio = true;
            Debug.Log("Espacio vacío generado en posición Z: " + ultimaPosicionZ);
        }
    }

    void GenerarArbolPapelera(ref float ultimaPosicionZ, float posicionX, List<GameObject> lista, int mirror, ref int ultimoPrefabUsado)
    {
        if (arboles_papeleras.Length == 0) return;

        // Avanzar la posición Z según la distancia uniforme configurada
        ultimaPosicionZ += distanciaUniformeArbolesPapeleras;

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
        float posZ = ultimaPosicionZ;
        float posY = 0;
        Vector3 posicionObjeto = new Vector3(posicionX, posY, posZ);

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

        DestruirObjetosAntiguosDeLista(calleNegativaX, distanciaDestruccion);
        DestruirObjetosAntiguosDeLista(callePositivaX, distanciaDestruccion);
        DestruirObjetosAntiguosDeLista(arbolesPapelerasNegativaX, distanciaDestruccion);
        DestruirObjetosAntiguosDeLista(arbolesPapelerasPositivaX, distanciaDestruccion);
    }

    void DestruirObjetosAntiguosDeLista(List<GameObject> lista, float distanciaDestruccion)
    {
        for (int i = lista.Count - 1; i >= 0; i--)
        {
            if (lista[i].transform.position.z < liegreCam.position.z - distanciaDestruccion)
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