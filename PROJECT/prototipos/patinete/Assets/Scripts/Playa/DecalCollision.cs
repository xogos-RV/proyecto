using UnityEngine;

public class DecalCollision : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Etiqueta del objeto jugador")]
    public string playerTag = "Player";

    private BoxCollider decalCollider;
    private GameObject currentPlayer;
    private bool playerIsDigging = false;
    private CarPatrolling carPatrolling;
    public AudioClip policeSound;
    private void Awake()
    {
        decalCollider = GetComponent<BoxCollider>();
        decalCollider.isTrigger = true;
        carPatrolling = GetComponentInParent<CarPatrolling>();
        policeSound = Resources.Load<AudioClip>("Audio/FX/Police");
        if (policeSound == null)
            Debug.LogError("No se encontró Police.mp3 en Resources/Audio/FX");
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
        if (carPatrolling != null)
        {
            carPatrolling.SetState(CarPatrolling.AgentState.Chasing);

            // añadir AudioSource
            AudioSource audioSource = carPatrolling.gameObject.AddComponent<AudioSource>();
            audioSource.clip = policeSound;
            audioSource.loop = true;
            audioSource.Play();

        }
    }

    public void EnableVision(bool value)
    {
        GameObject vision = GameObject.FindGameObjectWithTag("Decal");
        if (vision != null)
        {
            vision.SetActive(value);
        }
    }
}