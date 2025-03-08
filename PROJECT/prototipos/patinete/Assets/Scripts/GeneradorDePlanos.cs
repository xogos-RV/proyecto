using UnityEngine;
using System.Collections.Generic;

public class GeneradorDePlanos : MonoBehaviour
{
    private const float thresholdNextObject = 0.2f;
    public GameObject[] planoPrefabs; // Array de prefabs de planos
    public GameObject[] objetosPrefabs; // Array de prefabs de objetos (esferas, cubos, etc.)
    public Transform jugador; // Referencia al jugador (la bola)
    public float longitudPlano; // Longitud de cada plano
    private List<GameObject> planosActivos = new List<GameObject>(); // Lista de planos activos
    private Vector3 nextPosition; // Última posición donde termina el último plano generado
    private GameObject lastPrefab; // Último prefab instanciado
    Vector3 lastPosition;
    
    // Configuración para la generación de objetos
    [Header("Configuración de Objetos")]
    public int minObjetosPorPlano = 1;
    public int maxObjetosPorPlano = 5;
    public float alturaMinima = 1f;
    public float alturaMaxima = 2.0f;
    public bool distribuirUniformemente = true;
    
    // Configuración para la posición Z
    [Header("Configuración de Posición Z")]
    public float anchoPlano = 10f; // Ancho del plano en el eje Z
    
    // Configuración para la fuerza inicial
    [Header("Configuración de Fuerza Inicial")]
    public float fuerzaMinima = 1f;
    public float fuerzaMaxima = 5f;
    public bool aplicarFuerzaVertical = true;
    public bool aplicarFuerzaHorizontal = false;
    
    // Configuración para la escala de los objetos
    [Header("Configuración de Escala")]
    [Range(2f, 10f)]
    public float escalaMinima = 2f;
    [Range(2f, 10f)]
    public float escalaMaxima = 10f;
    public bool escalaUniforme = true; // Si es true, la escala será igual en X, Y, Z

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
        GenerarObjetosEnPlano(nuevoPlano);
        Debug.Log($"lastEndPosition: {lastPosition}");
        nextPosition = lastPosition + Vector3.right * longitudPlano;
    }

    void GenerarNuevoPlano()
    {
        Debug.Log($"[GenerarNuevoPlano]genera en     : {nextPosition}");
        GameObject nuevoPlano = Instantiate(SeleccionarPrefab(), nextPosition, Quaternion.Euler(0, 0, 0));
        planosActivos.Add(nuevoPlano);
        
        // Generar objetos sobre el nuevo plano
        GenerarObjetosEnPlano(nuevoPlano);
        
        lastPosition = nextPosition;
        nextPosition += Vector3.right * longitudPlano;
        Debug.Log($"[GenerarNuevoPlano]lastEndPosition: {lastPosition}");
    }

    void GenerarObjetosEnPlano(GameObject plano)
    {
        if (objetosPrefabs.Length == 0) return;
        
        // Determinar cuántos objetos generar
        int cantidadObjetos = Random.Range(minObjetosPorPlano, maxObjetosPorPlano + 1);
        
        for (int i = 0; i < cantidadObjetos; i++)
        {
            // Seleccionar un prefab aleatorio de la lista de objetos
            GameObject objetoPrefab = objetosPrefabs[Random.Range(0, objetosPrefabs.Length)];
            
            // Calcular posición X
            float posX;
            if (distribuirUniformemente)
            {
                // Distribuir uniformemente a lo largo del plano
                posX = plano.transform.position.x + (i + 1) * longitudPlano / (cantidadObjetos + 1);
            }
            else
            {
                // Distribuir aleatoriamente
                posX = plano.transform.position.x + Random.Range(0, longitudPlano);
            }
            
            // Altura aleatoria
            float posY = plano.transform.position.y + Random.Range(alturaMinima, alturaMaxima);
            
            // Posición Z aleatoria dentro de los límites del plano
            float mitadAncho = anchoPlano / 2f;
            float posZ = plano.transform.position.z + Random.Range(-mitadAncho, mitadAncho);
            
            Vector3 posicionObjeto = new Vector3(posX, posY, posZ);
            
            // Instanciar el objeto
            GameObject nuevoObjeto = Instantiate(objetoPrefab, posicionObjeto, Quaternion.identity);
            
            // Aplicar escala aleatoria
            AplicarEscalaAleatoria(nuevoObjeto);
            
            // Hacer el objeto hijo del plano para que se destruya con él
            nuevoObjeto.transform.SetParent(plano.transform);
            
            // Aplicar fuerza inicial aleatoria si el objeto tiene Rigidbody
            Rigidbody rb = nuevoObjeto.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Asegurarse de que el Rigidbody no sea kinematic
                rb.isKinematic = false;
                
                // Calcular la fuerza aleatoria
                Vector3 fuerza = Vector3.zero;
                
                if (aplicarFuerzaVertical)
                {
                    // Fuerza vertical (siempre hacia arriba para que salte)
                    fuerza.y = Random.Range(fuerzaMinima, fuerzaMaxima);
                }
                
                if (aplicarFuerzaHorizontal)
                {
                    // Fuerza horizontal aleatoria (X y Z)
                    fuerza.x = Random.Range(-fuerzaMaxima/2, fuerzaMaxima/2);
                    fuerza.z = Random.Range(-fuerzaMaxima/2, fuerzaMaxima/2);
                }
                
                // Aplicar la fuerza como impulso
                rb.AddForce(fuerza, ForceMode.Impulse);
                
                // Opcionalmente, añadir un poco de torque (rotación)
                rb.AddTorque(new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ), ForceMode.Impulse);
            }
        }
    }

    void AplicarEscalaAleatoria(GameObject objeto)
    {
        if (escalaUniforme)
        {
            // Escala uniforme (igual en todos los ejes)
            float escala = Random.Range(escalaMinima, escalaMaxima);
            objeto.transform.localScale = new Vector3(escala, escala, escala);
        }
        else
        {
            // Escala no uniforme (diferente en cada eje)
            float escalaX = Random.Range(escalaMinima, escalaMaxima);
            float escalaY = Random.Range(escalaMinima, escalaMaxima);
            float escalaZ = Random.Range(escalaMinima, escalaMaxima);
            objeto.transform.localScale = new Vector3(escalaX, escalaY, escalaZ);
        }
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