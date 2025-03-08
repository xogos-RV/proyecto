using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorBola : MonoBehaviour
{
    private Rigidbody rb;
    PlayerInput playerInput;
    Vector2 joystick;

    public float maxVelocity = 100f;
    public float rotationSpeed = 100f;
    public float maxRotationAngleY = 0f;
    public float maxRotationAngleX = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.right * maxVelocity;
        playerInput = GetComponent<PlayerInput>();

        Vector3 localScale = transform.localScale;
        Debug.Log("Escala local bola: " + localScale.x);
        Vector3 globalScale = transform.lossyScale;
        Debug.Log("Escala global bola: " + globalScale.x);
    }

    void Update()
    {
        ApplyRotation();
        MantenerVelocidadConstante();
    }

    void MantenerVelocidadConstante()
    {
        // Aplicamos la fuerza en la dirección actual del objeto
        Vector3 direccionActual = transform.right;
        float velocidadActualEnDireccion = Vector3.Dot(rb.linearVelocity, direccionActual);
        float diferenciaVelocidad = maxVelocity - velocidadActualEnDireccion;

        if (diferenciaVelocidad > 0)
        {
            Vector3 fuerza = direccionActual * diferenciaVelocidad * rb.mass;
            rb.AddForce(fuerza, ForceMode.Force);
        }
    }

    private void ApplyRotation()
    {
        joystick = playerInput.actions["Move"].ReadValue<Vector2>();

        // Rotación en Y (izquierda/derecha)
        float horizontalInput = joystick.x;
        float targetRotationX = horizontalInput * maxRotationAngleX;
        float targetRotationY = horizontalInput * maxRotationAngleY * -1f;

        // Obtenemos la rotación actual
        Quaternion currentRotation = rb.rotation;

        // Creamos la rotación objetivo manteniendo Z
        Quaternion targetRotation = Quaternion.Euler(
            targetRotationX,
            targetRotationY,
            currentRotation.eulerAngles.z
        );

        // Aplicamos la rotación gradualmente
        rb.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Si no hay input, volvemos a la rotación original
        if (joystick.magnitude < 0.1f)
        {
            Quaternion originalRotation = Quaternion.Euler(0, 0, currentRotation.eulerAngles.z);
            rb.rotation = Quaternion.RotateTowards(currentRotation, originalRotation, rotationSpeed * Time.deltaTime);
        }

        // Aplicamos impulso lateral basado en la rotación Y
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Calculamos la dirección lateral basada en la rotación
            Vector3 lateralDirection = transform.forward.normalized;
            float lateralForce = horizontalInput * maxVelocity * rb.mass * 0.2f; // Ajusta el 0.2f según necesites

            // Aplicamos la fuerza lateral
            rb.AddForce(lateralDirection * lateralForce, ForceMode.Force);
        }
    }
}