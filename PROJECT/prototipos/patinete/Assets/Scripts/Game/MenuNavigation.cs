using UnityEngine;
using System.Collections.Generic;

public class MenuNavigation : MonoBehaviour
{
    private RectTransform selector;
    private List<RectTransform> menuOptions;
    private int currentIndex = 0;

    private AudioSource audioSource;
    private AudioClip backgroundMusic;
    private AudioClip changeOptionSound;
    private AudioClip selectSound;

    private void Awake()
    {
        selector = GameObject.FindGameObjectWithTag("Selector").GetComponent<RectTransform>();
        Transform buttonsParent = transform.Find("Buttons");
        menuOptions = new List<RectTransform>();

        foreach (Transform child in buttonsParent)
        {
            if (child.TryGetComponent<RectTransform>(out var option))
            {
                menuOptions.Add(option);
            }
        }

        // Inicializa AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        // Carga los archivos de audio desde Resources
        backgroundMusic = Resources.Load<AudioClip>("Audio/invierno");
        changeOptionSound = Resources.Load<AudioClip>("Audio/FX/Landing");
        selectSound = Resources.Load<AudioClip>("Audio/FX/Bell");

        // Reproduce la música de fondo
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("No se encontró invierno.mp3 en Resources/Audio");
        }

        UpdateSelectorPosition();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex = (currentIndex + 1) % menuOptions.Count;
            UpdateSelectorPosition();
            PlaySound(changeOptionSound);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex = (currentIndex - 1 + menuOptions.Count) % menuOptions.Count;
            UpdateSelectorPosition();
            PlaySound(changeOptionSound);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaySound(selectSound);
            OnSelectOption();
        }
    }

    private void UpdateSelectorPosition()
    {
        if (menuOptions.Count == 0 || selector == null) return;
        Vector2 optionPosition = menuOptions[currentIndex].anchoredPosition;
        selector.anchoredPosition = new Vector2(
            optionPosition.x - 100f,
            optionPosition.y
        );
    }

    private void OnSelectOption()
    {
        string selectedButtonName = menuOptions[currentIndex].name;

        switch (selectedButtonName)
        {
            case "Play":
                FindAnyObjectByType<SceneController>().LoadGameScene("Playa");
                break;
            case "Exit":
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"No se encontró el archivo de audio: {clip.name}");
        }
    }
}