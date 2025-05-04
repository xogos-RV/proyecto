using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadGameScene(string value)
    {
        SceneManager.LoadScene(value);
    }
}