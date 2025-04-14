using UnityEngine;

public class DecalCollision : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Etiqueta del objeto jugador")]
    public string playerTag = "Player";

    [Tooltip("Nombre de la propiedad booleana que indica si está escarbando")]
    public string diggingPropertyName = "escarbando";

    private BoxCollider decalCollider;
    private GameObject currentPlayer;
    private bool playerIsDigging = false;

    private void Awake()
    {
        decalCollider = GetComponent<BoxCollider>();
        decalCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            currentPlayer = other.gameObject;
            CheckDiggingStatus();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            CheckDiggingStatus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            currentPlayer = null;
            playerIsDigging = false;
        }
    }

    private void CheckDiggingStatus()
    {
        var playerController = currentPlayer.GetComponent<PlayerControllerPlaya>();
        if (playerController != null)
        {
            bool newDiggingStatus = playerController.escarbando;

            if (newDiggingStatus && !playerIsDigging)
            {
                OnPlayerDiggingInDecal();
            }

            playerIsDigging = newDiggingStatus;
        }
    }

    // Método que se ejecuta cuando el jugador está escarbando dentro del decal
    private void OnPlayerDiggingInDecal()
    {
        Debug.Log("¡Jugador está escarbando dentro del área del decal!");

    }


}