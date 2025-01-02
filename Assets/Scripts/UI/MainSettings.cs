using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MainSettings : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject boardGame;
    [SerializeField] private TMP_Dropdown player1Dropdown;
    [SerializeField] private TMP_Dropdown player2Dropdown;

    private void Start()
    {
        InitializeUI();
        gameUI.SetActive(false);
        boardGame.SetActive(false);
    }

    private void InitializeUI()
    {
        player1Dropdown.ClearOptions();
        player2Dropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Default (Balanced)"),
            new TMP_Dropdown.OptionData("Fast Movement (More Moves)"),
            new TMP_Dropdown.OptionData("Strong Attack (High Damage)"),
            new TMP_Dropdown.OptionData("High Health (Tanky)")
        };

        player1Dropdown.options = options;
        player2Dropdown.options = new List<TMP_Dropdown.OptionData>(options);

        // Set different default selections
        player1Dropdown.value = 0;  // Default for Player 1
        player2Dropdown.value = 2;  // Strong Attack for Player 2
    }

    public void StartGame()
    {
        CharacterVariant p1Variant = (CharacterVariant)player1Dropdown.value;
        CharacterVariant p2Variant = (CharacterVariant)player2Dropdown.value;

        Debug.Log($"Starting game with P1: {p1Variant}, P2: {p2Variant}");

        Player player1 = new Player("Player 1", p1Variant);
        Player player2 = new Player("Player 2", p2Variant);

        if (BoardGame.Instance != null)
        {
            mainMenuCanvas.SetActive(false);
            settingsCanvas.SetActive(false);
            gameUI.SetActive(true);
            boardGame.SetActive(true);

            BoardGame.Instance.ResetAndInitialize(player1, player2);
        }
        else
        {
            Debug.LogError("BoardGame instance not found!");
        }
    }

    public void BackToMain()
    {
        settingsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    public void OpenSettings()
    {
        mainMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }
}
