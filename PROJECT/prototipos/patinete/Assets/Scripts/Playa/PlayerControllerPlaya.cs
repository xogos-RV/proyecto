using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControllerPlaya : MonoBehaviour
{
    public Animator animator;
    CharacterController CC;
    PlayerInput PI;
    ClickToMove clickToMove;
    public float runningSpeed;
    public float moveSpeed;
    public float gravity = 9.81f;
    public float rotateDump;
    [Range(1, 5)] public float jumpHeight = 1f;
    [Range(0, 1f)] public float thresholdGrounded = 0.3f;
    public bool escarbando = false;

    [Header("Knockback Settings")]
    public float knockbackDistance = 20f;
    public float knockbackMaxHeight = 1.5f;
    public float knockbackDuration = 1f;
    public float knockbackRotationSpeed = 10f;
    //public float maxSlopeAngle = 45f;

    private float normalHeight;
    private float verticalVelocity;
    private float groundedTimer = 0f;
    private Vector3 totalMovement;
    private Vector3 airMovementDirection; // Nueva variable para almacenar la dirección del movimiento en el aire
    private bool keepAirMovement = false; // Controlar si debemos mantener el movimiento aéreo
    private bool isGrounded = true;
    private bool isLanding = false;
    private float landingDelay = 0f;
    private bool isTouchingWater = false;
    private CarPatrolling carPatrolling;
    // AUDIO
    private AudioPlayer Audio;

    private bool isCollision = false;
    private bool isDead = false;

    void Awake()
    {
        PI = gameObject.GetComponent<PlayerInput>();
        CC = gameObject.GetComponent<CharacterController>();
        // Añadir ClickToMove automáticamente si no existe
        clickToMove = gameObject.GetComponent<ClickToMove>();
        if (clickToMove == null)
        {
            clickToMove = gameObject.AddComponent<ClickToMove>();
        }
        normalHeight = CC.height;
        GameObject car = GameObject.FindGameObjectWithTag("Enemy");
        if (car != null)
        {
            carPatrolling = car.GetComponent<CarPatrolling>();
        }
        Audio = gameObject.GetComponent<AudioPlayer>();
    }

    void Update()
    {
        if (isDead)
        {
            // Esperar a que el jugador pulse cualquier botón del mando o tecla
            if (Input.anyKeyDown)
            {
                Debug.Log("[Respawn] Tecla pulsada: " + Input.inputString);
                Respawn();
            }
            return;
        }

        totalMovement = Vector3.zero;
        escarbando = PI.escarbando;

        // Si el jugador mueve el joystick/pulsa teclas, cancelar el destino click
        if (clickToMove != null && clickToMove.isMovingToTarget && PI.movement.magnitude > 0.1f)
        {
            clickToMove.CancelMoveTarget();
        }

        ApplyGravity();
        CalculateMovementRotate();
        HandleJump();
        HandleWaterEffect();
        ApplyFinalMovement();
        SetAnimations();
        PlayFX();
        CheckGrounded();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            StartWaterEffect(other);
        }

        if (other.CompareTag("Enemy"))
        {
            Vector3 closestPoint = other.transform.position;
            Vector3 collisionPoint = new Vector3(closestPoint.x, transform.position.y, closestPoint.z);
            EnemyCollision(collisionPoint);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // TODO 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isTouchingWater = false;
            // TODO
        }
    }

    private void StartLanding()
    {
        isLanding = true;
        animator.SetBool("Landing", isLanding);
        StartCoroutine(LandingCoroutine());
    }

    private IEnumerator LandingCoroutine()
    {

        landingDelay = landingDelay > 1.5f ? 1.5f : landingDelay;
        yield return new WaitForSeconds(landingDelay);
        EndLanding();
    }

    private void EndLanding()
    {
        isLanding = false;
        isGrounded = true;
        animator.SetBool("Landing", isLanding);
    }


    private void ApplyGravity()
    {
        if (!CC.isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -0.5f;
        }
        totalMovement += Vector3.up * verticalVelocity * Time.deltaTime;
    }

    private void CalculateMovementRotate()
    {
        if (isLanding) return;

        Vector3 movement;
        float speed;

        // --- CLICK TO MOVE ---
        // Si hay un destino activo de click-to-move y estamos en el suelo
        if (clickToMove != null && clickToMove.isMovingToTarget && isGrounded)
        {
            // Calcular dirección hacia el destino (solo horizontal)
            Vector3 targetFlat = new Vector3(clickToMove.moveTarget.Value.x, transform.position.y, clickToMove.moveTarget.Value.z);
            Vector3 direction = (targetFlat - transform.position).normalized;

            // Si ya llegamos, cancelar destino
            if (clickToMove.HasArrivedAtTarget(transform.position))
            {
                clickToMove.CancelMoveTarget();
                movement = Vector3.zero;
                speed = 0;
            }
            else
            {
                // Si es doble clic, corre al destino (como si pulsaras run)
                bool shouldRun = clickToMove.isDoubleClick;
                speed = shouldRun ? runningSpeed : moveSpeed;
                speed = PI.escarbando ? moveSpeed * 0.5f : speed;
                movement = direction;
            }
        }
        else
        {
            // --- CONTROL NORMAL (joystick / teclas) ---
            if (isGrounded || !keepAirMovement)
            {
                movement = CalculateMovementFromCamera();
                airMovementDirection = movement.normalized;
            }
            else
            {
                movement = airMovementDirection * PI.movement.magnitude;
            }

            speed = PI.isRunning ? runningSpeed : moveSpeed;
            speed = PI.escarbando ? moveSpeed * 0.5f : speed;
        }

        totalMovement += movement * speed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * rotateDump);
        }
    }

    private void CheckGrounded()
    {
        if (!CC.isGrounded)
        {
            groundedTimer += Time.deltaTime;
            if (groundedTimer >= thresholdGrounded)
            {
                isGrounded = false;
                keepAirMovement = true;
                landingDelay = groundedTimer - thresholdGrounded;
            }
        }
        else
        {
            if (groundedTimer >= thresholdGrounded)
            {
                StartLanding();
            }
            else
            {
                isGrounded = true;
            }
            groundedTimer = 0f;
            keepAirMovement = false;
        }
    }

    private void HandleJump()
    {
        if (PI.jump > 0 && !PI.escarbando && CC.isGrounded && !isLanding)
        {
            animator.SetTrigger("Jump");
            isGrounded = false;
            keepAirMovement = true;
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            totalMovement += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }

    // Nuevo método para iniciar el efecto de agua
    private void StartWaterEffect(Collider other)
    {
        // TODO  isTouchingWater = true;
        Debug.Log("¡Entró en agua!");
    }

    // Nuevo método para manejar el efecto de agua
    private void HandleWaterEffect()
    { // TODO
        if (!isTouchingWater) return;
    }

    /// <summary>
    /// Indica si el jugador se está moviendo (por joystick/teclas O por click-to-move).
    /// </summary>
    private bool IsMoving()
    {
        return (PI.movement != Vector2.zero || (clickToMove != null && clickToMove.isMovingToTarget))
                && CC.velocity.magnitude > 0.1f;
    }

    /// <summary>
    /// Determina si el jugador debería estar en modo correr (por joystick/teclas O por doble clic).
    /// </summary>
    private bool IsRunning()
    {
        if (PI.isRunning) return true;
        if (clickToMove != null && clickToMove.isMovingToTarget && clickToMove.isDoubleClick) return true;
        return false;
    }

    private void SetAnimations()
    {
        float targetSpeed = 0;

        if (!isLanding || !isGrounded)
        {
            targetSpeed = IsMoving() ? (IsRunning() ? 1 : 0.5f) : 0;
        }

        animator.SetFloat("Movement", targetSpeed, 0.15f, Time.deltaTime);
        animator.SetBool("Escarbando", PI.escarbando);
        CC.height = !isGrounded ? normalHeight / 1.9f : normalHeight;
        CC.height = PI.escarbando ? normalHeight / 1.9f : normalHeight;
        animator.SetBool("isGrounded", isGrounded);
    }

    //AUDIO
    private void PlayFX()
    {

        bool moving = IsMoving();
        bool running = IsRunning();

        if (!PI.escarbando && moving && running && isGrounded && !isLanding)
        {
            Audio.LoadClip("pasos_317ms");
            Audio.Play(true);
        }

        if (!PI.escarbando && moving && !running && isGrounded && !isLanding)
        {
            Audio.LoadClip("pasos_500ms");
            Audio.Play(true);
        }

        if (PI.escarbando)
        {
            Audio.LoadClip("Dig");
            Audio.Play(true);
        }

        if (isLanding)
        {
            Audio.LoadClip("Landing");
            Audio.Play(false);
        }

        if (!PI.escarbando && !moving && !isLanding && !isCollision || !isGrounded && !isLanding || !PI.enabled && !CC.enabled)
        {
            Audio.Stop();
        }
    }

    private void EnemyCollision(Vector3 collisionPoint)
    {
        isCollision = true;
        animator.SetTrigger("Muerte");
        carPatrolling.SetState(CarPatrolling.AgentState.Patrolling);
        StartCoroutine(PerformKnockback(collisionPoint));
        Audio.LoadClip("CarCrash");
        Audio.Play(false);
    }

    //FIX el jugador se mueve en direccion contraria 
    private IEnumerator PerformKnockback(Vector3 collisionPoint)
    {
        CC.enabled = false;
        // Calcular dirección del knockback
        Vector3 knockbackDirection = (transform.position - collisionPoint).normalized;
        knockbackDirection.y = 0;
        knockbackDirection.Normalize();

        // Calcular rotación para mirar hacia el punto de impacto
        Vector3 lookDirection = collisionPoint - transform.position;
        lookDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        // Posiciones iniciales
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + knockbackDirection * knockbackDistance;
        float elapsedTime = 0f;

        // TODO calcular targetPosition.y del terreno en conde cae el jugador  

        // Efecto de knockback
        while (elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / knockbackDuration;

            // Movimiento horizontal (lineal)
            Vector3 horizontalPos = Vector3.Lerp(startPosition, targetPosition, progress);

            // Componente vertical (parábola)
            float verticalPos = Mathf.Sin(progress * Mathf.PI) * knockbackMaxHeight;

            // Aplicar movimiento combinado
            transform.position = new Vector3(horizontalPos.x, startPosition.y + verticalPos, horizontalPos.z);

            // Rotación progresiva
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, knockbackRotationSpeed * Time.deltaTime);

            yield return null;
        }

        // Asegurar posición final correcta
        transform.position = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        PI.enabled = false;
        CC.enabled = true;

        isLanding = true;
        isCollision = false;
        isDead = true;
        Audio.LoadClip("Landing");
        Audio.Play(false);
        groundedTimer = 1;
    }

    private void Respawn()
    {
        Debug.Log("[Respawn] Recargando escena...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private Vector3 CalculateMovementFromCamera()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        return forward * PI.movement.y + right * PI.movement.x;
    }

    private void ApplyFinalMovement()
    {
        if (CC.enabled)
        {
            CC.Move(totalMovement);
        }
    }

    /*void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
        } */
}
