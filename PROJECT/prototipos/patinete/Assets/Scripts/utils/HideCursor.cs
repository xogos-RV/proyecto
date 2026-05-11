using UnityEngine;

/// <summary>
/// DESACTIVADO: El cursor del sistema ahora lo gestiona CursorDestino.cs
/// para los cursores 3D holográficos de seguimiento y destino.
/// 
/// Si necesitas ocultar el cursor del sistema en el futuro,
/// descomenta las líneas de abajo o elimina este script del GameObject.
/// </summary>
public class HideCursor : MonoBehaviour
{
    void Start()
    {
        // El cursor del sistema se mantiene visible y desbloqueado
        // para que los cursores 3D (CursorDestino) funcionen correctamente.
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }
}
