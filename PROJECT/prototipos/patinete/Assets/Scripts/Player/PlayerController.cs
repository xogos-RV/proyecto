using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float runningSpeed;
    public float gravity = -9.81f;
    PlayerInput playerInput;
    CharacterController characterController;

    Animator animator;
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        MoveAndRotate();
        SetAnimation();
    }

    private void SetAnimation()
    {

        float targetSpeed = (playerInput.movement != Vector2.zero) ?
        (playerInput.isRunning ? 1 : 0.5f) : 0;
        animator.SetFloat("Blend", targetSpeed, 0.15f, Time.deltaTime); // FIX no me deja cambiar el monbre del parametro en el blend tree

    }

    private void MoveAndRotate()
    {
        Vector3 movement = new Vector3(playerInput.movement.x, 0, playerInput.movement.y);

        float speed = playerInput.isRunning ? runningSpeed : moveSpeed;

        characterController.Move(movement * speed * Time.deltaTime);

        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.up * gravity * Time.deltaTime);
        }

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
        }
    }
}
