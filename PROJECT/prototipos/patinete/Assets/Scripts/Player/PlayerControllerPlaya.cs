using System;
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

    void Start()
    {
        PI = gameObject.GetComponent<PlayerInput>();
        CC = gameObject.GetComponent<CharacterController>();
    }


    void Update()
    {
        MoveRotate();
        SetAnimation();
    }

    private void SetAnimation()
    {
        float targetSpeed = (PI.movement != Vector2.zero) ? (PI.isRunning ? 1 : 0.5f) : 0;
        animator.SetFloat("Movement", targetSpeed, 0f, Time.deltaTime);
    }


    private void MoveRotate()
    {
        Vector3 movement = new Vector3(-PI.movement.x, 0, -PI.movement.y);
        float speed = PI.isRunning ? runningSpeed : moveSpeed;
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
}
