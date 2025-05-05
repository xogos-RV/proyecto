using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private static SceneController instance;

    // Componentes clave
    private AudioListener menuAudioListener;
    private AudioListener playerAudioListener;
    private Camera menuCamera;
    private Camera playerCamera;

    // Referencias a objetos
    private GameObject mainMenuRoot;
    private GameObject eventSystem;

    // Estados
    private GameObject playaSceneRoot;
    private bool isGamePaused = false;

    private void Awake()
    {
        // Implementación Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeReferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeReferences()
    {
        // Busca los objetos clave por nombre
        mainMenuRoot = GameObject.Find("MainMenuRoot");
        eventSystem = GameObject.Find("EventSystem");

        // Obtiene los componentes del menú
        menuCamera = Camera.main; // Asume que la cámara del menú es la main
        menuAudioListener = menuCamera.GetComponent<AudioListener>();

        if (mainMenuRoot == null)
        {
            Debug.LogError("No se encontró MainMenuRoot en la escena");
        }

        // Asegura que solo el AudioListener del menú esté activo inicialmente
        if (menuAudioListener != null)
            menuAudioListener.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && playaSceneRoot != null)
        {
            if (isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void LoadGameScene(string sceneName)
    {
        if (playaSceneRoot == null)
        {
            StartCoroutine(LoadSceneAdditive(sceneName));
        }
        else
        {
            ResumeGame();
        }
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        // Oculta el menú usando la referencia directa
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(false);

        // Carga la escena
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Configuración post-carga
        Scene playaScene = SceneManager.GetSceneByName(sceneName);
        GameObject[] rootObjects = playaScene.GetRootGameObjects();
        playaSceneRoot = rootObjects.Length > 0 ? rootObjects[0] : null;

        SetupGameScene();
    }

    private void SetupGameScene()
    {
        // Busca componentes en la escena de juego
        playerCamera = FindObjectInScene<Camera>(playaSceneRoot.scene);
        playerAudioListener = playerCamera?.GetComponent<AudioListener>();

        // Desactiva componentes del menú
        if (menuAudioListener != null) menuAudioListener.enabled = false;
        if (menuCamera != null) menuCamera.enabled = false;

        // Activa componentes del juego
        if (playerAudioListener != null) playerAudioListener.enabled = true;
        if (playerCamera != null) playerCamera.enabled = true;

        Time.timeScale = 1;
        isGamePaused = false;
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        isGamePaused = true;

        // Activa componentes del menú
        if (menuAudioListener != null) menuAudioListener.enabled = true;
        if (menuCamera != null) menuCamera.enabled = true;

        // Desactiva componentes del juego
        if (playerAudioListener != null) playerAudioListener.enabled = false;
        if (playerCamera != null) playerCamera.enabled = false;

        // Muestra el menú
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        isGamePaused = false;

        // Desactiva componentes del menú
        if (menuAudioListener != null) menuAudioListener.enabled = false;
        if (menuCamera != null) menuCamera.enabled = false;

        // Activa componentes del juego
        if (playerAudioListener != null) playerAudioListener.enabled = true;
        if (playerCamera != null) playerCamera.enabled = true;

        // Oculta el menú
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(false);
    }

    private T FindObjectInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            T component = obj.GetComponentInChildren<T>(true);
            if (component != null)
                return component;
        }
        return null;
    }
}