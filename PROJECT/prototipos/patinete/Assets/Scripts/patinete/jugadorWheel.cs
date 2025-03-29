using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class jugadorWheel : MonoBehaviour
{
    private PlayerInput playerInput;

    [Header("Referencias")]
    public Rigidbody rb;
    public WheelCollider ruedaDelantera;
    public WheelCollider ruedaTrasera;

    private Vector2 joystick;
    private float breakInput;
    private float accelerateInput;
    private float jumpInput;

    [Header("Configuración de Velocidad")]
    [Range(0f, 100f)]
    public float maxVelocity = 60f;
    [Range(0f, 1000000f)]
    public float accelerationForce = 1000000f;
    [Range(0f, 1000000f)]
    public float breakFactor = 2000000f;

    [Header("Configuración de Rozamiento")]
    [Range(0f, 1f)]
    public float frictionCoefficient = 0.1f;

    [Header("Configuración de Gatillos")]
    [Range(-1f, 2f)]
    public float triggerResponseCurve = 2f;


    [Header("Configuración de Rotación")]
    [Range(0f, 200f)]
    public float rotationSpeed = 100f;

    [Range(0f, 90f)]
    public float ZmaxRotationAngle = 15f;

    [Range(0f, 90f)]
    public float YmaxRotationAngle = 10f;

    [Range(0f, 90f)]
    public float XmaxRotationAngle = 60f;

    [Header("Configuración de Fuerzas")]
    [Range(0f, 5f)]
    public float lateralForceMultiplier = 5f;

    [Header("Threshold-y Reset")]
    [Range(-10f, 0f)]
    public float ejeYThreshold = -5f;
    [Range(0f, 20f)]
    public float initAt = 10f;

    [Header("Configuración de Frenado")]
    [Range(0f, 10f)]
    public float xAxisDampingForce = 2.5f;

    [Header("Configuración de Colisión")]
    public string collisionTab = "DynamicPrefab";
    public string floorTag = "Floor";

    [Header("Detección de Suelo")]
    [Range(1, 60)]
    public int frameThreshold = 60;

    [Header("Fuerza de Colisión")]
    [Range(0f, 100f)]
    public float collisionForceMagnitude = 10f;

    [Range(0f, 100f)]
    public float collisionTorqueMagnitude = 25f;

    [Range(0f, 1f)]
    public float crashThreshold = 0.25f;

    [Header("Configuración de Freno Kinematic")]
    [Range(0f, 1f)]
    public float velocityThresholdForKinematic = 0.1f;

    [Header("UI Debug")]
    public TextMeshProUGUI forcesDebugText;
    public TextMeshProUGUI statesDebugText;
    public TextMeshProUGUI finalStateDebugText;
    public bool showDebugUI = true;
    [Range(0.1f, 1.0f)]
    public float uiUpdateInterval = 0.1f;

    private bool isGrounded = false;
    private float lastUIUpdateTime = 0f;
    private int framesWithoutGroundContact = 0;
    private bool isCollisionEffectActive = false;
    private Quaternion targetRotation = Quaternion.identity;
    private bool shouldBeKinematic = false;
    private Vector3 newVelocity;
    private Vector3 newPosition;
    private bool resetPosition = false;
    private Dictionary<string, string> debugParameters = new Dictionary<string, string>();

    void Start()
    {
        InitializeComponents();
    }

    void FixedUpdate()
    {
        ResetAccumulators();
        ReadInput();
        CalculateKinematicState();
        CalculateRotation();
        UpdateGroundedState();

        if (isGrounded && !isCollisionEffectActive)
        {
            ApplyAccelerationAndBraking(); // Aplicar tracción y frenado
            ApplySteering(); // Aplicar dirección
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
        if (collision.gameObject.CompareTag(floorTag) && isCollisionEffectActive)
        {
            isCollisionEffectActive = false;
            SetFreezeRotation(true);

        }

        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;

        if (relativeVelocityMagnitude < maxVelocity * crashThreshold)
        {
            return;
        }

        if (collision.gameObject.CompareTag(collisionTab))
        {
            Vector3 forceDirection = collision.contacts[0].normal;
            forceDirection.z = -forceDirection.z;
            rb.AddForce(forceDirection * collisionForceMagnitude, ForceMode.Impulse);

            Vector3 randomTorque = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ) * collisionTorqueMagnitude;
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            isCollisionEffectActive = true;
            SetFreezeRotation(false);
            StartCoroutine(RestoreRotationAfterDelay(2f));
        }
    }

    void OnCollisionExit(Collision collision)
    {
    }

    void OnCollisionStay(Collision collision)
    {

    }

    private void InitializeComponents()
    {
        if (ruedaDelantera == null || ruedaTrasera == null || rb == null)
        {
            Debug.LogError("No se han asignado las ruedas en el Inspector.");
        }
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
            debugParameters["Rotation"] = "CollisionEffect active - no constraints";
            return;
        }

        float verticalInput = CalculateVerticalRotationInput();
        float velocityFactor = isGrounded ? Mathf.Clamp01(rb.linearVelocity.magnitude / maxVelocity) : 0f;

        float targetRotationZ = joystick.x * ZmaxRotationAngle * -1f;
        float targetRotationY = joystick.x * YmaxRotationAngle;
        float targetRotationX = verticalInput * XmaxRotationAngle * velocityFactor;

        if (Mathf.Abs(joystick.x) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            targetRotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 0), rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X={targetRotationX:F2}, Y={targetRotationY:F2}, Z={targetRotationZ:F2}";
        }
        else if (Mathf.Abs(joystick.x) < 0.1f && Mathf.Abs(verticalInput) >= 0.1f)
        {
            Quaternion xOnlyRotation = Quaternion.Euler(targetRotationX, 0, 0);
            targetRotation = Quaternion.RotateTowards(transform.rotation, xOnlyRotation, rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X={targetRotationX:F2}, Y=0, Z=0";
        }
        else if (Mathf.Abs(joystick.x) >= 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            Quaternion yzOnlyRotation = Quaternion.Euler(0, targetRotationY, targetRotationZ);
            targetRotation = Quaternion.RotateTowards(transform.rotation, yzOnlyRotation, rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X=0, Y={targetRotationY:F2}, Z={targetRotationZ:F2}";
        }
        else
        {
            Quaternion fullTargetRotation = Quaternion.Euler(targetRotationX, targetRotationY, targetRotationZ);
            targetRotation = Quaternion.RotateTowards(transform.rotation, fullTargetRotation, rotationSpeed * Time.deltaTime);
            debugParameters["Rotation"] = $"Target rotation: X={targetRotationX:F2}, Y={targetRotationY:F2}, Z={targetRotationZ:F2}";
        }
    }

    private float CalculateVerticalRotationInput()
    {
        float netTriggerInput = jumpInput - breakInput;
        return ApplyResponseCurve(netTriggerInput) * -1;
    }

    private float ApplyResponseCurve(float input)
    {
        float normalizedInput = Mathf.Clamp01(Mathf.Abs(input));
        float processedInput;

        if (triggerResponseCurve > 0)
        {
            processedInput = Mathf.Pow(normalizedInput, 1f + triggerResponseCurve);
        }
        else if (triggerResponseCurve < 0)
        {
            processedInput = Mathf.Pow(normalizedInput, 1f / (1f + Mathf.Abs(triggerResponseCurve)));
        }
        else
        {
            processedInput = normalizedInput;
        }

        return Mathf.Sign(input) * processedInput;
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
        if (resetPosition)
        {
            transform.position = newPosition;
        }

        rb.isKinematic = shouldBeKinematic;
        transform.rotation = targetRotation;

        if (newVelocity != rb.linearVelocity)
        {
            rb.linearVelocity = newVelocity;
        }
    }

    private void ApplyAccelerationAndBraking()
    {
        float motorTorque = accelerateInput * accelerationForce;
        float brakeTorque = breakInput * breakFactor;
        //Debug.Log($"Motor Torque: {motorTorque:F2}, Brake Torque: {brakeTorque:F2}");
        ruedaDelantera.motorTorque = motorTorque;
        ruedaTrasera.motorTorque = motorTorque;
        ruedaDelantera.brakeTorque = brakeTorque;
        ruedaTrasera.brakeTorque = brakeTorque;
    }

    private void ApplySteering()
    {
        float steerAngle = joystick.x * YmaxRotationAngle;
        ruedaDelantera.steerAngle = steerAngle;
    }

    private void LogDebugInfo()
    {
        string paramLog = "Parameters:\n";
        foreach (var param in debugParameters)
        {
            paramLog += $"- {param.Key}: {param.Value}\n";
        }

        string finalState = $"Final State:\n" +
                           $"- Position: {transform.position}\n" +
                           $"- Rotation: {transform.rotation.eulerAngles}\n" +
                           $"- Velocity: {rb.linearVelocity} (mag: {rb.linearVelocity.magnitude:F2})\n" +
                           $"- IsKinematic: {rb.isKinematic}\n";

        //Debug.Log($"--- PHYSICS UPDATE ---\n{paramLog}\n{finalState}");
    }

    private void UpdateGroundedState()
    {
        WheelHit hit;
        bool ruedaDelanteraGrounded = ruedaDelantera.GetGroundHit(out hit);
        bool ruedaTraseraGrounded = ruedaTrasera.GetGroundHit(out hit);

        isGrounded = ruedaDelanteraGrounded || ruedaTraseraGrounded;

        if (isGrounded)
        {
            framesWithoutGroundContact = 0;
            debugParameters["GroundState"] = "Grounded";
        }
        else
        {
            framesWithoutGroundContact++;

            if (framesWithoutGroundContact >= frameThreshold)
            {
                debugParameters["GroundState"] = $"Airborne for {framesWithoutGroundContact} frames";
            }
            else
            {
                debugParameters["GroundState"] = $"Still grounded (buffer: {framesWithoutGroundContact}/{frameThreshold})";
            }
        }
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
        if (statesDebugText != null)
        {
            string[] allPossibleParams = new string[]
            {
                "KinematicState",
                "Rotation",
                "Velocity",
                "GroundState",
                "PositionReset"
            };

            string paramText = "<b>PARÁMETROS:</b>\n";

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
            finalStateText += $"• <color=#32CD32>Rotación</color>: ({transform.rotation.eulerAngles.x:F1}, {transform.rotation.eulerAngles.y:F1}, {transform.rotation.eulerAngles.z:F1})\n";
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
        targetRotation = transform.rotation;
        shouldBeKinematic = rb.isKinematic;
        newVelocity = rb.linearVelocity;
        resetPosition = false;

        string[] allPossibleParams = new string[]
        {
            "KinematicState",
            "Rotation",
            "Velocity",
            "GroundState",
            "PositionReset"
        };

        foreach (string param in allPossibleParams)
        {
            debugParameters[param] = "---";
        }
    }
}