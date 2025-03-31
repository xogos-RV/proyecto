using UnityEngine;

public class HideCursor : MonoBehaviour
{
    void Start()
    {
        // Oculta el cursor del ratón
        Cursor.visible = false;
        // Opcional: Bloquea el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
    }
}
