using UnityEngine;
using System.Collections.Generic;

public class GenerateStreetBuildings : MonoBehaviour
{
    public Transform liegreCam; // Referencia al jugador (la bola)

    // Configuración para la generación de objetos
    [Header("Configuración de Objetos")]
    public GameObject objetosPrefab; // Prefab único que se repetirá
    public float umbralGeneracion = 20f; // Umbral en X para generar nuevos objetos

    // Configuración para la posición Z
    [Header("Configuración de Posición Z")]
    public float posicionFijaZ = 20f; // Posición fija en Z (tanto -z como +z)

    // Configuración para la escala de los objetos
    [Header("Configuración de Escala")]
    [Range(10f, 30f)]
    public float anchoMinimo = 10f; // Ancho mínimo en X
    [Range(10f, 30f)]
    public float anchoMaximo = 30f; // Ancho máximo en X
    [Range(5f, 20f)]
    public float alturaMinima = 5f; // Altura mínima en Y
    [Range(5f, 20f)]
    public float alturaMaxima = 20f; // Altura máxima en Y
    public bool escalaUniforme = true; // Si es true, la escala será igual en X, Y, Z

    private float ultimaPosicionGeneracionXNegativaZ = 0f; // Última posición X en la calle -z
    private float ultimaPosicionGeneracionXPositivaZ = 0f; // Última posición X en la calle +z
    private List<GameObject> calleNegativaZ = new List<GameObject>(); // Lista de objetos en -z
    private List<GameObject> callePositivaZ = new List<GameObject>(); // Lista de objetos en +z

    void Start()
    {
        // Generar los primeros objetos al inicio
        GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ);
        GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ);
    }

    void Update()
    {
        // Verificar si el jugador ha alcanzado el umbral para generar nuevos objetos en -z
        if (liegreCam.position.x > ultimaPosicionGeneracionXNegativaZ - umbralGeneracion)
        {
            GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ, calleNegativaZ);
        }

        // Verificar si el jugador ha alcanzado el umbral para generar nuevos objetos en +z
        if (liegreCam.position.x > ultimaPosicionGeneracionXPositivaZ - umbralGeneracion)
        {
            GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ, callePositivaZ);
        }

        // Destruir objetos que ya no son necesarios (opcional)
        DestruirObjetosAntiguos();
    }

    void GenerarObjetoEnCalle(ref float ultimaPosicionX, float posicionZ, List<GameObject> calle)
    {
        if (objetosPrefab == null) return;

        // Calcular la escala X del nuevo objeto
        float escalaX = Random.Range(anchoMinimo, anchoMaximo);

        // Calcular la escala Y del nuevo objeto
        float escalaY = Random.Range(alturaMinima, alturaMaxima);

        // Calcular la posición X del nuevo objeto
        float posX = ultimaPosicionX + escalaX / 2f; // Centrar el objeto

        // Calcular la posición Y (escalaY / 2 para que esté sobre el plano Y = 0)
        float posY = escalaY / 2f;

        Vector3 posicionObjeto = new Vector3(posX, posY, posicionZ);

        // Instanciar el objeto
        GameObject nuevoObjeto = Instantiate(objetosPrefab, posicionObjeto, Quaternion.identity);

        // Añadir Rigidbody y configurarlo como kinematic
        Rigidbody rb = nuevoObjeto.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = nuevoObjeto.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Hacer el objeto kinematic

        // Aplicar escala aleatoria
        if (escalaUniforme)
        {
            // Escala uniforme (igual en todos los ejes)
            nuevoObjeto.transform.localScale = new Vector3(escalaX, escalaY, escalaX);
        }
        else
        {
            // Escala no uniforme (diferente en cada eje)
            float escalaZ = Random.Range(anchoMinimo, anchoMaximo); // Escala Z aleatoria
            nuevoObjeto.transform.localScale = new Vector3(escalaX, escalaY, escalaZ);
        }

        // Actualizar la última posición de generación
        ultimaPosicionX = posX + escalaX / 2f; // Sumar la mitad de la escala X para el siguiente objeto

        // Almacenar el objeto generado en la lista
        calle.Add(nuevoObjeto);
    }

    void DestruirObjetosAntiguos()
    {
        // Destruir objetos en -z que están muy atrás del jugador
        for (int i = calleNegativaZ.Count - 1; i >= 0; i--)
        {
            if (calleNegativaZ[i].transform.position.x < liegreCam.position.x - umbralGeneracion * 2)
            {
                Destroy(calleNegativaZ[i]);
                calleNegativaZ.RemoveAt(i);
            }
        }

        // Destruir objetos en +z que están muy atrás del jugador
        for (int i = callePositivaZ.Count - 1; i >= 0; i--)
        {
            if (callePositivaZ[i].transform.position.x < liegreCam.position.x - umbralGeneracion * 2)
            {
                Destroy(callePositivaZ[i]);
                callePositivaZ.RemoveAt(i);
            }
        }
    }
}