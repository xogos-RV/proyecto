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
    private float jumpInput;
    private bool isGrounded = false;

    [Header("Configuración de Velocidad")]
    [Range(0f, 100f)]
    public float maxVelocity = 60f;

    [Range(0f, 10f)]
    public float accelerationForce = 5f;

    [Range(0f, 1f)]
    public float velocityControlMultiplier = 0.5f;

    [Header("Configuración de Rozamiento")]
    [Tooltip("Coeficiente de rozamiento: 0 = sin rozamiento (desliza indefinidamente), 1 = rozamiento máximo (se detiene rápido)")]
    [Range(0f, 1f)]
    public float frictionCoefficient = 0.1f;

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

    [Header("Fuerza de Colisión")]
    [Range(0f, 100f)]
    public float collisionForceMagnitude = 10f;

    [Range(0f, 100f)]
    public float collisionTorqueMagnitude = 25f;

    [Range(0f, 1f)]
    public float crashThreshold = 0.25f;
    private bool isCollisionEffectActive = false;

    void Start()
    {
        InitializeComponents();
    }

    void Update()
    {
        ReadInput();
        if (!isCollisionEffectActive)
        {
            ApplyRotation();

            UpdateGroundedState();

            if (isGrounded)
            {
                ControlVelocidad();
                StopZMovementWhenJoystickCentered();
            }
        }
        doResetPositionY();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void ReadInput()
    {
        joystick = playerInput.actions["Move"].ReadValue<Vector2>();
        breakInput = playerInput.actions["break"].ReadValue<float>();
        accelerateInput = playerInput.actions["accelerate"].ReadValue<float>();
        jumpInput = playerInput.actions["jump"].ReadValue<float>();
    }

    private void ControlVelocidad()
    {
        if (!isGrounded)
            return;

        float processedAccelerate = ApplyResponseCurve(accelerateInput);
        float processedJump = ApplyResponseCurve(jumpInput);

        // Calcular la velocidad actual en la dirección de avance
        Vector3 direccionActual = transform.right;
        float velocidadActualEnDireccion = Vector3.Dot(rb.linearVelocity, direccionActual);

        // Calcular la velocidad objetivo basada en el input de aceleración
        float velocidadObjetivo = velocidadActualEnDireccion;
        
        // Solo aplicar aceleración si se presiona el botón de aceleración
        if (processedAccelerate > 0.1f)
        {
            velocidadObjetivo = processedAccelerate * maxVelocity;
        }
        else
        {
            // Aplicar rozamiento cuando no se está acelerando
            ApplyFriction(direccionActual);
        }
        
        // Aplicar el efecto de jump a la velocidad objetivo
        velocidadObjetivo += processedJump * maxVelocity * velocityControlMultiplier;

        // Si se está frenando, reducir la velocidad objetivo
        if (breakInput > 0.1f)
        {
            velocidadObjetivo = 0;
        }

        // Aplicar la fuerza necesaria para alcanzar la velocidad objetivo
        ApplyVelocityForce(direccionActual, velocidadActualEnDireccion, velocidadObjetivo);
    }

    private void ApplyFriction(Vector3 direccionMovimiento)
    {
        // Solo aplicar rozamiento si hay velocidad
        if (rb.linearVelocity.magnitude > 0.01f)
        {
            // Calcular la fuerza de rozamiento proporcional a la velocidad actual
            float frictionForce = rb.linearVelocity.magnitude * frictionCoefficient * rb.mass;
            
            // Aplicar la fuerza en dirección opuesta al movimiento
            Vector3 frictionDirection = -rb.linearVelocity.normalized;
            rb.AddForce(frictionDirection * frictionForce, ForceMode.Force);
        }
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

    private void ApplyVelocityForce(Vector3 direccion, float velocidadActual, float velocidadObjetivo)
    {
        float diferenciaVelocidad = velocidadObjetivo - velocidadActual;
        Vector3 fuerza = direccion * diferenciaVelocidad * rb.mass * accelerationForce;

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
        // Usar la diferencia entre jump y freno para la rotación vertical
        float netTriggerInput = jumpInput - breakInput;
        return ApplyResponseCurve(netTriggerInput) * -1;
    }

    private Quaternion CalculateTargetRotation(float horizontalInput, float verticalInput, float velocityFactor)
    {
        float targetRotationX = horizontalInput * maxRotationAngleX;
        float targetRotationY = horizontalInput * maxRotationAngleY * -1f;

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
            rb.linearVelocity = Mathf.Abs(rb.linearVelocity.x) * Vector3.right;
            transform.position = new Vector3(transform.position.x, initAt, 0);
            rb.rotation = Quaternion.Euler(0, 0, 0);
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
        // TODO SIMPLIFICAR LAS CASUISTICAS
        if (collision.gameObject.CompareTag(floorTag))
        {
            Debug.Log("FLOOR COLLISION ");
            isInContactWithGround = true;
            // Restaurar el comportamiento normal al tocar el suelo
            if (isCollisionEffectActive)
            {
                isCollisionEffectActive = false;
                SetFreezeRotation(true);
            }
        }
        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;

        // Verificar si la velocidad relativa supera el umbral
        if (relativeVelocityMagnitude < maxVelocity * crashThreshold)
        {   
            // TODO A VECES EL PATIN QUEDA TIRADO EN EL SUELO
            Debug.Log("CUBE COLLISION SKIP relativeVelocityMagnitude: " + relativeVelocityMagnitude + " --------- threshold: " + maxVelocity * crashThreshold);
            return;
        }

        if (collision.gameObject.CompareTag(collisionTab))
        {
            Debug.Log("------>  CUBE COLLISION ENTER relativeVelocityMagnitude: " + relativeVelocityMagnitude);
            // Calcular la dirección de la fuerza basada en el ángulo de la colisión
            Vector3 forceDirection = collision.contacts[0].normal;
            // mantener la fuerza hacia delante en el eje x
            forceDirection.x = -forceDirection.x;
            // Aplicar la fuerza al jugador
            rb.AddForce(forceDirection * collisionForceMagnitude, ForceMode.Impulse);
            // Aplicar un torque aleatorio para simular rotación
            Vector3 randomTorque = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ) * collisionTorqueMagnitude;
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            // Activar el efecto de colisión
            isCollisionEffectActive = true;
            SetFreezeRotation(false);

            // Iniciar una corrutina para restaurar la rotación después de 2 segundo
            StartCoroutine(RestoreRotationAfterDelay(2f));
        }
    }

    void OnCollisionExit(Collision collision)
    {

    }

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
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private IEnumerator RestoreRotationAfterDelay(float delay)
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