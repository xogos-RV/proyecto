using UnityEngine;

public class CameraController : MonoBehaviour
{
    PlayerInput playerInput;

    [Header("Configuración de Rotación")]
    public float rotationSpeed = 90f; // Grados por segundo
    public float rotationAngle = 90f; // Ángulo total a rotar
    public float smoothTime = 0.3f; // Tiempo de suavizado

    private float targetYRotation = 0f;
    private float currentYRotation = 0f;
    private float yRotationVelocity = 0f;
    private float camLInput;
    private float camRInput;
    private bool wasCamLPressed = false;
    private bool wasCamRPressed = false;

    private void Start()
    {
        // Inicializar con la rotación actual
        targetYRotation = transform.localEulerAngles.y;
        currentYRotation = targetYRotation;
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {

        if (playerInput != null)
        {
            camLInput = playerInput.camL;
            camRInput = playerInput.camR;
        }

        bool isCamLPressed = camLInput > 0.5f;
        bool isCamRPressed = camRInput > 0.5f;

        // Rotar solo en el frame que se presiona el botón
        if (isCamLPressed && !wasCamLPressed)
        {
            targetYRotation += rotationAngle;
        }

        if (isCamRPressed && !wasCamRPressed)
        {
            targetYRotation -= rotationAngle;
        }

        // Actualizar estados anteriores
        wasCamLPressed = isCamLPressed;
        wasCamRPressed = isCamRPressed;

        // Suavizar la rotación
        currentYRotation = Mathf.SmoothDampAngle(currentYRotation, targetYRotation, ref yRotationVelocity, smoothTime);

        // FIX cinemachoine   Aplicar la rotación alrededor del eje Y (vertical) 
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            currentYRotation,
            transform.localEulerAngles.z
        );
        Debug.Log(" transform.localEulerAngles " + transform.localEulerAngles);
    }
}