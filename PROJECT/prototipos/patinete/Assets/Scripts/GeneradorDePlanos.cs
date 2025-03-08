using UnityEngine;
using System.Collections.Generic;

public class GeneradorDePlanos : MonoBehaviour
{
    private const float thresholdNextObject = 0.2f;
    public GameObject[] planoPrefabs; // Array de prefabs de planos
    public Transform jugador; // Referencia al jugador (la bola)
    public float longitudPlano; // Longitud de cada plano
    private List<GameObject> planosActivos = new List<GameObject>(); // Lista de planos activos
    private Vector3 nextPosition; // Última posición donde termina el último plano generado
    private GameObject lastPrefab; // Último prefab instanciado
    Vector3 lastPosition;

    void Start()
    {
        initPlane();
    }

    void Update()
    {
        // - Debug.Log($"pos relativa jugador : {jugador.position.x - lastPosition.x} ----------- longitudPlano * threshold: {longitudPlano * thresholdNextObject} ");

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
        Debug.Log($"lastEndPosition: {lastPosition}");
        nextPosition = lastPosition + Vector3.right * longitudPlano;
    }

    void GenerarNuevoPlano()
    {
        Debug.Log($"[GenerarNuevoPlano]genera en     : {nextPosition}");
        GameObject nuevoPlano = Instantiate(SeleccionarPrefab(), nextPosition, Quaternion.Euler(0, 0, 0));
        planosActivos.Add(nuevoPlano);
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