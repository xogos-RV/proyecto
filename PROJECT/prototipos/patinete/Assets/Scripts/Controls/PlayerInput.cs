using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    PlayerControls playerControls;
    public Vector2 movement { get; private set; }
    public Vector2 look { get; private set; }
    public float accelerate { get; private set; }
    public float jump { get; private set; }
    public float breakButton { get; private set; }
    public float fire { get; private set; }
    public float run { get; private set; }
    public bool isRunning { get; private set; }
    public float camL { get; private set; }
    public float camR { get; private set; }
    public float drive { get; private set; }
    public float JoisticSensitivity = 20f;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.player.run.performed += _ => isRunning = true;
        playerControls.player.run.canceled += _ => isRunning = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();

    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        movement = playerControls.player.Move.ReadValue<Vector2>();
        accelerate = playerControls.player.accelerate.ReadValue<float>();
        jump = playerControls.player.jump.ReadValue<float>();
        breakButton = playerControls.player.@break.ReadValue<float>();
        fire = playerControls.player.fire.ReadValue<float>();
        look = playerControls.player.look.ReadValue<Vector2>();
        InputDevice device = playerControls.player.look.activeControl?.device;
        if (device != null && device is Gamepad && (Math.Abs(look.x) > 0.1 || Math.Abs(look.y) > 0.1))
        {
            look *= JoisticSensitivity;

        }
        run = playerControls.player.run.ReadValue<float>();
        camL = playerControls.player.camL.ReadValue<float>();
        camR = playerControls.player.camR.ReadValue<float>();
        drive = playerControls.player.drive.ReadValue<float>();
    }
}