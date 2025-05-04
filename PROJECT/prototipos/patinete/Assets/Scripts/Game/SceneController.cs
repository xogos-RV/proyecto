using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Playa");
    }
}