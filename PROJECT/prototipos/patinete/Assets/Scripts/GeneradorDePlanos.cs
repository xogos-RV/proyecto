using UnityEngine;
using System.Collections.Generic;

public class GeneradorDePlanos : MonoBehaviour
{
    private const float thresholdNextObject = 0.2f;
    public GameObject[] planoPrefabs; // Array de prefabs de planos
    public GameObject[] objetosPrefabs; // Array de prefabs de objetos (esferas, cubos, etc.)
    public Transform jugador; // Referencia al jugador (la bola)
    public float longitudPlano; // Longitud de cada plano // TODO quitarlo del plano
    private List<GameObject> planosActivos = new List<GameObject>(); // Lista de planos activos

    private GameObject lastPrefab; // Último prefab instanciado
    Vector3 lastPosition; // TODO QUIERO sustituir mi sistema de dos planoPrefabs en escena controlados  con lastPosition y nextPosition para mantener un array de planoPrefabs  
    private Vector3 nextPosition; // Última posición donde termina el último plano generado

    void Start()
    {
        initPlane();
    }

    void Update()
    {
        if (jugador.position.x - lastPosition.x > longitudPlano * thresholdNextObject)
        {
            Debug.Log($"-------------------------------------------------------------");
            GenerarNuevoPlano();
            DestruirPlanoAntiguo();
        }
    }

    void initPlane()
    {
        lastPosition = Vector3.zero;
        GameObject nuevoPlano = Instantiate(SeleccionarPrefab(), lastPosition, Quaternion.Euler(0, 0, 0));
        planosActivos.Add(nuevoPlano);
        //  GenerarObjetosEnPlano(nuevoPlano, planoPrefabs, longitudPlano);
        Debug.Log($"lastEndPosition: {lastPosition}");
        nextPosition = lastPosition + Vector3.right * longitudPlano;
    }

    void GenerarNuevoPlano()
    {
        Debug.Log($"[GenerarNuevoPlano]genera en     : {nextPosition}");
        GameObject nuevoPlano = Instantiate(SeleccionarPrefab(), nextPosition, Quaternion.Euler(0, 0, 0));
        planosActivos.Add(nuevoPlano);

        // Generar objetos sobre el nuevo plano
        //  GenerarObjetosEnPlano(nuevoPlano, planoPrefabs, longitudPlano);

        lastPosition = nextPosition;
        nextPosition += Vector3.right * longitudPlano;
        Debug.Log($"[GenerarNuevoPlano]lastEndPosition: {lastPosition}");
    }


    GameObject SeleccionarPrefab()
    {
        GameObject prefabSeleccionado;
        do
        {
            // Seleccionar un prefab aleatorio
            prefabSeleccionado = planoPrefabs[Random.Range(0, planoPrefabs.Length)];
        } while (prefabSeleccionado == lastPrefab); // Asegurarse de que no sea el mismo que el último

        lastPrefab = prefabSeleccionado; // Actualizar el último prefab
        return prefabSeleccionado;
    }

    void DestruirPlanoAntiguo()
    {
        if (planosActivos.Count > 2)
        {
            GameObject planoAntiguo = planosActivos[0];
            planosActivos.RemoveAt(0);
            Destroy(planoAntiguo);
            Debug.Log($"[DestruirPlanoAntiguo] lastEndPosition: {nextPosition}");
        }
    }

}