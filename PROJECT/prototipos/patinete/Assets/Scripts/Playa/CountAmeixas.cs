using TMPro;
using UnityEngine;

public class PlayerStatsDisplay : MonoBehaviour
{
    public GameManager game;
    public TMP_Text statsText;

    void Update()
    {
        statsText.text = game.collectedAmeixas.ToString();
    }
}