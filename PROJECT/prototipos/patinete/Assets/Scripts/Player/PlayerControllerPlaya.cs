using UnityEngine;

public class PlayerControllerPlaya : MonoBehaviour
{
    public Animator animator;
    CharacterController CC;
    PlayerInput PI;
    public float runningSpeed;
    public float moveSpeed;
    public float gravity;
    public float rotateDump;
    private float normalHeight;

    void Start()
    {
        PI = gameObject.GetComponent<PlayerInput>();
        CC = gameObject.GetComponent<CharacterController>();
        normalHeight = CC.height;
    }

    void Update()
    {
        MoveRotate();
        SetAnimations();
    }

    private void SetAnimations()
    {
        float targetSpeed = (PI.movement != Vector2.zero) ? (PI.isRunning ? 1 : 0.5f) : 0;
        animator.SetFloat("Movement", targetSpeed, 0.15f, Time.deltaTime);
        animator.SetBool("Escarbando", PI.escarbando);
        
        // Ajustar altura del character controller
        CC.height = PI.escarbando ? normalHeight / 2 : normalHeight;
    }

    private void MoveRotate()
    {
        Vector3 movement = CalculateMovementFromCamera();
        float speed = PI.isRunning ? runningSpeed : moveSpeed;
        speed = PI.escarbando ? moveSpeed * 0.5f : speed;
        
        CC.Move(movement * speed * Time.deltaTime);
        
        if (!CC.isGrounded)
        {
            CC.Move(-Vector3.up * gravity * Time.deltaTime);
        }
        
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