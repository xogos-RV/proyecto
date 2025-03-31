using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator;

    public Transform camtrans;
    public float moveSpeed = 5f;
    public float runningSpeed = 16f;
    public float gravity = -9.81f;
    public float rotateDump = 5;
    PlayerInput playerInput;
    CharacterController characterController;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
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
