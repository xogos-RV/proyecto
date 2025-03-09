using UnityEngine;

public class GenerateStreetBuildings : MonoBehaviour
{
    public Transform liegreCam; // Referencia al jugador (la bola)

    public GameObject[] objetosPrefabs;

    // Configuración para la generación de objetos
    [Header("Configuración de Objetos")]
    public int minObjetosPorPlano = 1;
    public int maxObjetosPorPlano = 5;
    public float alturaMinima = 1f;
    public float alturaMaxima = 2.0f;
    public bool distribuirUniformemente = true;

    // Configuración para la posición Z
    [Header("Configuración de Posición Z")]
    public float anchoPasillo = 10f; // Ancho del pasillo en el eje Z

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

    // Configuración para la generación continua
    [Header("Configuración de Generación Continua")]
    public float distanciaGeneracion = 20f; // Distancia delante del jugador para generar objetos
    public float distanciaDestruccion = 10f; // Distancia detrás del jugador para destruir objetos

    private float ultimaPosicionGeneracionX = 0f;

    void Start()
    {
        // Inicializar la posición de generación
        ultimaPosicionGeneracionX = liegreCam.position.x;
    }

    void Update()
    {
        // Verificar si el jugador ha avanzado lo suficiente para generar nuevos objetos
        if (liegreCam.position.x > ultimaPosicionGeneracionX - distanciaGeneracion)
        {
            GenerarObjetosDelanteDelJugador();
            ultimaPosicionGeneracionX += distanciaGeneracion;
        }
    }

    public void GenerarObjetosDelanteDelJugador()
    {
        if (objetosPrefabs.Length == 0) return;

        // Determinar cuántos objetos generar
        int cantidadObjetos = Random.Range(minObjetosPorPlano, maxObjetosPorPlano + 1);

        for (int i = 0; i < cantidadObjetos; i++)
        {
            // Seleccionar un prefab aleatorio de la lista de objetos
            GameObject objetoPrefab = objetosPrefabs[Random.Range(0, objetosPrefabs.Length)];

            // Calcular posición X delante del jugador
            float posX = liegreCam.position.x + distanciaGeneracion;

            // Altura aleatoria
            float posY = liegreCam.position.y + Random.Range(alturaMinima, alturaMaxima);

            // Posición Z aleatoria dentro de los límites del pasillo
            float mitadAncho = anchoPasillo / 2f;
            float posZ = liegreCam.position.z + Random.Range(-mitadAncho, mitadAncho);

            Vector3 posicionObjeto = new Vector3(posX, posY, posZ);

            // Instanciar el objeto
            GameObject nuevoObjeto = Instantiate(objetoPrefab, posicionObjeto, Quaternion.identity);

            // Aplicar escala aleatoria
            AplicarEscalaAleatoria(nuevoObjeto);

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
                    fuerza.x = Random.Range(-fuerzaMaxima / 2, fuerzaMaxima / 2);
                    fuerza.z = Random.Range(-fuerzaMaxima / 2, fuerzaMaxima / 2);
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
}