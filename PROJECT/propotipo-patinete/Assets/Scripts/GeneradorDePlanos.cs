using UnityEngine;
using System.Collections.Generic;

public class GeneradorDePlanos : MonoBehaviour
{
    public GameObject planoPrefab; // Prefab del plano
    public Transform jugador; // Referencia al jugador (la bola)
    public float longitudPlano; // Longitud de cada plano

    private List<GameObject> planosActivos = new List<GameObject>(); // Lista de planos activos
    private Vector3 ultimaPosicionGenerada; // Última posición donde se generó un plano
    private float radioBola; // Radio de la bola

    void Start()
    {
        // Obtener el radio de la bola (asumiendo que la bola es una esfera)
        radioBola = jugador.localScale.x / 2f;
        // Generar el primer plano debajo de la bola
        ultimaPosicionGenerada = jugador.position - Vector3.up * radioBola;
        GenerarNuevoPlano();
    }

    void Update()
    {
        // Verificar si el jugador está cerca del final del último plano generado solo en el eje X
        if (Mathf.Abs(jugador.position.x - ultimaPosicionGenerada.x) < longitudPlano * 0.2f)
        {
            GenerarNuevoPlano();
            DestruirPlanoAntiguo();
        }
    }

    void GenerarNuevoPlano()
    {
        // Calcular la posición del nuevo plano (justo después del último plano generado)
        Vector3 nuevaPosicion = ultimaPosicionGenerada + Vector3.right * longitudPlano;
        // Ajustar la posición del plano para que esté debajo de la bola
        nuevaPosicion.y = jugador.position.y - radioBola;
        // Instanciar el nuevo plano
        GameObject nuevoPlano = Instantiate(planoPrefab, nuevaPosicion, Quaternion.Euler(0, 0, 0));
        // Añadir el nuevo plano a la lista de planos activos
        planosActivos.Add(nuevoPlano);
        // Actualizar la última posición generada
        ultimaPosicionGenerada = nuevaPosicion;
    }

    void DestruirPlanoAntiguo()
    {
        // Destruir el plano más antiguo si hay más de 2 planos activos
        if (planosActivos.Count > 2)
        {
            GameObject planoAntiguo = planosActivos[0];
            planosActivos.RemoveAt(0);
            Destroy(planoAntiguo);
        }
    }
}