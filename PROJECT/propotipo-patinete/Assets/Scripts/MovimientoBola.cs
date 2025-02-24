using UnityEngine;

public class MovimientoBola : MonoBehaviour
{
    public float velocidad = 5f; // Velocidad de movimiento

    void Update()
    {
        // Mover la bola hacia adelante
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }
}