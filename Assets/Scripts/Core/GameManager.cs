using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private BoardGame boardGame;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private Slider audioSlider;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject gameUICanvas;

    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }
    public GameState CurrentState { get; private set; }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;

        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }

        if (audioSlider != null)
        {
            audioSlider.onValueChanged.AddListener(SetAudioVolume);
        }
    }

    private void Start()
    {
        if (boardGame == null)
            boardGame = FindObjectOfType<BoardGame>();
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        CurrentState = GameState.Playing;

        if (audioSlider != null)
        {
            audioSlider.value = AudioManager.Instance.GetVolume();
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }

    public void InitializePlayers()
    {
        Player1 = new Player("Player 1");
        Player2 = new Player("Player 2");

        if (boardGame != null)
        {
            boardGame.InitializeGame(Player1, Player2);
        }
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }
        if (gameUICanvas != null)
        {
            gameUICanvas.SetActive(true);
        }
        if (boardGame != null)
        {
            boardGame.enabled = true;
            boardGame.ResetAndInitialize(Player1, Player2);
        }
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0;
            if (settingsMenu != null)
            {
                settingsMenu.SetActive(true);
            }
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1;
            if (settingsMenu != null)
            {
                settingsMenu.SetActive(false);
            }
        }
    }

    public void CheckWinCondition(Player winner)
    {
        if (winner.Wins >= 2)
        {
            EndGame(winner);
        }
    }

    public bool IsGameOver() => CurrentState == GameState.GameOver;

    private void EndGame(Player winner)
    {
        CurrentState = GameState.GameOver;
        Debug.Log($"Game Over! {winner.Name} wins the match with {winner.Wins} wins!");

        if (boardGame != null)
        {
            boardGame.enabled = false;
            boardGame.DisplayGameOver(winner);
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.transform.SetAsLastSibling();
            gameOverCanvas.SetActive(true);
            winnerText.text = $"{winner.Name} wins!";
            replayButton.onClick.AddListener(RestartGame);
            backToMenuButton.onClick.AddListener(BackToMainMenu);
        }
    }

    public void BackToMainMenu()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }
        if (boardGame != null)
        {
            boardGame.enabled = false;
        }
        CurrentState = GameState.MainMenu;
    }

    public void RestartGame()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }
        if (boardGame != null)
        {
            CharacterVariant p1Variant = Player1?.Variant ?? CharacterVariant.Default;
            CharacterVariant p2Variant = Player2?.Variant ?? CharacterVariant.Default;

            Player1 = new Player("Player 1", p1Variant);
            Player2 = new Player("Player 2", p2Variant);

            CurrentState = GameState.Playing;
            boardGame.enabled = true;
            boardGame.ResetAndInitialize(Player1, Player2);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
            else if (settingsMenu != null && settingsMenu.activeSelf)
            {
                settingsMenu.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void ToggleSettingsMenu()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
        }
    }

    public void CloseSettingsMenu()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
            ResumeGame();
        }
    }

    private void SetAudioVolume(float volume)
    {
        AudioManager.Instance.SetVolume(volume);
    }
}
