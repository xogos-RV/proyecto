using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    PlayerControls playerControls;
    public Vector2 movement { get; private set; }
    public Vector2 look { get; private set; }

    public float accelerate { get; private set; }
    public float jump { get; private set; }
    public float breakButton { get; private set; }
    public float fire { get; private set; }
    private void Awake()
    {
        playerControls = new PlayerControls();
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
    }
}