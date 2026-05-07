using UnityEngine;

public class ClickToMove : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Máscara de layers donde se puede hacer clic para moverse (el terreno). -1 = todos los layers")]
    public LayerMask groundLayer = -1; // Todos los layers por defecto
    [Tooltip("Distancia mínima para considerar que se ha llegado al destino")]
    public float arrivalDistance = 0.5f;
    [Tooltip("Distancia máxima a la que se puede hacer clic")]
    public float maxClickDistance = 200f;
    [Tooltip("Tiempo máximo entre clics para considerar doble clic (segundos)")]
    public float doubleClickTime = 0.3f;

    [Header("Debug")]
    public bool showDebug = true;
    public Color debugColor = Color.green;

    /// <summary>
    /// Destino actual del movimiento click-to-move. null si no hay destino activo.
    /// </summary>
    public Vector3? moveTarget { get; private set; } = null;

    /// <summary>
    /// Indica si el jugador está actualmente moviéndose hacia un destino de click.
    /// </summary>
    public bool isMovingToTarget => moveTarget.HasValue;

    /// <summary>
    /// Indica si el último clic fue doble clic (para correr).
    /// </summary>
    public bool isDoubleClick { get; private set; } = false;

    // Referencia al PlayerInput para detectar si el jugador usa joystick/teclas
    private PlayerInput playerInput;
    private Camera mainCamera;

    // Variables para detección de doble clic
    private float lastClickTime = 0f;
    private Vector2 lastClickScreenPos;
    private bool hasPendingClick = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;

        // Asegurar que el cursor del ratón esté visible y no bloqueado
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Tecla Escape para ocultar/mostrar el cursor del ratón
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Detectar clic izquierdo del ratón o toque táctil
        if (HasClickOrTouch(out Vector2 screenPos))
        {
            HandleClick(screenPos);
        }
    }

    /// <summary>
    /// Detecta si hubo un clic izquierdo del ratón o un toque en pantalla táctil.
    /// Devuelve la posición en pantalla.
    /// </summary>
    private bool HasClickOrTouch(out Vector2 screenPosition)
    {
        screenPosition = Vector2.zero;

        // Toque táctil (prioritario para móvil)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                screenPosition = touch.position;
                return true;
            }
        }

        // Clic izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Maneja el clic/toque, detectando si es simple o doble.
    /// </summary>
    private void HandleClick(Vector2 screenPos)
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // Detectar doble clic: mismo botón, cerca en tiempo y espacio
        if (hasPendingClick && timeSinceLastClick < doubleClickTime &&
            Vector2.Distance(screenPos, lastClickScreenPos) < 50f)
        {
            // Doble clic
            isDoubleClick = true;
            hasPendingClick = false;

            if (showDebug)
            {
                Debug.Log("[ClickToMove] ¡Doble clic! Corriendo al destino");
            }

            TrySetMoveTarget(screenPos);
        }
        else
        {
            // Primer clic
            isDoubleClick = false;
            lastClickTime = Time.time;
            lastClickScreenPos = screenPos;
            hasPendingClick = true;

            TrySetMoveTarget(screenPos);
        }
    }

    /// <summary>
    /// Lanza un rayo desde la cámara hasta el terreno para calcular el destino.
    /// </summary>
    private void TrySetMoveTarget(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxClickDistance, groundLayer))
        {
            moveTarget = hit.point;

            if (showDebug)
            {
                Debug.Log($"[ClickToMove] Nuevo destino: {hit.point} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
        }
        else
        {
            if (showDebug)
            {
                Debug.Log("[ClickToMove] No se encontró terreno en esa posición");
            }
        }
    }

    /// <summary>
    /// Cancela el destino actual de movimiento.
    /// </summary>
    public void CancelMoveTarget()
    {
        moveTarget = null;
        isDoubleClick = false;
    }

    /// <summary>
    /// Comprueba si el jugador ha llegado al destino.
    /// </summary>
    public bool HasArrivedAtTarget(Vector3 currentPosition)
    {
        if (!moveTarget.HasValue) return true;

        Vector3 targetFlat = new Vector3(moveTarget.Value.x, currentPosition.y, moveTarget.Value.z);
        float distance = Vector3.Distance(currentPosition, targetFlat);

        return distance <= arrivalDistance;
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || !moveTarget.HasValue) return;

        // Dibujar un marcador en el destino
        Gizmos.color = debugColor;
        Gizmos.DrawSphere(moveTarget.Value, 0.3f);

        // Línea desde el jugador hasta el destino
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, moveTarget.Value + Vector3.up * 0.5f);
        }
    }
}
