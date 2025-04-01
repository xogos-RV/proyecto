using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public GameObject patinete;
    public Transform camtrans;
    public float moveSpeed = 5f;
    public float runningSpeed = 16f;
    public float gravity = -9.81f;
    public float rotateDump = 5;
    PlayerInput playerInput;
    CharacterController characterController;

    public CinemachineCamera playerCamera;

    private bool isDriving = false;
    private bool push = false;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        SetDriving(true);
        patinete.GetComponent<FireController>().enabled = false;
    }

    void Update()
    {
        if (playerInput.drive > 0.1f && !push)
        {
            SetDriving(!isDriving);
            push = true;
        }
        else if (playerInput.drive < 0.1f)
        {
            push = false;
        }


        if (!isDriving)
        {
            MoveAndRotate();
            SetAnimation();
        }
        else
        {
            // Si está conduciendo, desactivar animaciones de caminar
            // animator.SetFloat("Movement", 0); TODO

            // Mantener al jugador en la posición relativa al patinete
            transform.position = patinete.transform.TransformPoint(new Vector3(0.89f, 0.86f, -1.88f));

            Quaternion rot = patinete.transform.rotation;
            rot *= Quaternion.Euler(new Vector3(19.6f, 311f, 0f)); // TODO coregir la postura en la animacion
            transform.GetChild(0).rotation = rot;

            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }



    private void SetAnimation()
    {
        float targetSpeed = (playerInput.movement != Vector2.zero) ? (playerInput.isRunning ? 1 : 0.5f) : 0;

        animator.SetFloat("Movement", targetSpeed, 0.15f, Time.deltaTime);
    }

    // Método para activar/desactivar la conducción
    public void SetDriving(bool driving)
    {
        isDriving = driving;
        animator.SetBool("Driving", driving);

        if (driving)
        {
            // Desactivar el CharacterController para evitar conflictos con el movimiento físico
            characterController.enabled = false;
            // Hacer que el jugador sea hijo del patinete para que siga su movimiento
            /* transform.parent = patinete.transform;
            // Posicionar al jugador en la posición relativa al patinete
            transform.localPosition = new Vector3(0, 0.54f, -1.82f);
            // Opcional: Rotar al jugador para que mire hacia adelante respecto al patinete
            transform.localRotation = Quaternion.identity; */
            if (patinete != null)
                patinete.GetComponent<PatineteController>().enabled = true;

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
        }
        else
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);

            if (patinete != null)
                patinete.GetComponent<PatineteController>().enabled = false;
            // Reactivar el CharacterController
            characterController.enabled = true;
            transform.GetChild(0).rotation = transform.rotation;
            // Separar al jugador del patinete
            /* transform.parent = null; */
        }
    }

    private void MoveAndRotate()
    {
        Vector3 movement = CalculateMovementoFromCam();
        float speed = playerInput.isRunning ? runningSpeed : moveSpeed;
        characterController.Move(movement * speed * Time.deltaTime);

        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.up * gravity * Time.deltaTime);
        }

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * rotateDump);
        }
    }

    private Vector3 CalculateMovementoFromCam()
    {
        Vector3 forward = camtrans.forward;
        Vector3 right = camtrans.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return forward * playerInput.movement.y + right * playerInput.movement.x;
    }
}