using UnityEngine;

/// <summary>
/// Gestiona dos cursores 3D holográficos sobre el terreno:
/// 1. CursorSeguimiento - Sigue al ratón en tiempo real (color amarillo fijo)
/// 2. CursorDestino - Marca el destino del click-to-move (alterna Cian↔Magenta)
/// Se engancha al ClickToMove existente sin duplicar su funcionalidad.
/// </summary>
public class CursorDestino : MonoBehaviour
{
    [Header("Cursores")]
    [Tooltip("Objeto 3D que sigue al ratón en tiempo real. Si no se asigna, se crea uno por defecto (aro).")]
    public GameObject cursorSeguimiento;
    [Tooltip("Objeto 3D que marca el destino del clic. Si no se asigna, se crea uno por defecto (disco).")]
    public GameObject cursorDestino;

    [Header("Colores Cursor Seguimiento")]
    [Tooltip("Color del cursor que sigue al ratón (amarillo por defecto)")]
    public Color colorSeguimiento = new Color(1f, 1f, 0f, 0.5f);

    [Header("Colores Cursor Destino (alternancia)")]
    [Tooltip("Primer color de la alternancia (Cian)")]
    public Color colorDestino1 = new Color(0f, 1f, 1f, 0.5f);
    [Tooltip("Segundo color de la alternancia (Magenta)")]
    public Color colorDestino2 = new Color(1f, 0f, 1f, 0.5f);

    [Header("Animación")]
    [Tooltip("Velocidad de alternancia entre colores del destino")]
    public float animSpeed = 2f;

    [Header("Materiales")]
    [Tooltip("Material para el cursor de seguimiento (se carga automáticamente)")]
    public Material materialSeguimiento;
    [Tooltip("Material para el cursor de destino (se carga automáticamente)")]
    public Material materialDestino;

    [Header("Configuración")]
    [Tooltip("Máscara de layers para detectar el terreno con el rayo del ratón")]
    public LayerMask groundLayer = -1;
    [Tooltip("Distancia máxima del rayo")]
    public float maxRayDistance = 500f;

    // Componentes
    private ClickToMove clickToMove;
    private Camera mainCamera;

    // Renderers y PropertyBlocks para cada cursor
    private Renderer renderSeguimiento;
    private Renderer renderDestino;
    private MaterialPropertyBlock pbSeguimiento;
    private MaterialPropertyBlock pbDestino;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Configurar cursor del sistema:
        // - No bloqueado (para que Input.mousePosition se actualice con el ratón)
        // - Oculto (para no verlo encima del cursor 3D)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // Buscar ClickToMove en este mismo GameObject
        clickToMove = GetComponent<ClickToMove>();
        if (clickToMove == null)
        {
            Debug.LogError("[CursorDestino] No se encontró ClickToMove en el GameObject.");
            enabled = false;
            return;
        }

        // Cargar materiales si no se asignaron
        if (materialSeguimiento == null)
            materialSeguimiento = Resources.Load<Material>("Materials/CursorSeguimiento");
        if (materialDestino == null)
            materialDestino = Resources.Load<Material>("Materials/CursorHolograma");

        // Crear cursores por defecto si no se asignaron
        if (cursorSeguimiento == null)
            CreateCursorSeguimiento();
        if (cursorDestino == null)
            CreateCursorDestino();

        // Configurar renderers
        SetupCursorRenderer(cursorSeguimiento, ref renderSeguimiento, ref pbSeguimiento);
        SetupCursorRenderer(cursorDestino, ref renderDestino, ref pbDestino);

        // El cursor de destino empieza oculto
        if (cursorDestino != null)
            cursorDestino.SetActive(false);
    }

    private void Update()
    {
        if (clickToMove == null || mainCamera == null) return;

        // Asegurar en cada frame que el cursor del sistema:
        // - No esté bloqueado (para que Input.mousePosition funcione)
        // - Esté oculto (para no verlo encima del cursor 3D)
        if (Cursor.lockState != CursorLockMode.None || Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }

        // --- CURSOR DE SEGUIMIENTO (siempre activo) ---
        UpdateCursorSeguimiento();

        // --- CURSOR DE DESTINO (solo cuando hay destino activo) ---
        UpdateCursorDestino();
    }

    /// <summary>
    /// Lanza un rayo desde la cámara a la posición del ratón y posiciona el cursor de seguimiento.
    /// </summary>
    private void UpdateCursorSeguimiento()
    {
        if (cursorSeguimiento == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundLayer))
        {
            cursorSeguimiento.transform.position = hit.point + Vector3.up * 0.05f;
            cursorSeguimiento.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (!cursorSeguimiento.activeSelf)
                cursorSeguimiento.SetActive(true);

            // Aplicar color fijo
            if (renderSeguimiento != null && pbSeguimiento != null)
            {
                pbSeguimiento.SetColor("_BaseColor", colorSeguimiento);
                pbSeguimiento.SetColor("_EmissionColor", colorSeguimiento * 0.8f);
                renderSeguimiento.SetPropertyBlock(pbSeguimiento);
            }
        }
        else
        {
            if (cursorSeguimiento.activeSelf)
                cursorSeguimiento.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra/oculta el cursor de destino según el estado del ClickToMove.
    /// </summary>
    private void UpdateCursorDestino()
    {
        if (cursorDestino == null) return;

        if (clickToMove.isMovingToTarget && clickToMove.moveTarget.HasValue)
        {
            Vector3 targetPos = clickToMove.moveTarget.Value;
            cursorDestino.transform.position = targetPos + Vector3.up * 0.05f;
            cursorDestino.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (!cursorDestino.activeSelf)
                cursorDestino.SetActive(true);

            // Animar colores (alternancia)
            if (renderDestino != null && pbDestino != null)
            {
                float t = Mathf.PingPong(Time.time * animSpeed, 1f);
                Color currentColor = Color.Lerp(colorDestino1, colorDestino2, t);
                pbDestino.SetColor("_BaseColor", currentColor);
                pbDestino.SetColor("_EmissionColor", currentColor * 0.8f);
                renderDestino.SetPropertyBlock(pbDestino);
            }
        }
        else
        {
            if (cursorDestino.activeSelf)
                cursorDestino.SetActive(false);
        }
    }

    /// <summary>
    /// Configura el Renderer y MaterialPropertyBlock para un cursor.
    /// </summary>
    private void SetupCursorRenderer(GameObject cursor, ref Renderer rendererRef, ref MaterialPropertyBlock pbRef)
    {
        if (cursor == null) return;
        rendererRef = cursor.GetComponentInChildren<Renderer>();
        if (rendererRef != null)
        {
            pbRef = new MaterialPropertyBlock();
            rendererRef.GetPropertyBlock(pbRef);
        }
    }

    /// <summary>
    /// Crea el cursor de seguimiento por defecto (un aro/cuadrado).
    /// </summary>
    private void CreateCursorSeguimiento()
    {
        // Usamos un quad (plano) rotado horizontalmente como marcador
        GameObject aro = GameObject.CreatePrimitive(PrimitiveType.Quad);
        aro.name = "CursorSeguimiento_Aro";
        aro.transform.SetParent(transform);
        aro.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        aro.transform.localPosition = Vector3.zero;

        if (materialSeguimiento != null)
        {
            Renderer rend = aro.GetComponent<Renderer>();
            rend.material = materialSeguimiento;
        }

        cursorSeguimiento = aro;
    }

    /// <summary>
    /// Crea el cursor de destino por defecto (un disco circular).
    /// </summary>
    private void CreateCursorDestino()
    {
        GameObject disco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disco.name = "CursorDestino_Disco";
        disco.transform.SetParent(transform);
        disco.transform.localScale = new Vector3(0.8f, 0.04f, 0.8f);
        disco.transform.localPosition = Vector3.zero;

        if (materialDestino != null)
        {
            Renderer rend = disco.GetComponent<Renderer>();
            rend.material = materialDestino;
        }

        cursorDestino = disco;
    }

    private void OnDestroy()
    {
        pbSeguimiento?.Clear();
        pbDestino?.Clear();
    }
}
