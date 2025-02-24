using UnityEngine;
public class GeneradorDePlanos : MonoBehaviour
{
    public GameObject planoPrefab; // Prefab del plano
    public Transform jugador; // Referencia al jugador (la bola)
    public float distanciaGeneracion = 20f; // Distancia a la que se genera un nuevo plano
    public float longitudPlano = 10f; // Longitud de cada plano

    private Vector3 ultimaPosicionGenerada; // Última posición donde se generó un plano

    void Start()
    {
        ultimaPosicionGenerada = jugador.position;
    }

    void Update()
    {
        // Verificar si el jugador está cerca del final del último plano generado
        if (Vector3.Distance(jugador.position, ultimaPosicionGenerada) < distanciaGeneracion)
        {
            GenerarNuevoPlano();
        }
    }

    void GenerarNuevoPlano()
    {
        // Calcular la posición del nuevo plano
        Vector3 nuevaPosicion = ultimaPosicionGenerada + Vector3.forward * longitudPlano;

        // Instanciar el nuevo plano
        Instantiate(planoPrefab, nuevaPosicion, Quaternion.identity);

        // Actualizar la última posición generada
        ultimaPosicionGenerada = nuevaPosicion;
    }
}