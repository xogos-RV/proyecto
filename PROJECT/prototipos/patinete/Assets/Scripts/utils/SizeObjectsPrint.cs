using UnityEngine;

public class MedirTamanos : MonoBehaviour
{
    void Start()
    {
        // Obtener todos los objetos activos en la escena
        GameObject[] todosLosObjetos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in todosLosObjetos)
        {
            MedirTamanio(obj);
        }
    }

    void MedirTamanio(GameObject obj)
    {
        // Verificar si el objeto tiene un Renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Obtener el tamaño del objeto
            Vector3 tamaño = renderer.bounds.size;
            Debug.Log("Nombre del objeto: " + obj.name + ", Tamaño: " + tamaño);
        }

        // Recorrer recursivamente los hijos del objeto
        foreach (Transform hijo in obj.transform)
        {
            MedirTamanio(hijo.gameObject);
        }
    }
}