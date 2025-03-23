using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JugadorController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 joystick;
    private float breakInput;
    private float accelerateInput;
    private float jumpInput;
    private bool isGrounded = false;

    [Header("Configuración de Velocidad")]
    [Tooltip("Velocidad máxima que puede alcanzar el objeto")]
    [Range(0f, 100f)]
    public float maxVelocity = 60f;
    [Tooltip("Fuerza de aceleración aplicada para alcanzar la velocidad objetivo")]
    [Range(0f, 10f)]
    public float accelerationForce = 5f;

    [Header("Configuración de Rozamiento")]
    [Tooltip("Coeficiente de rozamiento: 0 = sin rozamiento (desliza indefinidamente), 1 = rozamiento máximo (se detiene rápido)")]
    [Range(0f, 1f)]
    public float frictionCoefficient = 0.1f;

    [Header("Configuración de Gatillos")]
    [Tooltip("Linealidad de los gatillos: -1 = más respuesta al inicio, 0 = lineal, 1 = más respuesta al final")]
    [Range(-1f, 2f)]
    public float triggerResponseCurve = 2f;

    [Tooltip("Factor multiplicador para la fuerza de frenado")]
    [Range(1f, 5f)]
    public float breakFactor = 2f;

    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad a la que rota el objeto")]
    [Range(0f, 200f)]
    public float rotationSpeed = 100f;

    [Tooltip("Ángulo máximo de rotación en el eje Z")]
    [Range(0f, 90f)]
    public float ZmaxRotationAngle = 15f;

    [Tooltip("Ángulo máximo de rotación en el eje Y")]
    [Range(0f, 90f)]
    public float YmaxRotationAngle = 10f;

    [Tooltip("Ángulo máximo de rotación en el eje X")]
    [Range(0f, 90f)]
    public float XmaxRotationAngle = 60f;

    [Header("Configuración de Fuerzas")]
    [Tooltip("Multiplicador para la fuerza lateral aplicada con el joystick")]
    [Range(0f, 5f)]
    public float lateralForceMultiplier = 5f;

    [Header("Threshold-y Reset")]
    [Tooltip("Umbral del eje Y por debajo del cual se resetea la posición")]
    [Range(-10f, 0f)]
    public float ejeYThreshold = -5f;
    [Tooltip("Altura Y a la que se reposiciona el objeto al resetearse")]
    [Range(0f, 20f)]
    public float initAt = 10f;

    [Header("Configuración de Frenado")]
    [Tooltip("Fuerza de amortiguación aplicada al eje X cuando el joystick está centrado")]
    [Range(0f, 10f)]
    public float xAxisDampingForce = 2.5f;

    [Header("Configuración de Colisión")]
    [Tooltip("Tag de los objetos con los que se puede colisionar")]
    public string collisionTab = "DynamicPrefab";

    [Tooltip("Tag del suelo para detectar cuando está en contacto con el terreno")]
    public string floorTag = "FloorPrefab";

    [Header("Detección de Suelo")]
    [Tooltip("Tag de los objetos con los que se puede colisionar")]
    [Range(1, 60)]
    public int frameThreshold = 60;

    [Header("Fuerza de Colisión")]
    [Tooltip("Magnitud de la fuerza aplicada al colisionar con objetos")]
    [Range(0f, 100f)]
    public float collisionForceMagnitude = 10f;

    [Tooltip("Magnitud del torque aleatorio aplicado al colisionar")]
    [Range(0f, 100f)]
    public float collisionTorqueMagnitude = 25f;

    [Tooltip("Umbral de velocidad relativa para activar el efecto de colisión (como porcentaje de maxVelocity)")]
    [Range(0f, 1f)]
    public float crashThreshold = 0.25f;

    [Header("Configuración de Freno Kinematic")]
    [Tooltip("Umbral de velocidad por debajo del cual el objeto se vuelve kinematic al frenar")]
    [Range(0f, 1f)]
    public float velocityThresholdForKinematic = 0.1f;

    [Header("UI Debug")]
    [Tooltip("Referencia al TextMeshPro para mostrar las fuerzas")]
    public TextMeshProUGUI forcesDebugText;

    [Tooltip("Referencia al TextMeshPro para mostrar los estados")]
    public TextMeshProUGUI statesDebugText;

    [Tooltip("Referencia al TextMeshPro para mostrar el estado final")]
    public TextMeshProUGUI finalStateDebugText;

    [Tooltip("Activar/desactivar la visualización de debug en UI")]
    public bool showDebugUI = true;

    [Tooltip("Frecuencia de actualización de la UI en segundos")]
    [Range(0.1f, 1.0f)]
    public float uiUpdateInterval = 0.1f;

    private float lastUIUpdateTime = 0f;


    private int framesWithoutGroundContact = 0;

    private bool isInContactWithGround = false;

    private bool isCollisionEffectActive = false;


    private Vector3 totalForce = Vector3.zero;

    private Quaternion targetRotation = Quaternion.identity;

    private bool shouldBeKinematic = false;

    private RigidbodyConstraints targetConstraints = RigidbodyConstraints.None;

    private Vector3 newVelocity;

    private Vector3 newPosition;

    private bool resetPosition = false;

    private Dictionary<string, Vector3> debugForces = new Dictionary<string, Vector3>();

    private Dictionary<string, string> debugParameters = new Dictionary<string, string>();


    void Start()
    {
        InitializeComponents();
    }


    void FixedUpdate()
    {
        // Reiniciar acumuladores
        ResetAccumulators();

        // Leer entrada
        ReadInput();

        // Calcular todas las fuerzas y cambios
        CalculateKinematicState();
        CalculateRotation();
        UpdateGroundedState();

        if (isGrounded && !isCollisionEffectActive)
        {
            CalculateVelocityForces();
            CalculateXAxisDamping();
            CalculateLateralForces();
        }

        CalculatePositionReset();
        ApplyAccumulatedChanges();
        LogDebugInfo();
        if (showDebugUI && Time.time - lastUIUpdateTime > uiUpdateInterval)
        {
            UpdateDebugUI();
            lastUIUpdateTime = Time.time;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = false;

        if (collision.gameObject.CompareTag(floorTag))
        {
            isInContactWithGround = true;

            // Restaurar el comportamiento normal al tocar el suelo
            if (isCollisionEffectActive)
            {
                isCollisionEffectActive = false;
                SetFreezeRotation(true);
            }
        }
        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;

        // Verificar si la velocidad relativa es inferior al umbral de colisión
        if (relativeVelocityMagnitude < maxVelocity * crashThreshold)
        {
            return;
        }

        if (collision.gameObject.CompareTag(collisionTab))
        {
            // Calcular la dirección de la fuerza basada en el ángulo de la colisión
            Vector3 forceDirection = collision.contacts[0].normal;
            // mantener la fuerza hacia delante en el eje Z (antes era X)
            forceDirection.z = -forceDirection.z; // Cambiado de X a Z
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


    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }


    private void ReadInput()
    {
        joystick = playerInput.movement;
        breakInput = playerInput.breakButton;
        accelerateInput = playerInput.accelerate;
        jumpInput = playerInput.jump;
    }


    private void CalculateKinematicState()
    {
        bool newKinematicState = isGrounded && breakInput > 0.1f && rb.linearVelocity.magnitude < velocityThresholdForKinematic;
        shouldBeKinematic = newKinematicState;

        debugParameters["KinematicState"] = $"isKinematic={shouldBeKinematic}, isGrounded={isGrounded}, breakInput={breakInput:F2}, velocity={rb.linearVelocity.magnitude:F2}";
    }


    private void CalculateRotation()
    {
        if (isCollisionEffectActive)
        {
            targetConstraints = RigidbodyConstraints.None;
            debugParameters["Rotation"] = "CollisionEffect active - no constraints";
            return;
        }

        // Usar los gatillos para la rotación vertical independientemente del joystick
        float verticalInput = CalculateVerticalRotationInput();

        // Calcular el factor de velocidad para la rotación en X
        float velocityFactor = isGrounded ? Mathf.Clamp01(rb.linearVelocity.magnitude / maxVelocity) : 0f;

        // Calcular rotación objetivo
        float targetRotationZ = joystick.x * ZmaxRotationAngle * -1f;
        float targetRotationY = joystick.x * YmaxRotationAngle;
        float targetRotationX = verticalInput * XmaxRotationAngle * velocityFactor;

        // Determinar la rotación final basada en las entradas
        if (Mathf.Abs(joystick.x) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            // Resetear toda la rotación
            targetRotation = Quaternion.RotateTowards(rb.rotation, Quaternion.Euler(0, 0, 0), rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X={targetRotationX:F2}, Y={targetRotationY:F2}, Z={targetRotationZ:F2}";
        }
        else if (Mathf.Abs(joystick.x) < 0.1f && Mathf.Abs(verticalInput) >= 0.1f)
        {
            // Solo aplicar rotación en X cuando solo hay entrada vertical
            Quaternion xOnlyRotation = Quaternion.Euler(targetRotationX, 0, 0);
            targetRotation = Quaternion.RotateTowards(rb.rotation, xOnlyRotation, rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X={targetRotationX:F2}, Y=0, Z=0";
        }
        else if (Mathf.Abs(joystick.x) >= 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            // Solo aplicar rotación en Y y Z cuando solo hay entrada horizontal
            Quaternion yzOnlyRotation = Quaternion.Euler(0, targetRotationY, targetRotationZ);
            targetRotation = Quaternion.RotateTowards(rb.rotation, yzOnlyRotation, rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X=0, Y={targetRotationY:F2}, Z={targetRotationZ:F2}";
        }
        else
        {
            // Aplicar rotación completa cuando hay ambas entradas
            Quaternion fullTargetRotation = Quaternion.Euler(targetRotationX, targetRotationY, targetRotationZ);
            targetRotation = Quaternion.RotateTowards(rb.rotation, fullTargetRotation, rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X={targetRotationX:F2}, Y={targetRotationY:F2}, Z={targetRotationZ:F2}";
        }

        // Establecer restricciones de rotación
        targetConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }


    private float CalculateVerticalRotationInput()
    {
        // Usar la diferencia entre jump y freno para la rotación vertical
        float netTriggerInput = jumpInput - breakInput;
        return joystickResponseCurve(netTriggerInput) * -1;
    }


    private void CalculateVelocityForces()
    {
        float processedAccelerate = joystickResponseCurve(accelerateInput);
        Vector3 direccionActual = transform.forward;
        float velocidadActualEnDireccion = Vector3.Dot(rb.linearVelocity, direccionActual);
        float velocidadObjetivo = velocidadActualEnDireccion;

        // Calcular velocidad objetivo
        if (processedAccelerate > 0.1f)
        {
            velocidadObjetivo = processedAccelerate * maxVelocity;
            debugParameters["Velocity"] = $"Accelerating: target={velocidadObjetivo:F2}, current={velocidadActualEnDireccion:F2}";
        }
        else
        {
            // Calcular fuerza de rozamiento
            float frictionForce = rb.linearVelocity.z * frictionCoefficient * rb.mass;
            Vector3 frictionDirection = -rb.linearVelocity.normalized;
            Vector3 frictionVector = frictionDirection * frictionForce;

            totalForce += frictionVector;
            debugForces["Friction"] = frictionVector;
            debugParameters["Velocity"] = $"Friction: magnitude={frictionForce:F2}";
        }

        // Si se está frenando, reducir la velocidad objetivo
        if (breakInput > 0.1f)
        {
            velocidadObjetivo = 0;
            debugParameters["Velocity"] += ", Breaking active";
        }

        // Solo calcular fuerzas si no está en modo kinematic
        if (!shouldBeKinematic)
        {
            // Calcular la fuerza necesaria para alcanzar la velocidad objetivo
            float diferenciaVelocidad = velocidadObjetivo - velocidadActualEnDireccion;
            Vector3 accelForce = direccionActual * diferenciaVelocidad * rb.mass * accelerationForce;

            totalForce += accelForce;
            debugForces["Acceleration"] = accelForce;
        }
    }

    private void CalculateXAxisDamping()
    {
        if (Mathf.Abs(joystick.x) < 0.1f)
        {
            float currentXVelocity = rb.linearVelocity.x;
            if (Mathf.Abs(currentXVelocity) > 0.01f)
            {
                float dampingForce = -currentXVelocity * xAxisDampingForce * rb.mass;
                Vector3 dampingVector = new Vector3(dampingForce, 0, 0);

                totalForce += dampingVector;
                debugForces["XAxisDamping"] = dampingVector;

                if (Mathf.Abs(currentXVelocity) < 0.1f)
                {
                    newVelocity.x = 0;
                    debugParameters["XAxisDamping"] = "Zeroing X velocity";
                }
                else
                {
                    debugParameters["XAxisDamping"] = $"Damping X: force={dampingForce:F2}, velocity={currentXVelocity:F2}";
                }
            }
        }
    }


    private void CalculateLateralForces()
    {
        // Normaliza la velocidad en Z (0 a 1)
        float normalizedZVelocity = Mathf.Clamp01(Mathf.Abs(rb.linearVelocity.z) / maxVelocity);

        // Función de campana: f(x) = 4x(1-x) que da 0 en x=0, 1 en x=0.5, y 0 en x=1
        // float bellCurveResponse = 4f * normalizedZVelocity * (1f - normalizedZVelocity);
        // Función de campana: comienza en 0, pico en 1, termina en 0.2
        // función cuadrática ajustada
        float bellCurveResponse = 0.2f + 0.8f * (1f - Mathf.Pow(2f * normalizedZVelocity - 1f, 2));

        if (Mathf.Abs(joystick.x) > 0.1f)
        {
            Vector3 lateralDirection = transform.right.normalized;

            // Usa la respuesta de campana en lugar del factor lineal
            float lateralForce = joystick.x * maxVelocity * rb.mass * lateralForceMultiplier * bellCurveResponse;
            Vector3 lateralVector = lateralDirection * lateralForce;

            totalForce += lateralVector;
            debugForces["LateralForce"] = lateralVector;
            debugParameters["LateralForce"] = $"Lateral: input={joystick.x:F2}, force={lateralForce:F2}, " +
                $"zVelocity={normalizedZVelocity:F2}, bellResponse={bellCurveResponse:F2}";
        }
        else
        {
            debugParameters["LateralForce"] = "No lateral force (no input)";
        }
    }

    private void CalculatePositionReset()
    {
        if (transform.position.y < ejeYThreshold)
        {
            resetPosition = true;
            shouldBeKinematic = false;
            newPosition = new Vector3(0, initAt, transform.position.z);
            newVelocity = Mathf.Abs(rb.linearVelocity.z) * Vector3.forward;
            targetRotation = Quaternion.Euler(0, 0, 0);

            debugParameters["PositionReset"] = $"Resetting position to Y={initAt:F2}, Z={transform.position.z:F2}";
        }
    }


    private void ApplyAccumulatedChanges()
    {
        // Aplicar posición si es necesario
        if (resetPosition)
        {
            transform.position = newPosition;
        }
        // Aplicar estado kinematic
        rb.isKinematic = shouldBeKinematic;
        // Aplicar restricciones
        rb.constraints = targetConstraints;
        // Aplicar rotación
        rb.rotation = targetRotation;
        // Aplicar velocidad si se ha modificado directamente
        if (newVelocity != rb.linearVelocity)
        {
            rb.linearVelocity = newVelocity;
        }
        // Aplicar la fuerza total acumulada
        if (totalForce.magnitude > 0 && !rb.isKinematic)
        {
            rb.AddForce(totalForce, ForceMode.Force);
        }
    }


    private void LogDebugInfo()
    {
        string forceLog = "Applied Forces:\n";
        foreach (var force in debugForces)
        {
            forceLog += $"- {force.Key}: {force.Value.magnitude:F2}N ({force.Value})\n";
        }
        forceLog += $"- TOTAL: {totalForce.magnitude:F2}N ({totalForce})\n";

        string paramLog = "Parameters:\n";
        foreach (var param in debugParameters)
        {
            paramLog += $"- {param.Key}: {param.Value}\n";
        }

        string finalState = $"Final State:\n" +
                           $"- Position: {transform.position}\n" +
                           $"- Rotation: {rb.rotation.eulerAngles}\n" +
                           $"- Velocity: {rb.linearVelocity} (mag: {rb.linearVelocity.magnitude:F2})\n" +
                           $"- IsKinematic: {rb.isKinematic}\n" +
                           $"- Constraints: {rb.constraints}\n";

        //Debug.Log($"--- PHYSICS UPDATE ---\n{forceLog}\n{paramLog}\n{finalState}");
    }


    private void UpdateGroundedState()
    {
        if (isInContactWithGround)
        {
            // Si hay contacto con el suelo, resetear el contador y establecer isGrounded a true
            framesWithoutGroundContact = 0;
            isGrounded = true;
            debugParameters["GroundState"] = "Grounded";
        }
        else
        {
            // Si no hay contacto, incrementar el contador
            framesWithoutGroundContact++;

            // Solo establecer isGrounded a false si han pasado suficientes frames sin contacto
            if (framesWithoutGroundContact >= frameThreshold)
            {
                isGrounded = false;
                debugParameters["GroundState"] = $"Airborne for {framesWithoutGroundContact} frames";
            }
            else
            {
                debugParameters["GroundState"] = $"Still grounded (buffer: {framesWithoutGroundContact}/{frameThreshold})";
            }
        }

        // Resetear la bandera de contacto para el próximo frame
        isInContactWithGround = false;
    }



    private IEnumerator RestoreRotationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetFreezeRotation(true);
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


    private void UpdateDebugUI()
    {
        if (forcesDebugText != null)
        {
            // Lista de todas las posibles fuerzas que queremos mostrar siempre
            string[] allPossibleForces = new string[]
            {
            "Acceleration",
            "Friction",
            "XAxisDamping",
            "LateralForce",
            "Collision"
            };

            string forceText = "<b>FUERZAS:</b>\n";

            // Mostrar todas las fuerzas posibles, con valores o placeholders
            foreach (string forceType in allPossibleForces)
            {
                if (debugForces.ContainsKey(forceType))
                {
                    Vector3 force = debugForces[forceType];

                    // Si la magnitud es prácticamente cero, mostrar un formato especial
                    if (force.magnitude < 0.001f)
                    {
                        forceText += $"• <color=#FFD700>{forceType}</color>: <color=#888888>0.00N</color>\n";
                        forceText += $"  <size=80%><color=#888888>(0.00, 0.00, 0.00)</color></size>\n";
                    }
                    else
                    {
                        forceText += $"• <color=#FFD700>{forceType}</color>: {force.magnitude:F2}N\n";
                        forceText += $"  <size=80%>({force.x:F2}, {force.y:F2}, {force.z:F2})</size>\n";
                    }
                }
                else
                {
                    forceText += $"• <color=#FFD700>{forceType}</color>: <color=#888888>------</color>\n";
                    forceText += $"  <size=80%><color=#888888>(------, ------, ------)</color></size>\n";
                }
            }

            // Mostrar cualquier otra fuerza no listada explícitamente
            foreach (var force in debugForces)
            {
                if (!Array.Exists(allPossibleForces, element => element == force.Key))
                {
                    forceText += $"• <color=#FFD700>{force.Key}</color>: {force.Value.magnitude:F2}N\n";
                    forceText += $"  <size=80%>({force.Value.x:F2}, {force.Value.y:F2}, {force.Value.z:F2})</size>\n";
                }
            }

            forceText += $"\n<b>TOTAL:</b> {totalForce.magnitude:F2}N\n";
            forceText += $"<size=80%>({totalForce.x:F2}, {totalForce.y:F2}, {totalForce.z:F2})</size>";

            forcesDebugText.text = forceText;
        }

        if (statesDebugText != null)
        {
            // Lista de todos los posibles parámetros que queremos mostrar siempre
            string[] allPossibleParams = new string[]
            {
            "KinematicState",
            "Rotation",
            "Velocity",
            "XAxisDamping",
            "LateralForce",
            "GroundState",
            "PositionReset"
            };

            string paramText = "<b>PARÁMETROS:</b>\n";

            // Mostrar todos los parámetros posibles, con valores o placeholders
            foreach (string paramType in allPossibleParams)
            {
                if (debugParameters.ContainsKey(paramType))
                {
                    paramText += $"• <color=#00BFFF>{paramType}</color>: {debugParameters[paramType]}\n";
                }
                else
                {
                    paramText += $"• <color=#00BFFF>{paramType}</color>: ------\n";
                }
            }



            // Mostrar cualquier otro parámetro no listado explícitamente
            foreach (var param in debugParameters)
            {
                if (!Array.Exists(allPossibleParams, element => element == param.Key))
                {
                    paramText += $"• <color=#00BFFF>{param.Key}</color>: {param.Value}\n";
                }
            }



            statesDebugText.text = paramText;
        }

        if (finalStateDebugText != null)
        {
            string finalStateText = "<b>ESTADO:</b>\n";
            finalStateText += $"• <color=#32CD32>Posición</color>: ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2})\n";
            finalStateText += $"• <color=#32CD32>Rotación</color>: ({rb.rotation.eulerAngles.x:F1}, {rb.rotation.eulerAngles.y:F1}, {rb.rotation.eulerAngles.z:F1})\n";
            finalStateText += $"• <color=#32CD32>Velocidad</color>: {rb.linearVelocity.magnitude:F2} m/s\n";
            finalStateText += $"  <size=80%>({rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2}, {rb.linearVelocity.z:F2})</size>\n";
            finalStateText += $"• <color=#32CD32>Kinematic</color>: {(rb.isKinematic ? "<color=yellow>Sí</color>" : "No")}\n";
            finalStateText += $"• <color=#32CD32>En suelo</color>: {(isGrounded ? "<color=green>Sí</color>" : "<color=red>No</color>")}\n";
            finalStateText += $"• <color=#32CD32>Colisión activa</color>: {(isCollisionEffectActive ? "<color=red>Sí</color>" : "No")}\n";

            finalStateDebugText.text = finalStateText;
        }
    }


    private void ResetAccumulators()
    {
        totalForce = Vector3.zero;
        targetRotation = rb.rotation;
        shouldBeKinematic = rb.isKinematic;
        targetConstraints = rb.constraints;
        newVelocity = rb.linearVelocity;
        resetPosition = false;

        // En lugar de limpiar completamente los diccionarios, establece valores vacíos
        // para mantener las claves consistentes
        string[] allPossibleForces = new string[]
        {
        "Acceleration",
        "Friction",
        "XAxisDamping",
        "LateralForce",
        "Collision"
        };

        string[] allPossibleParams = new string[]
        {
        "KinematicState",
        "Rotation",
        "Velocity",
        "XAxisDamping",
        "LateralForce",
        "GroundState",
        "PositionReset"
        };

        foreach (string force in allPossibleForces)
        {
            debugForces[force] = Vector3.zero;
        }

        foreach (string param in allPossibleParams)
        {
            debugParameters[param] = "------";
        }
    }

    private float joystickResponseCurve(float input)
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


}