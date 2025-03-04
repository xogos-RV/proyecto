using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorBola : MonoBehaviour
{
    private Rigidbody rb;
    PlayerInput playerInput;
    Vector2 joystick;
    public float maxVelocity = 100f;
    public float rotationSpeed = 100f;
    public float maxRotationAngle = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.right * maxVelocity;
        playerInput = GetComponent<PlayerInput>();
        Vector3 localScale = transform.localScale;
        Debug.Log("Escala local bola" + localScale.x);
        Vector3 globalScale = transform.lossyScale;
        Debug.Log("Escala global bola" + globalScale.x);
    }

    void Update()
    {
        ApplyRotation();
        MantenerVelocidadConstante();
    }

    void MantenerVelocidadConstante()
    {
        Vector3 velocidadActual = rb.linearVelocity;
        float diferenciaVelocidad = maxVelocity - velocidadActual.magnitude;
        if (diferenciaVelocidad > 0)
        {
            Vector3 fuerza = transform.right * diferenciaVelocidad * rb.mass;
            rb.AddForce(fuerza, ForceMode.Force);
        }
    }


    private void ApplyRotation()
    {
        joystick = playerInput.actions["Move"].ReadValue<Vector2>();
        float horizontalInput = joystick.x;
        float targetRotationY = horizontalInput * maxRotationAngle;
        Quaternion currentRotation = rb.rotation;
        Quaternion targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, targetRotationY, currentRotation.eulerAngles.z);
        rb.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        if (joystick.x == 0)
        {
            Quaternion originalRotation = Quaternion.Euler(90f, 0, currentRotation.eulerAngles.z);
            rb.rotation = Quaternion.RotateTowards(currentRotation, originalRotation, rotationSpeed * Time.deltaTime);

        }
    }


}