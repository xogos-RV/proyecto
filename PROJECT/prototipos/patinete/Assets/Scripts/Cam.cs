using UnityEngine;

public class CameraFollowX : MonoBehaviour
{
    public Transform player; // Referencia al transform del jugador

    private Vector3 initialPosition; // Posición inicial de la cámara
    private Vector3 initialLookAtOffset; // Offset inicial para el LookAt

    void Start()
    {
        // Guardamos la posición inicial de la cámara
        initialPosition = transform.position;

        // Calculamos el offset inicial para el LookAt
        initialLookAtOffset = player.position - transform.position;
    }

    void LateUpdate()
    {
        // Calculamos la nueva posición de la cámara (solo en el eje X)
        Vector3 newPosition = new Vector3(
            player.position.x, // Seguimos al jugador en el eje X
            initialPosition.y, // Mantenemos la posición inicial en el eje Y
            initialPosition.z // Mantenemos la posición inicial en el eje Z
        );

        // Aplicamos la nueva posición a la cámara
        transform.position = newPosition;

        // Calculamos el punto de LookAt (solo en el eje X)
        Vector3 lookAtPosition = new Vector3(
            player.position.x + 5, // El LookAt sigue al jugador en el eje X
            transform.position.y + (initialLookAtOffset.y * 1.5f), // Mantenemos la altura inicial del LookAt
            transform.position.z + initialLookAtOffset.z // Mantenemos la distancia inicial del LookAt en Z
        );

        // Hacemos que la cámara mire hacia el punto calculado
        transform.LookAt(lookAtPosition);
    }
}