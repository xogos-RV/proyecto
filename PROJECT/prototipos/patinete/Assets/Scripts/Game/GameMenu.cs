using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    GameObject Play;
    void Start()
    {
        Play = GameObject.Find("Play");
        Play.GetComponent<Button>();
    }

    void Update()
    {

    }
}
