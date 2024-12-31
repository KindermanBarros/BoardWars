using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController[] players;
    public Text currentPlayerText;
    private int currentPlayerIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartTurn();
    }

    public void EndTurn()
    {
        players[currentPlayerIndex].EndTurn();
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        StartTurn();
    }

    private void StartTurn()
    {
        players[currentPlayerIndex].StartTurn();
        UpdateCurrentPlayerText();
    }

    public bool IsPlayerTurn(PlayerController player)
    {
        return players[currentPlayerIndex] == player;
    }

    private void UpdateCurrentPlayerText()
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = $"Current Player: Player {players[currentPlayerIndex].GetComponent<PlayerPiece>().PlayerNumber}";
        }
    }
}