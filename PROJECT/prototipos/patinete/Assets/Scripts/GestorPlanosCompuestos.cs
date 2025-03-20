using UnityEngine;
using System.Collections.Generic;

public class GestorPlanosCompuestos : MonoBehaviour
{
    public ObjectPool objectPool; // Referencia al GenericObjectPool
    [Header("Configuración Básica")]
    public GameObject[] planoPrefabs; // Array de prefabs de planos
    public Transform jugador; // Referencia al jugador (la bola)
    public float longitudPlano = 10f; // Longitud de cada plano
    public int numeroPlanosEnEscena = 3; // Número de planos que queremos mantener activos

    [Header("Configuración de Generación")]
    [Range(0.1f, 0.9f)]
    public float umbralGeneracion = 0.7f; // Cuando el jugador ha recorrido este % del plano actual, genera uno nuevo

    [Header("Optimización de Meshes")]
    public bool combinarMeshes = true; // Activar/desactivar la combinación de meshes
    public bool usarUnSoloCollider = true; // Usar un solo collider para todos los planos

    private List<GameObject> planosActivos = new List<GameObject>(); // Lista de planos activos
    private GameObject ultimoPrefabUsado; // Último prefab instanciado
    private int planoActualIndex = 0; // Índice del plano en el que está el jugador
    private GameObject planoCompuesto; // Objeto que contendrá los meshes combinados

    public string floorTag = "FloorPrefab"; // Usar un solo collider para todos los planos

    void Start()
    {
        // Crear el objeto para los meshes combinados si es necesario
        if (combinarMeshes)
        {
            planoCompuesto = new GameObject("PlanoCompuesto");
            planoCompuesto.tag = floorTag;

            if (usarUnSoloCollider)
            {
                // Añadir un único Rigidbody al plano compuesto
                Rigidbody rb = planoCompuesto.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Hacerlo kinematic para que no se mueva

                // Añadir un único MeshCollider al plano compuesto
                planoCompuesto.AddComponent<MeshCollider>();
            }
        }

        // Inicializar los pools para cada prefab de planos
        foreach (GameObject prefab in planoPrefabs)
        {
            objectPool.CreatePool(prefab, 10); // Crear un pool de 10 objetos para cada prefab
        }

        // Generar los planos iniciales
        GenerarPlanosIniciales();
    }

    void Update()
    {
        // Verificar en qué plano está el jugador
        ActualizarPlanoActual();

        // Comprobar si necesitamos generar un nuevo plano
        VerificarGeneracionPlano();
    }

    void GenerarPlanosIniciales()
    {
        // Limpiar cualquier plano existente
        foreach (GameObject plano in planosActivos)
        {
            if (plano != null)
                objectPool.ReturnObject(plano); // Devolver al pool en lugar de destruir
        }
        planosActivos.Clear();

        // Generar los planos iniciales
        Vector3 posicionActual = Vector3.zero;
        for (int i = 0; i < numeroPlanosEnEscena; i++)
        {
            GameObject nuevoPrefab = SeleccionarPrefabAleatorio();
            GameObject nuevoPlano = objectPool.GetObject(nuevoPrefab); // Obtener del pool
            nuevoPlano.transform.position = posicionActual;
            nuevoPlano.transform.rotation = Quaternion.identity;

            if (combinarMeshes)
            {
                // Desactivar componentes físicos si vamos a combinar meshes
                if (usarUnSoloCollider)
                {
                    DesactivarComponentesFisicos(nuevoPlano);
                }

                // Hacer el plano hijo del plano compuesto
                nuevoPlano.transform.SetParent(planoCompuesto.transform);
            }

            planosActivos.Add(nuevoPlano);

            // Actualizar la posición para el siguiente plano
            posicionActual += Vector3.forward * longitudPlano;
        }

        planoActualIndex = 0;

        // Combinar los meshes si está activada la opción
        if (combinarMeshes)
        {
            CombinarMeshes();
        }
    }

    void DesactivarComponentesFisicos(GameObject objeto)
    {
        // Desactivar Rigidbody si existe
        Rigidbody rb = objeto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Hacerlo kinematic para que no se mueva
        }

        // Desactivar Colliders si vamos a usar uno solo
        if (usarUnSoloCollider)
        {
            Collider[] colliders = objeto.GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false; // Desactivar en lugar de destruir
            }
        }

        // También desactivar componentes en hijos
        foreach (Transform hijo in objeto.transform)
        {
            DesactivarComponentesFisicos(hijo.gameObject);
        }
    }

    GameObject SeleccionarPrefabAleatorio()
    {
        if (planoPrefabs.Length == 0)
            return null;

        GameObject prefabSeleccionado;

        // Si solo hay un prefab, usarlo
        if (planoPrefabs.Length == 1)
            return planoPrefabs[0];

        // Seleccionar un prefab diferente al último usado
        do
        {
            prefabSeleccionado = planoPrefabs[Random.Range(0, planoPrefabs.Length)];
        } while (prefabSeleccionado == ultimoPrefabUsado && planoPrefabs.Length > 1);

        ultimoPrefabUsado = prefabSeleccionado;
        return prefabSeleccionado;
    }

    void ActualizarPlanoActual()
    {
        if (planosActivos.Count == 0)
            return;

        // Determinar en qué plano está el jugador basado en su posición Z
        for (int i = 0; i < planosActivos.Count; i++)
        {
            float inicioPlanoZ = planosActivos[i].transform.position.z;
            float finPlanoZ = inicioPlanoZ + longitudPlano;

            if (jugador.position.z >= inicioPlanoZ && jugador.position.z < finPlanoZ)
            {
                planoActualIndex = i;
                break;
            }
        }
    }

    void VerificarGeneracionPlano()
    {
        if (planosActivos.Count == 0 || planoActualIndex >= planosActivos.Count)
            return;

        // Calcular la posición relativa del jugador en el plano actual
        float inicioPlanoZ = planosActivos[planoActualIndex].transform.position.z;
        float finPlanoZ = inicioPlanoZ + longitudPlano;
        float posicionRelativa = (jugador.position.z - inicioPlanoZ) / longitudPlano;

        // Si el jugador ha avanzado lo suficiente en el plano actual
        if (posicionRelativa > umbralGeneracion)
        {
            // Si estamos en el penúltimo plano, generar uno nuevo
            if (planoActualIndex >= planosActivos.Count - 2)
            {
                GenerarNuevoPlano();
                EliminarPlanoAntiguo();

                // Recombinar los meshes si está activada la opción
                if (combinarMeshes)
                {
                    CombinarMeshes();
                }
            }
        }
    }

    void GenerarNuevoPlano()
    {
        if (planosActivos.Count == 0)
            return;

        // Calcular la posición del nuevo plano
        Vector3 posicionUltimoPlano = planosActivos[planosActivos.Count - 1].transform.position;
        Vector3 posicionNuevoPlano = posicionUltimoPlano + Vector3.forward * longitudPlano;

        // Obtener el nuevo plano del pool
        GameObject nuevoPrefab = SeleccionarPrefabAleatorio();
        GameObject nuevoPlano = objectPool.GetObject(nuevoPrefab); // Obtener del pool
        nuevoPlano.transform.position = posicionNuevoPlano;
        nuevoPlano.transform.rotation = Quaternion.identity;

        if (combinarMeshes)
        {
            // Desactivar componentes físicos si vamos a combinar meshes
            if (usarUnSoloCollider)
            {
                DesactivarComponentesFisicos(nuevoPlano);
            }

            // Hacer el plano hijo del plano compuesto
            nuevoPlano.transform.SetParent(planoCompuesto.transform);
        }

        // Añadir a la lista de planos activos
        planosActivos.Add(nuevoPlano);

        Debug.Log($"Nuevo plano generado en Z: {posicionNuevoPlano.z}");
    }

    void EliminarPlanoAntiguo()
    {
        // Si tenemos más planos de los necesarios, eliminar el más antiguo
        if (planosActivos.Count > numeroPlanosEnEscena)
        {
            GameObject planoAntiguo = planosActivos[0];
            planosActivos.RemoveAt(0);

            Debug.Log($"Plano eliminado en Z: {planoAntiguo.transform.position.z}");
            objectPool.ReturnObject(planoAntiguo); // Devolver al pool en lugar de destruir

            // Actualizar el índice del plano actual
            planoActualIndex--;
            if (planoActualIndex < 0) planoActualIndex = 0;
        }
    }

    void CombinarMeshes()
    {
        // Recolectar todos los MeshFilters de los planos activos
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (GameObject plano in planosActivos)
        {
            if (plano != null)
            {
                meshFilters.AddRange(plano.GetComponentsInChildren<MeshFilter>());
            }
        }

        if (meshFilters.Count == 0)
            return;

        // Combinar los meshes
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++)
        {
            if (meshFilters[i] != null && meshFilters[i].sharedMesh != null)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }
        }

        // Crear un nuevo mesh combinado
        Mesh meshCombinado = new Mesh();
        meshCombinado.CombineMeshes(combine);

        // Asignar el mesh combinado al MeshFilter del plano compuesto
        MeshFilter meshFilter = planoCompuesto.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = planoCompuesto.AddComponent<MeshFilter>();

        meshFilter.mesh = meshCombinado;

        // Asignar el mesh al MeshCollider si existe
        if (usarUnSoloCollider)
        {
            MeshCollider meshCollider = planoCompuesto.GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = meshCombinado;
        }

        // Añadir un MeshRenderer si no existe
        MeshRenderer meshRenderer = planoCompuesto.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = planoCompuesto.AddComponent<MeshRenderer>();

        // Configurar el material (puedes ajustar esto según tus necesidades)
        if (planosActivos.Count > 0 && planosActivos[0] != null)
        {
            MeshRenderer primerRenderer = planosActivos[0].GetComponent<MeshRenderer>();
            if (primerRenderer != null && primerRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial = primerRenderer.sharedMaterial;
            }
        }
    }

    // Método para depuración - dibuja gizmos para visualizar los límites de los planos
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 0; i < planosActivos.Count; i++)
        {
            if (planosActivos[i] == null)
                continue;

            Vector3 inicio = planosActivos[i].transform.position;
            Vector3 fin = inicio + Vector3.forward * longitudPlano;

            // Dibujar línea para cada plano
            Gizmos.color = (i == planoActualIndex) ? Color.green : Color.yellow;
            Gizmos.DrawLine(inicio + Vector3.up * 0.1f, fin + Vector3.up * 0.1f);

            // Marcar el umbral de generación
            Vector3 umbral = inicio + Vector3.forward * (longitudPlano * umbralGeneracion);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(umbral + Vector3.up * 0.1f, 0.2f);
        }
    }
}