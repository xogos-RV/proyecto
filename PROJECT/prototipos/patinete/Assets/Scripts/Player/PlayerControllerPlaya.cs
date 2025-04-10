using System;
using UnityEngine;

public class PlayerControllerPlaya : MonoBehaviour
{
    public Animator animator;
    CharacterController CC;
    PlayerInput PI;
    public float runningSpeed;
    public float moveSpeed;
    public float gravity = 9.81f;
    public float rotateDump;
    [Range(1, 5)] public float jumpHeight = 1f;
    [Range(0, 2)] public float minAirDistance = 0.5f;
    [Range(0, 1f)] public float thresholdGrounded = 0.5f;
    private float normalHeight;
    private float verticalVelocity;
    private bool isJumping;
    private bool isGrounded;
    private float groundedTimer = 0f;

    void Start()
    {
        PI = gameObject.GetComponent<PlayerInput>();
        CC = gameObject.GetComponent<CharacterController>();
        normalHeight = CC.height;
    }

    void Update()
    {
        CheckGrounded();

        // Resetear estados al tocar el suelo
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -0.5f; // Pequeña fuerza hacia abajo para asegurar contacto
            if (isJumping)
            {
                animator.SetTrigger("Aterrizaje"); // Disparar animación de aterrizaje
                isJumping = false;
            }
        }

        MoveRotate();
        HandleJump();
        SetAnimations();
    }

    private void CheckGrounded()
    {
        if (!CC.isGrounded)
        {
            groundedTimer += Time.deltaTime;
        }
        else
        {
            groundedTimer = 0f;
        }

        isGrounded = !(groundedTimer >= thresholdGrounded);
    }

    private void HandleJump()
    {
        if (PI.jump > 0 && !PI.escarbando)
        {
            isGrounded = false;
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * -gravity); // Fórmula física para salto
            animator.SetBool("Despegue", true);
            isJumping = true;
        }

        // Aplicar gravedad
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            animator.SetBool("Aire", true);
            animator.SetBool("Despegue", false);
        }
        else
        {
            animator.SetBool("Aire", false);
            /* Vector3 rayOrigin = transform.position + Vector3.up * 0.2f; // Offset para evitar el collider del jugador
            bool isNearGround = Physics.Raycast(rayOrigin, Vector3.down, minAirDistance + 0.2f, ~0);            
            Debug.DrawRay(rayOrigin, Vector3.down * (minAirDistance + 0.2f), Color.cyan); */
        }

        // Mover en Y
        CC.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void SetAnimations()
    {
        float targetSpeed = (PI.movement != Vector2.zero) ? (PI.isRunning ? 1 : 0.5f) : 0;
        animator.SetFloat("Movement", targetSpeed, 0.15f, Time.deltaTime);
        animator.SetBool("Escarbando", PI.escarbando);
        CC.height = PI.escarbando ? normalHeight / 2 : normalHeight;
    }

    private void MoveRotate()
    {
        Vector3 movement = CalculateMovementFromCamera();
        float speed = PI.isRunning ? runningSpeed : moveSpeed;
        speed = PI.escarbando ? moveSpeed * 0.5f : speed;

        CC.Move(movement * speed * Time.deltaTime);

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * rotateDump);
        }
    }

    private Vector3 CalculateMovementFromCamera()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        return forward * PI.movement.y + right * PI.movement.x;
    }
}