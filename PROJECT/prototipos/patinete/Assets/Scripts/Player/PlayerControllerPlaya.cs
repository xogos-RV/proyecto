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

        if (!isLanding || !isGrounded)
        {
            CalculateMovementRotate();
        }

        HandleJump();
        ApplyFinalMovement();
        SetAnimations();
        CheckGrounded();
        //  CheckLandingAnimation();
    }

    /*void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
     } */

    public void StartLanding()
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
        if (PI.jump > 0 && !PI.escarbando && isGrounded && !isLanding)
        {
            animator.SetTrigger("Jump");
            isGrounded = false;
            keepAirMovement = true;
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            totalMovement += Vector3.up * verticalVelocity * Time.deltaTime;
        }
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
        CC.height = PI.escarbando ? normalHeight / 2 : normalHeight;
        animator.SetBool("isGrounded", isGrounded);
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
}