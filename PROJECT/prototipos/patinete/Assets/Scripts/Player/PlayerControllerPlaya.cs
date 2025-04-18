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
    //[Range(0, 2)] public float minAirDistance = 0.5f;
    [Range(0, 1f)] public float thresholdGrounded = 0.3f;
    private float normalHeight;
    private float verticalVelocity;
    private float groundedTimer = 0f;
    private Vector3 totalMovement;
    private Vector3 airMovementDirection; // Nueva variable para almacenar la dirección del movimiento en el aire
    private bool keepAirMovement = false; // Controlar si debemos mantener el movimiento aéreo
    private bool isGrounded = true;
    private bool isLanding = false;
    public bool escarbando = false;

    // variables para el efecto de agua
    private bool isTouchingWater = false; // TODO estado nadando, animaciones 


    void Start()
    {
        PI = gameObject.GetComponent<PlayerInput>();
        CC = gameObject.GetComponent<CharacterController>();
        normalHeight = CC.height;
    }

    void Update()
    {
        totalMovement = Vector3.zero;
        ApplyGravity();
        CalculateMovementRotate();
        HandleJump();
        HandleWaterEffect();
        ApplyFinalMovement();
        SetAnimations();
        CheckGrounded();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // StartWaterEffect(other);
        }
    }

    // Mientras siga en el agua
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // Opcional: Recalcular dirección si el agua se mueve
            // waterPushDirection = (transform.position - other.transform.position).normalized;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            //  isTouchingWater = false;
            Debug.Log("¡Salió del agua!");
        }
    }

    public void StartLanding() //TODO  Desacoplar de Landing.cs
    {
        Debug.Log("StartLanding");
        isLanding = true;
    }

    public void EndLanding()
    {
        isLanding = false;
        Debug.Log("EndLanding");
    }


    private void ApplyGravity()
    {
        if (!CC.isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -0.5f;
        }
        totalMovement += Vector3.up * verticalVelocity * Time.deltaTime;
    }

    private void CalculateMovementRotate()
    {
        if (isLanding) return;

        Vector3 movement;

        if (isGrounded || !keepAirMovement)
        {
            movement = CalculateMovementFromCamera();
            airMovementDirection = movement.normalized;
        }
        else
        {
            movement = airMovementDirection * PI.movement.magnitude;
        }

        float speed = PI.isRunning ? runningSpeed : moveSpeed;
        speed = PI.escarbando ? moveSpeed * 0.5f : speed;

        totalMovement += movement * speed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * rotateDump);
        }
    }

    private void CheckGrounded()
    {
        if (!CC.isGrounded)
        {
            groundedTimer += Time.deltaTime;
            if (groundedTimer >= thresholdGrounded)
            {
                isGrounded = false;
                keepAirMovement = true;
            }
        }
        else
        {
            if (groundedTimer >= thresholdGrounded)
            {
                isLanding = true; // TODO borrar isLanding pasado un tiempo / separar logica del animator
            }
            isGrounded = true;
            groundedTimer = 0f;
            keepAirMovement = false;
        }
    }

    private void HandleJump()
    {
        if (PI.jump > 0 && !PI.escarbando && CC.isGrounded && !isLanding)
        {
            animator.SetTrigger("Jump");
            isGrounded = false;
            keepAirMovement = true;
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            totalMovement += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }

    // Nuevo método para iniciar el efecto de agua
    private void StartWaterEffect(Collider other)
    {
        // TODO  isTouchingWater = true;

        Debug.Log("¡-------------------------------- Entró en agua! ------------------------------------");
    }

    // Nuevo método para manejar el efecto de agua
    private void HandleWaterEffect()
    { // TODO
        if (!isTouchingWater) return;

    }


    private void SetAnimations()
    {
        float targetSpeed = 0;

        if (!isLanding || !isGrounded)
        {
            targetSpeed = (PI.movement != Vector2.zero) ? (PI.isRunning ? 1 : 0.5f) : 0;
        }

        animator.SetFloat("Movement", targetSpeed, 0.15f, Time.deltaTime);
        animator.SetBool("Escarbando", PI.escarbando);
        CC.height = !isGrounded ? normalHeight / 2 : normalHeight;
        CC.height = PI.escarbando ? normalHeight / 2 : normalHeight;
        animator.SetBool("isGrounded", isGrounded);
        escarbando = PI.escarbando; // TODO mover a una funcion: añadir un tiempo que debemos matener el boton para empezar a escarbar y un tiempo de relajacion
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

    private void ApplyFinalMovement()
    {
        CC.Move(totalMovement);
    }

    /*void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
        } */
}
