using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControladorBola : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 joystick;
    private float breakInput;
    private float accelerateInput;
    private bool isGrounded = false;

    [Header("Configuración de Velocidad")]
    [Range(0f, 100f)]
    public float maxVelocity = 60f;

    [Range(0f, 1f)]
    public float velocityControlMultiplier = 0.5f;

    [Header("Configuración de Gatillos")]
    [Tooltip("Linealidad de los gatillos: -1 = más respuesta al inicio, 0 = lineal, 1 = más respuesta al final")]
    [Range(-1f, 1f)]
    public float triggerResponseCurve = 0f;

    [Range(1f, 5f)]
    public float breakFactor = 2f;

    [Header("Configuración de Rotación")]
    [Range(0f, 200f)]
    public float rotationSpeed = 100f;

    [Range(0f, 90f)]
    public float maxRotationAngleX = 15f;

    [Range(0f, 90f)]
    public float maxRotationAngleY = 10f;

    [Range(0f, 90f)]
    public float maxRotationAngleZ = 60f;

    [Header("Configuración de Fuerzas")]
    [Range(0f, 1f)]
    public float lateralForceMultiplier = 0.2f;

    [Header("Threshold-y Reset")]
    [Range(-10f, 0f)]
    public float ejeY = -5f;

    [Range(0f, 20f)]
    public float initAt = 10f;

    [Header("Configuración de Frenado")]
    [Range(0f, 10f)]
    public float zAxisDampingForce = 2.5f;

    [Header("Configuración de Colisión")]
    public string collisionTab = "DynamicPrefab";
    public string floorTag = "FloorPrefab";

    [Header("Detección de Suelo")]
    [Range(1, 10)]
    public int frameThreshold = 3;

    private int framesWithoutGroundContact = 0;
    private bool isInContactWithGround = false;

    void Start()
    {
        InitializeComponents();
    }

    void Update()
    {
        ReadInput();
        ApplyRotation();

        UpdateGroundedState();

        if (isGrounded)
        {
            ControlVelocidad();
            StopZMovementWhenJoystickCentered();
        }

        doResetPositionY();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        rb.linearVelocity = transform.right * maxVelocity;
    }

    private void ReadInput()
    {
        joystick = playerInput.actions["Move"].ReadValue<Vector2>();
        breakInput = playerInput.actions["break"].ReadValue<float>();
        accelerateInput = playerInput.actions["caballito"].ReadValue<float>();
    }

    private void ControlVelocidad()
    {
        if (!isGrounded)
            return;

        float processedAccelerate = ApplyResponseCurve(accelerateInput);
        float processedBreak = ApplyResponseCurve(breakInput) * breakFactor;

        float netInput = processedAccelerate - processedBreak;

        Vector3 direccionActual = transform.right;
        float velocidadActualEnDireccion = Vector3.Dot(rb.linearVelocity, direccionActual);

        float velocidadObjetivo = CalcularVelocidadObjetivo(netInput);

        if (breakInput > 0.1f)
        {
            velocidadObjetivo = 0;
        }

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

        return velocidadObjetivo;
    }

    private void ApplyVelocityForce(Vector3 direccion, float velocidadActual, float velocidadObjetivo)
    {
        float diferenciaVelocidad = velocidadObjetivo - velocidadActual;
        Vector3 fuerza = direccion * diferenciaVelocidad * rb.mass;

        rb.AddForce(fuerza, ForceMode.Force);
    }

    private void ApplyRotation()
    {
        float horizontalInput = joystick.x * -1;

        // Usar los gatillos para la rotación vertical independientemente del joystick
        float verticalInput = CalculateVerticalRotationInput();

        // Calcular el factor de velocidad para la rotación en Z (0 cuando está parado, 1 cuando está a velocidad máxima)
        float velocityFactor = isGrounded ? Mathf.Clamp01(rb.linearVelocity.magnitude / maxVelocity) : 0f;

        Quaternion targetRotation = CalculateTargetRotation(horizontalInput, verticalInput, velocityFactor);
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

        if (isGrounded)
        {
            ApplyLateralForce(horizontalInput);
        }
    }

    private float CalculateVerticalRotationInput()
    {
        // Usar la diferencia entre aceleración y freno para la rotación vertical
        float netTriggerInput = accelerateInput - breakInput;
        return ApplyResponseCurve(netTriggerInput) * -1;
    }

    private Quaternion CalculateTargetRotation(float horizontalInput, float verticalInput, float velocityFactor)
    {
        float targetRotationX = horizontalInput * maxRotationAngleX;
        float targetRotationY = horizontalInput * maxRotationAngleY * -1f;
        // - Debug.Log($"[ControladorBola] Rotación objetivo: X={targetRotationX:F2}, Y={targetRotationY:F2}, Z={targetRotationZ:F2}");

        // Aplicar el factor de velocidad a la rotación en Z
        float targetRotationZ = verticalInput * maxRotationAngleZ * -1f * velocityFactor;

        return Quaternion.Euler(targetRotationX, targetRotationY, targetRotationZ);
    }

    private void ApplyRotationToRigidbody(Quaternion targetRotation)
    {
        Quaternion currentRotation = rb.rotation;
        rb.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ResetRotation()
    {
        Quaternion currentRotation = rb.rotation;
        Quaternion originalRotation = Quaternion.Euler(0, 0, 0);
        rb.rotation = Quaternion.RotateTowards(currentRotation, originalRotation, rotationSpeed * Time.deltaTime);
    }

    private void ResetHorizontalRotation()
    {
        // Mantener solo la rotación en Z (vertical) y resetear X e Y
        Vector3 currentEuler = rb.rotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(0, 0, currentEuler.z);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyLateralForce(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Vector3 lateralDirection = transform.forward.normalized;
            float lateralForce = horizontalInput * maxVelocity * rb.mass * lateralForceMultiplier;

            rb.AddForce(lateralDirection * lateralForce, ForceMode.Force);
        }
    }

    private void doResetPositionY()
    {
        if (transform.position.y < ejeY)
        {
            rb.linearVelocity = rb.linearVelocity.x * Vector3.right;
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

    void OnCollisionEnter(Collision collision)
    {
        // Verificar si está en contacto con el suelo
        if (collision.gameObject.CompareTag(floorTag))
        {
            isInContactWithGround = true;
        }

        if (collision.gameObject.CompareTag(collisionTab))
        {
            // Calcular la dirección de la fuerza basada en el ángulo de la colisión
            Vector3 forceDirection = collision.contacts[0].normal;
            float forceMagnitude = 10f; // Ajusta la magnitud de la fuerza según sea necesario

            // Aplicar la fuerza al objeto con el que colisionas
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.AddForce(-forceDirection * forceMagnitude, ForceMode.Impulse);
            }

            // Descongelar la rotación del objeto con el que colisionas
            SetFreezeRotation(false);

            // Iniciar una corrutina para restaurar la rotación después de 1 segundo
            StartCoroutine(RestoreRotationAfterDelay(otherRb, 1f));
        }
    }

    void OnCollisionExit(Collision collision)
    {

    }

    // Método para verificar si hay algún contacto con el suelo
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag(floorTag))
        {
            isInContactWithGround = true;
        }
    }

    private void SetFreezeRotation(bool freeze)
    {
        if (freeze)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.None;
        }
    }

    private IEnumerator RestoreRotationAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetFreezeRotation(true);
    }

    private void UpdateGroundedState()
    {
        if (isInContactWithGround)
        {
            // Si hay contacto con el suelo, resetear el contador y establecer isGrounded a true
            framesWithoutGroundContact = 0;
            isGrounded = true;
        }
        else
        {
            // Si no hay contacto, incrementar el contador
            framesWithoutGroundContact++;

            // Solo establecer isGrounded a false si han pasado suficientes frames sin contacto
            if (framesWithoutGroundContact >= frameThreshold)
            {
                isGrounded = false;
            }
        }

        // Resetear la bandera de contacto para el próximo frame
        isInContactWithGround = false;
    }
}