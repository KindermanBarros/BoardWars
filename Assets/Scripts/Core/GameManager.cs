using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private BoardGame boardGame;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private int winningScore = 2; // Changed to 2 for best-of-3
    [SerializeField] private Canvas pauseMenuCanvas;

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
        InitializePlayers();

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (boardGame == null)
            boardGame = FindObjectOfType<BoardGame>();
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        StartGame();
    }

    private void InitializePlayers()
    {
        Player1 = new Player("Player 1");
        Player2 = new Player("Player 2");
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        boardGame.InitializeGame(Player1, Player2);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0;
            if (pauseMenuCanvas != null)
            {
                pauseMenuCanvas.gameObject.SetActive(true);
            }
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1;
            if (pauseMenuCanvas != null)
            {
                pauseMenuCanvas.gameObject.SetActive(false);
            }
        }
    }

    public void CheckWinCondition()
    {
        if (Player1.Wins >= winningScore)
        {
            EndGame(Player1);
        }
        else if (Player2.Wins >= winningScore)
        {
            EndGame(Player2);
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
    }

    public void RestartGame()
    {
        Player1.ResetWins();
        Player2.ResetWins();
        CurrentState = GameState.Playing;

        if (boardGame != null)
        {
            boardGame.enabled = true;
            boardGame.InitializeGame(Player1, Player2);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing)
                PauseGame();
            else if (CurrentState == GameState.Paused)
                ResumeGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
}
