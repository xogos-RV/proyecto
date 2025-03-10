using UnityEngine;
using System.Collections.Generic;

public class GenerateStreetBuildings : MonoBehaviour
{
    public Transform liegreCam; // Referencia al jugador (la bola)

    // Configuración para la generación de objetos
    [Header("Configuración de Objetos")]
    public GameObject buildingPrefab;
    public float umbralGeneracion = 20f;

    [Header("Configuración de Posición Z")]
    public float posicionFijaZ = 20f;

    [Header("Configuración de Escala")]
    [Range(1, 3)]
    public float escala = 2f;


    private float ultimaPosicionGeneracionXNegativaZ = 0f; // Última posición X en la calle -z
    private float ultimaPosicionGeneracionXPositivaZ = 0f; // Última posición X en la calle +z
    private List<GameObject> calleNegativaZ = new List<GameObject>(); // Lista de objetos en -z
    private List<GameObject> callePositivaZ = new List<GameObject>(); // Lista de objetos en +z
    Vector3 size;

    void Start()
    {
        size = getSize(buildingPrefab);
        GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ + escala * size.z, callePositivaZ, 1);
        GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ - escala * size.z, calleNegativaZ, -1);
    }

    void Update()
    {
        // Calle +Z
        if (liegreCam.position.x > ultimaPosicionGeneracionXPositivaZ - umbralGeneracion)
        {
            GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXPositivaZ, posicionFijaZ + escala * size.z, callePositivaZ, 1);
        }
        // Calle -Z
        if (liegreCam.position.x > ultimaPosicionGeneracionXNegativaZ - umbralGeneracion)
        {
            GenerarObjetoEnCalle(ref ultimaPosicionGeneracionXNegativaZ, -posicionFijaZ - escala * size.z, calleNegativaZ, -1);
        }

        DestruirObjetosAntiguos();
    }

    void GenerarObjetoEnCalle(ref float ultimaPosicionX, float posicionZ, List<GameObject> calle, int mirror)
    {
        if (buildingPrefab == null) return;

        Debug.Log("SIZE: " + size.ToString());

        float posX = ultimaPosicionX + escala * size.x;
        float posY = 0;
        Vector3 posicionObjeto = new Vector3(posX, posY, posicionZ);
        GameObject nuevoObjeto = Instantiate(buildingPrefab, posicionObjeto, Quaternion.identity);
        Rigidbody rb = nuevoObjeto.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = nuevoObjeto.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        ultimaPosicionX = posX;
        nuevoObjeto.transform.localScale = new Vector3(escala, escala, escala * mirror);
        calle.Add(nuevoObjeto);
    }

    void DestruirObjetosAntiguos()
    {
        for (int i = calleNegativaZ.Count - 1; i >= 0; i--)
        {
            if (calleNegativaZ[i].transform.position.x < liegreCam.position.x - escala * size.z * 2)
            {
                Destroy(calleNegativaZ[i]);
                calleNegativaZ.RemoveAt(i);
            }
        }
        for (int i = callePositivaZ.Count - 1; i >= 0; i--)
        {
            if (callePositivaZ[i].transform.position.x < liegreCam.position.x - escala * size.z * 2)
            {
                Destroy(callePositivaZ[i]);
                callePositivaZ.RemoveAt(i);
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