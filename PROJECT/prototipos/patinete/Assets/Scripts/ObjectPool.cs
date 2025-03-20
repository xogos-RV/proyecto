using UnityEngine;
using System.Collections.Generic;
public class ObjectPool : MonoBehaviour
{
    // Diccionario para almacenar los pools de cada tipo de objeto
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // Método para crear un nuevo pool para un tipo de objeto
    public void CreatePool(GameObject prefab, int poolSize)
    {
        string poolKey = prefab.name; // Usamos el nombre del prefab como clave

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary[poolKey] = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObj = Instantiate(prefab);
                newObj.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObj);
            }
        }
    }

    // Método para obtener un objeto del pool
    public GameObject GetObject(GameObject prefab)
    {
        string poolKey = prefab.name;

        if (poolDictionary.ContainsKey(poolKey))
        {
            if (poolDictionary[poolKey].Count > 0)
            {
                GameObject obj = poolDictionary[poolKey].Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Si no hay objetos disponibles, crear uno nuevo
                GameObject newObj = Instantiate(prefab);
                return newObj;
            }
        }
        else
        {
            Debug.LogWarning("No existe un pool para el prefab: " + poolKey);
            return null;
        }
    }

    // Método para devolver un objeto al pool
    public void ReturnObject(GameObject obj)
    {
        string poolKey = obj.name.Replace("(Clone)", ""); // Eliminar "(Clone)" del nombre

        if (poolDictionary.ContainsKey(poolKey))
        {
            obj.SetActive(false);
            poolDictionary[poolKey].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("No existe un pool para el objeto: " + poolKey);
        }
    }
}