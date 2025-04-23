using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarPatrolling : MonoBehaviour
{
    public enum AgentState { Chasing, Patrolling }

    public AgentState currentState = AgentState.Patrolling;

    [Header("NavMesh Settings")]
    public float acceleration = 10f;
    public float chaisingAcceleration = 30f;
    public float deceleration = 10f;
    public float angularSpeed = 120f;
    public float maxSteeringAngle = 45f;
    public float brakingDistance = 5f;
    public float reverseDistanceThreshold = 3f;
    public float pathUpdateRate = 0.5f;

    [Header("Reverse Settings")]
    public float minReverseDistance = 2f;
    public float minDirectionChangeTime = 2f;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;

    private Transform target;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private bool isReversing = false;
    private float lastPathUpdateTime;
    private Vector3 lastTargetPosition;

    private Vector3 reverseStartPosition;
    private float lastDirectionChangeTime;
    private bool forcedReverse = false;

    private int currentPatrolIndex = 0;
    private bool angleCondition;
    private bool distanceCondition;
    private bool timeCondition;
    private DecalCollision decal;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        GetPlayer();

        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.1f;
        agent.avoidancePriority = 50;

        lastDirectionChangeTime = -minDirectionChangeTime;

        GameObject dec = GameObject.FindGameObjectWithTag("Decal");
        if (dec != null)
        {
            decal = dec.GetComponent<DecalCollision>();
        }
    }

    void Update()
    {
        if (currentState == AgentState.Chasing)
        {
            if (target == null)
            {
                GetPlayer();
                return;
            }

            if (Time.time - lastPathUpdateTime > pathUpdateRate ||
                Vector3.Distance(target.position, lastTargetPosition) > 1f)
            {
                UpdatePath(target.position);
            }
            // TODO  colision jugador
            // Calcular la distancia entre el Rigidbody y el destino
            float distanceToPlayer = Vector3.Distance(rb.position, target.position);

            // Comprobar si el Rigidbody está cerca del destino
            if (distanceToPlayer < 1) // TODO parametrizar 
            {
                SetState(AgentState.Patrolling);
            }
        }
        else if (currentState == AgentState.Patrolling)
        {
            Patrol();
        }

        MoveTowardsTarget();
    }

    private void UpdatePath(Vector3 destination)
    {
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
            lastPathUpdateTime = Time.time;
            lastTargetPosition = destination;
        }
    }


    public void SetState(AgentState newState)
    {
        currentState = newState;
        if (currentState == AgentState.Patrolling && patrolPoints.Count > 0)
        {
            decal.EnableVision(false);
            UpdatePath(patrolPoints[currentPatrolIndex].position);
        }
        else if (currentState == AgentState.Chasing && target != null)
        {
            decal.EnableVision(true);
            UpdatePath(target.position);
        }
    }

    private void MoveTowardsTarget()
    {
        if (agent.pathPending || agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        Vector3 nextPathPoint = agent.steeringTarget;
        Vector3 directionToNextPoint = nextPathPoint - transform.position;
        directionToNextPoint.y = 0;
        float distanceToNextPoint = directionToNextPoint.magnitude;
        directionToNextPoint.Normalize();

        float angleToNextPoint = Vector3.SignedAngle(transform.forward, directionToNextPoint, Vector3.up);

        // Condiciones para iniciar el retroceso forzado
        angleCondition = Mathf.Abs(angleToNextPoint) > 90f;
        distanceCondition = distanceToNextPoint < reverseDistanceThreshold;
        timeCondition = Time.time - lastDirectionChangeTime >= minDirectionChangeTime;

        bool shouldStartReverse = angleCondition && distanceCondition && !forcedReverse && timeCondition;

        if (shouldStartReverse)
        {
            StartForcedReverse();
        }

        // Calcular velocidad deseada
        float desiredSpeed;
        float accel = currentState == AgentState.Patrolling ? acceleration : chaisingAcceleration;
        if (forcedReverse)
        {
            // Mantener retroceso hasta alcanzar la distancia mínima
            float reverseDistanceCovered = Vector3.Distance(transform.position, reverseStartPosition);
            if (reverseDistanceCovered < minReverseDistance)
            {
                desiredSpeed = accel * -1; // Retroceso
            }
            else
            {
                EndForcedReverse();
                desiredSpeed = accel; // Regresar a la velocidad normal
            }
        }
        else if (distanceToNextPoint > brakingDistance)
        {
            desiredSpeed = accel; // Velocidad normal
        }
        else
        {
            desiredSpeed = Mathf.Lerp(0, accel, distanceToNextPoint / brakingDistance); // Frenar
        }

        // Suavizar cambios de velocidad

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, (desiredSpeed > currentSpeed ? accel : deceleration) * Time.deltaTime);

        // Movimiento y rotación usando Rigidbody
        if (Mathf.Abs(currentSpeed) > 0.01f)
        {
            Vector3 moveDirection = directionToNextPoint * currentSpeed;

            // FIX no sirve Aplicar fuerza al Rigidbody  navMeshAgent anula el comportamiento del rigidbody 
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z), ForceMode.Acceleration);

            float steeringAngle = Mathf.Clamp(angleToNextPoint, -maxSteeringAngle, maxSteeringAngle);

            // Invertir el ángulo de dirección cuando estamos en reversa
            if (currentSpeed < 0)
            {
                steeringAngle *= -1f;
            }

            transform.Rotate(Vector3.up, steeringAngle * Time.deltaTime * (currentSpeed < 0 ? 0.7f : 1f));
        }
        else
        {
            rb.linearVelocity = Vector3.zero; // Detener el Rigidbody si la velocidad es muy baja
        }

        // Actualizar estado de reversa
        bool newReversingState = currentSpeed < 0;
        if (newReversingState != isReversing)
        {
            isReversing = newReversingState;
            if (isReversing && !forcedReverse)
            {
                lastDirectionChangeTime = Time.time;
            }
        }

        Debug.DrawLine(transform.position, nextPathPoint, Color.green);
    }

    private void StartForcedReverse()
    {
        forcedReverse = true;
        reverseStartPosition = transform.position;
        lastDirectionChangeTime = Time.time;
        Debug.Log("Iniciando retroceso forzado");
    }

    private void EndForcedReverse()
    {
        forcedReverse = false;
        Debug.Log("Finalizando retroceso forzado");
    }

    private void GetPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró el jugador.");
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Count == 0)
            return;
        if (agent.pathPending)
            return;

        // Obtener el destino actual del NavMeshAgent
        Vector3 destination = agent.pathEndPosition;

        // Calcular la distancia entre el Rigidbody y el destino
        float distanceToDestination = Vector3.Distance(rb.position, destination);

        // Comprobar si el Rigidbody está cerca del destino
        if (distanceToDestination < 1) // TODO parametrizar 
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            UpdatePath(patrolPoints[currentPatrolIndex].position);
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
        }

        if (forcedReverse)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(reverseStartPosition, 0.5f);
            Gizmos.DrawLine(transform.position, reverseStartPosition);
        }

        // Depuración de condiciones de retroceso
        Vector3 debugPosition = transform.position + Vector3.up * 2f;

        Gizmos.color = angleCondition ? Color.green : Color.red;
        Gizmos.DrawSphere(debugPosition + Vector3.left, 0.2f);

        Gizmos.color = distanceCondition ? Color.green : Color.red;
        Gizmos.DrawSphere(debugPosition, 0.2f);

        Gizmos.color = timeCondition ? Color.green : Color.red;
        Gizmos.DrawSphere(debugPosition + Vector3.right, 0.2f);
    }
}