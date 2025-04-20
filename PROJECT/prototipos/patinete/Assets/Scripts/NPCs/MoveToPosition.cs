using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarPatrolling : MonoBehaviour
{
    public enum AgentState { Chasing, Patrolling }

    [Header("NavMesh Settings")]
    public float acceleration = 5f;
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
    public float patrolWaitTime = 2f;

    private Transform target;
    private NavMeshAgent agent;
    private float currentSpeed = 0f;
    private bool isReversing = false;
    private float lastPathUpdateTime;
    private Vector3 lastTargetPosition;

    // Variables para control de retroceso
    private Vector3 reverseStartPosition;
    private float lastDirectionChangeTime;
    private bool forcedReverse = false;

    public AgentState currentState = AgentState.Patrolling;
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GetPlayer();

        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.1f;
        agent.avoidancePriority = 50;

        lastDirectionChangeTime = -minDirectionChangeTime; // Permitir cambio inmediato al inicio
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

        // Verificar si debemos comenzar un retroceso forzado
        bool shouldStartReverse = Mathf.Abs(angleToNextPoint) > 90f &&
                                    distanceToNextPoint < reverseDistanceThreshold &&
                                    !forcedReverse &&
                                    Time.time - lastDirectionChangeTime >= minDirectionChangeTime;

        if (shouldStartReverse)
        {
            StartForcedReverse();
        }

        // Calcular velocidad deseada
        float desiredSpeed;

        if (forcedReverse)
        {
            // Mantener retroceso hasta alcanzar la distancia mínima
            float reverseDistanceCovered = Vector3.Distance(transform.position, reverseStartPosition);
            if (reverseDistanceCovered < minReverseDistance)
            {
                desiredSpeed = -agent.speed * 0.5f;
            }
            else
            {
                EndForcedReverse();
                desiredSpeed = agent.speed;
            }
        }
        else if (distanceToNextPoint > brakingDistance)
        {
            desiredSpeed = agent.speed;
        }
        else
        {
            desiredSpeed = Mathf.Lerp(0, agent.speed, distanceToNextPoint / brakingDistance);
        }

        // Suavizar cambios de velocidad
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed,
                                      (desiredSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime);

        // Movimiento y rotación
        if (Mathf.Abs(currentSpeed) > 0.01f)
        {
            float steeringAngle = Mathf.Clamp(angleToNextPoint, -maxSteeringAngle, maxSteeringAngle);

            // Invertir el ángulo de dirección cuando estamos en reversa
            if (currentSpeed < 0)
            {
                steeringAngle *= -1f;
            }

            transform.Rotate(Vector3.up, steeringAngle * Time.deltaTime * (currentSpeed < 0 ? 0.7f : 1f));

            Vector3 moveDirection = transform.forward * currentSpeed;
            agent.velocity = moveDirection;
        }
        else
        {
            agent.velocity = Vector3.zero;
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
            UpdatePath(target.position);
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

        if (agent.remainingDistance < agent.stoppingDistance)
        {
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                UpdatePath(patrolPoints[currentPatrolIndex].position);
                patrolWaitTimer = 0f;
            }
        }
    }

    public void SetState(AgentState newState)
    {
        currentState = newState;
        if (currentState == AgentState.Patrolling && patrolPoints.Count > 0)
        {
            UpdatePath(patrolPoints[currentPatrolIndex].position);
        }
        else if (currentState == AgentState.Chasing && target != null)
        {
            UpdatePath(target.position);
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

        // Dibujar posición de inicio del retroceso forzado
        if (forcedReverse)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(reverseStartPosition, 0.5f);
            Gizmos.DrawLine(transform.position, reverseStartPosition);
        }
    }
}