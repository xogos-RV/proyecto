using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorBola : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 joystick;
    private float breakInput;
    private float accelerateInput;

    [Header("Configuración de Velocidad")]
    public float maxVelocity = 100f;
    public float velocityControlMultiplier = 0.5f;

    [Header("Configuración de Gatillos")]
    [Tooltip("Linealidad de los gatillos: -1 = más respuesta al inicio, 0 = lineal, 1 = más respuesta al final")]
    [Range(-1f, 1f)]
    public float triggerResponseCurve = 0f;


    [Header("Configuración de Rotación")]
    public float rotationSpeed = 100f;
    public float maxRotationAngleX = 15f;
    public float maxRotationAngleY = 10f;
    public float maxRotationAngleZ = 60f;

    [Header("Configuración de Fuerzas")]
    public float lateralForceMultiplier = 0.2f;

    [Header("Threshold-y Reset")]
    public float ejeY = -5f;
    public float initAt = 10f;

    [Header("Configuración de Frenado")]
    public float zAxisDampingForce = 2.5f;

    void Start()
    {
        InitializeComponents();
        LogInitialState();
    }

    void Update()
    {
        ReadInput();
        ApplyRotation();
        ControlVelocidad();
        doResetPositionY();
        StopZMovementWhenJoystickCentered();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        rb.linearVelocity = transform.right * maxVelocity;
    }

    private void LogInitialState()
    {
        Vector3 localScale = transform.localScale;
        Vector3 globalScale = transform.lossyScale;
        // - Debug.Log($"[ControladorBola] Escala local bola: {localScale.x}, {localScale.y}, {localScale.z}");
        // - Debug.Log($"[ControladorBola] Escala global bola: {globalScale.x}, {globalScale.y}, {globalScale.z}");
        // - Debug.Log($"[ControladorBola] Velocidad inicial: {rb.linearVelocity.magnitude}");
    }

    private void ReadInput()
    {
        joystick = playerInput.actions["Move"].ReadValue<Vector2>();
        breakInput = playerInput.actions["break"].ReadValue<float>();
        accelerateInput = playerInput.actions["caballito"].ReadValue<float>();

        // - Debug.Log($"[ControladorBola] Input joystick: X={joystick.x:F2}, Y={joystick.y:F2}");
        // - Debug.Log($"[ControladorBola] Freno: {breakInput:F2}, Acelerador: {accelerateInput:F2}");
    }

    private void ControlVelocidad()
    {
        // Procesar entradas de gatillos con la curva de respuesta
        float processedAccelerate = ApplyResponseCurve(accelerateInput);
        float processedBreak = ApplyResponseCurve(breakInput);

        // Calcular el input neto (aceleración - freno)
        float netInput = processedAccelerate - processedBreak;

        Vector3 direccionActual = transform.right;
        float velocidadActualEnDireccion = Vector3.Dot(rb.linearVelocity, direccionActual);

        float velocidadObjetivo = CalcularVelocidadObjetivo(netInput);

        ApplyVelocityForce(direccionActual, velocidadActualEnDireccion, velocidadObjetivo);
    }

    private float ApplyResponseCurve(float input)
    {

        // Normalizar el input al rango efectivo después del deadzone
        float normalizedInput = Mathf.Clamp01(Mathf.Abs(input));

        // Aplicar la curva de respuesta
        float processedInput;
        if (triggerResponseCurve > 0) // Más respuesta al final
        {
            processedInput = Mathf.Pow(normalizedInput, 1f + triggerResponseCurve);
        }
        else if (triggerResponseCurve < 0) // Más respuesta al inicio
        {
            processedInput = Mathf.Pow(normalizedInput, 1f / (1f + Mathf.Abs(triggerResponseCurve)));
        }
        else // Respuesta lineal
        {
            processedInput = normalizedInput;
        }

        // Mantener el signo original
        return Mathf.Sign(input) * processedInput;
    }

    private float CalcularVelocidadObjetivo(float input)
    {
        float velocidadObjetivo = maxVelocity;

        // Aplicar el efecto de los gatillos a la velocidad objetivo
        velocidadObjetivo += input * maxVelocity * velocityControlMultiplier;

        // - Debug.Log($"[ControladorBola] Input procesado: {input:F2}, Velocidad objetivo: {velocidadObjetivo:F2}");

        return velocidadObjetivo;
    }

    private void ApplyVelocityForce(Vector3 direccion, float velocidadActual, float velocidadObjetivo)
    {
        float diferenciaVelocidad = velocidadObjetivo - velocidadActual;
        Vector3 fuerza = direccion * diferenciaVelocidad * rb.mass;

        rb.AddForce(fuerza, ForceMode.Force);
        // - Debug.Log($"[ControladorBola] Velocidad actual: {velocidadActual:F2}, Objetivo: {velocidadObjetivo:F2}, Fuerza aplicada: {fuerza.magnitude:F2}");
    }

    private void ApplyRotation()
    {
        float horizontalInput = joystick.x * -1;

        // Usar los gatillos para la rotación vertical independientemente del joystick
        float verticalInput = CalculateVerticalRotationInput();

        Quaternion targetRotation = CalculateTargetRotation(horizontalInput, verticalInput);
        ApplyRotationToRigidbody(targetRotation);

        // Solo resetear la rotación horizontal cuando el joystick está centrado
        // pero mantener la rotación vertical si los gatillos están activos
        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            ResetRotation();
        }
        else if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            // Resetear solo la rotación horizontal
            ResetHorizontalRotation();
        }

        ApplyLateralForce(horizontalInput);
    }

    private float CalculateVerticalRotationInput()
    {
        // Usar la diferencia entre aceleración y freno para la rotación vertical
        float netTriggerInput = accelerateInput - breakInput;
        return ApplyResponseCurve(netTriggerInput) * -1;
    }

    private Quaternion CalculateTargetRotation(float horizontalInput, float verticalInput)
    {
        float targetRotationX = horizontalInput * maxRotationAngleX;
        float targetRotationY = horizontalInput * maxRotationAngleY * -1f;
        float targetRotationZ = verticalInput * maxRotationAngleZ * -1f;

        // - Debug.Log($"[ControladorBola] Rotación objetivo: X={targetRotationX:F2}, Y={targetRotationY:F2}, Z={targetRotationZ:F2}");

        return Quaternion.Euler(targetRotationX, targetRotationY, targetRotationZ);
    }

    private void ApplyRotationToRigidbody(Quaternion targetRotation)
    {
        Quaternion currentRotation = rb.rotation;
        rb.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

        // - Debug.Log($"[ControladorBola] Rotación actual: {rb.rotation.eulerAngles}");
    }

    private void ResetRotation()
    {
        Quaternion currentRotation = rb.rotation;
        Quaternion originalRotation = Quaternion.Euler(0, 0, 0);
        rb.rotation = Quaternion.RotateTowards(currentRotation, originalRotation, rotationSpeed * Time.deltaTime);

        // - Debug.Log("[ControladorBola] Reseteando rotación");
    }

    private void ResetHorizontalRotation()
    {
        // Mantener solo la rotación en Z (vertical) y resetear X e Y
        Vector3 currentEuler = rb.rotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(0, 0, currentEuler.z);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // - Debug.Log("[ControladorBola] Reseteando rotación horizontal");
    }

    private void ApplyLateralForce(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Vector3 lateralDirection = transform.forward.normalized;
            float lateralForce = horizontalInput * maxVelocity * rb.mass * lateralForceMultiplier;

            rb.AddForce(lateralDirection * lateralForce, ForceMode.Force);
            // - Debug.Log($"[ControladorBola] Fuerza lateral aplicada: {lateralForce:F2} en dirección {lateralDirection}");
        }
    }

    private void doResetPositionY()
    {
        if (transform.position.y < ejeY)
        {
            transform.position = new Vector3(transform.position.x, initAt, 0);
        }
    }

    private void StopZMovementWhenJoystickCentered()
    {
        if (Mathf.Abs(joystick.x) < 0.1f)
        {
            // Obtén la velocidad actual en Z
            float currentZVelocity = rb.linearVelocity.z;

            // Si hay alguna velocidad en Z, aplica una fuerza de frenado
            if (Mathf.Abs(currentZVelocity) > 0.01f)
            {
                // Calcula la fuerza necesaria para detener el movimiento en Z
                float dampingForce = -currentZVelocity * zAxisDampingForce * rb.mass;

                // Aplica la fuerza solo en el eje Z
                rb.AddForce(0, 0, dampingForce, ForceMode.Force);

                // Opcional: Si la velocidad es muy pequeña, detenla completamente para evitar deslizamientos
                if (Mathf.Abs(currentZVelocity) < 0.1f)
                {
                    Vector3 newVelocity = rb.linearVelocity;
                    newVelocity.z = 0;
                    rb.linearVelocity = newVelocity;
                }
            }
        }
    }
}