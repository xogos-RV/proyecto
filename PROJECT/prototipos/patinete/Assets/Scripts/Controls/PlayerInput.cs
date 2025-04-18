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
    public bool escarbando { get; private set; }
    public float JoisticSensitivity = 20f;

    // Variables para el doble clic en teclas de movimiento
    private float lastMovementPressTime;
    private float doubleClickTime = 0.3f;
    private bool forceRunning = false;
    private Vector2 lastMovementInput;

    private void Awake()
    {
        playerControls = new PlayerControls();
        
        // Configuración original del botón de correr
        playerControls.player.run.performed += _ => {
            if (!forceRunning) isRunning = true;
        };
        playerControls.player.run.canceled += _ => {
            if (!forceRunning) isRunning = false;
        };

        playerControls.player.drive.performed += _ => escarbando = true;
        playerControls.player.drive.canceled += _ => escarbando = false;
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
        CheckForDoubleClickRun();
    }

    private void CheckForDoubleClickRun()
    {
        Vector2 currentMovement = playerControls.player.Move.ReadValue<Vector2>();
        
        // Detectar cuando se presiona una tecla de movimiento
        if (currentMovement.magnitude > 0.1f && lastMovementInput.magnitude < 0.1f)
        {
            // Verificar si es un doble clic
            if (Time.time - lastMovementPressTime < doubleClickTime)
            {
                forceRunning = true;
                isRunning = true;
            }
            lastMovementPressTime = Time.time;
        }
        
        // Si estamos en forceRunning y se soltó el movimiento, desactivar
        if (forceRunning && currentMovement.magnitude < 0.1f)
        {
            forceRunning = false;
            isRunning = false;
        }

        lastMovementInput = currentMovement;
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