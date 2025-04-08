using UnityEngine;

public class PlayerControllerPlaya : MonoBehaviour
{
    public Animator animator;
    CharacterController CC;
    PlayerInput PI;
    public GameObject holePrefab; // Prefab opcional para hoyo visual (Opcional)
    public float runningSpeed;
    public float moveSpeed;
    public float gravity;
    public float rotateDump;
    private float normalHeight;

    // Variables para crear hoyos
    public float digRadius = 0.5f; // Radio del hoyo
    public float digDepth = 0.1f; // Profundidad del hoyo
    public float digRate = 0.1f; // Cada cuántos segundos se hace un hoyo nuevo
    public float maxDeep = 0.5f; // TODO Profundidad máxima permitida para los hoyos 

    private float lastDigTime;
    private Terrain terrain;

    void Start()
    {
        PI = gameObject.GetComponent<PlayerInput>();
        CC = gameObject.GetComponent<CharacterController>();
        normalHeight = CC.height;
        lastDigTime = -digRate;

        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("No se encontró un terreno activo en la escena.");
        }
    }

    void Update()
    {
        MoveRotate();
        SetAnimations();

        // Lógica para cavar hoyos
        if (PI.escarbando)
        {
            DigHoleAtPlayerPosition();
        }
    }

    private void DigHoleAtPlayerPosition()
    {
        if (Time.time - lastDigTime < digRate) return;
        // Posición del jugador en el terreno
        Vector3 playerPos = transform.position;
        // Opción 1: Modificar el terreno directamente
        if (terrain != null)
        {
            lastDigTime = Time.time;
            Vector3 terrainPos = playerPos - terrain.transform.position;
            Vector3 normalizedPos = new Vector3(terrainPos.x / terrain.terrainData.size.x, 0, terrainPos.z / terrain.terrainData.size.z);
            int x = (int)(normalizedPos.x * terrain.terrainData.heightmapResolution);
            int z = (int)(normalizedPos.z * terrain.terrainData.heightmapResolution);
            int radius = (int)(digRadius * terrain.terrainData.heightmapResolution / terrain.terrainData.size.x) | 1; // 1: ancho minimo del hoyo
            float[,] heights = terrain.terrainData.GetHeights(x - radius, z - radius, radius * 2, radius * 2);
            for (int i = 0; i < radius * 2; i++)
            {
                for (int j = 0; j < radius * 2; j++)
                {
                    float distance = Vector2.Distance(new Vector2(i, j), new Vector2(radius, radius));
                    if (distance <= radius)
                    {
                        float reduction = digDepth * (1 - distance / radius);
                        heights[i, j] = Mathf.Max(0, heights[i, j] - reduction / terrain.terrainData.size.y);
                    }
                }
            }
            terrain.terrainData.SetHeights(x - radius, z - radius, heights);
        }

        // Opción 2: Instanciar un prefab de hoyo visual (opcional)
        if (holePrefab != null)
        {
            Instantiate(holePrefab, playerPos, Quaternion.identity);
        }
    }

    private void SetAnimations()
    {
        float targetSpeed = (PI.movement != Vector2.zero) ? (PI.isRunning ? 1 : 0.5f) : 0;
        animator.SetFloat("Movement", targetSpeed, 0.15f, Time.deltaTime);
        animator.SetBool("Escarbando", PI.escarbando);
        // corregir altura de la animacion, personaje flotando
        CC.height = normalHeight;
        if (PI.escarbando)
        {
            CC.height = normalHeight / 2;
        }
    }

    private void MoveRotate()
    {
        Vector3 movement = CalculateMovementFromCamera();
        float speed = PI.isRunning ? runningSpeed : moveSpeed;
        speed = PI.escarbando ? moveSpeed * 0.5f : speed;
        CC.Move(movement * speed * Time.deltaTime);
        if (!CC.isGrounded)
        {
            CC.Move(-Vector3.up * gravity * Time.deltaTime);
        }
        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * rotateDump);
        }
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
}