using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BoardGame : MonoBehaviour
{
    [SerializeField] private int maxMovesPerTurn = 3;
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] public PlayerPiece[] players;
    [SerializeField] private Material possibleMoveHighlightMaterial;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text powerText;

    [SerializeField] private CollectableType[] collectableTypes;
    [SerializeField] private float collectableSpawnRate = 0.2f;
    [SerializeField] private float collectableRefillThreshold = 0.1f;

    private int currentPlayerIndex = 0;
    private int remainingMoves;
    private PlayerPiece draggingPlayerPiece;
    private Vector3 dragOffset;
    private Vector3 dragStartPosition;
    private Camera mainCamera;
    private bool isDragging = false;

    private List<HexCell> cellsWithCollectables = new List<HexCell>();
    private int totalCollectables;

    private bool resolvingBattle = false;

    public int[] playerWins;
    private bool gameEnded = false;

    public static BoardGame Instance { get; private set; }
    public Player CurrentPlayer => players[currentPlayerIndex].Player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("BoardGame starting. Number of players: " + players.Length);

        playerWins = new int[players.Length];

        InitializePlayers();
        UpdateUI();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
        UpdateCameraTarget();

        Debug.Log($"Current player position: {players[currentPlayerIndex].transform.position}");
        Debug.Log($"Remaining moves: {remainingMoves}");

        SpawnInitialCollectables();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            HandleDragging();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    private void OnDestroy()
    {
        if (MouseController.Instance != null)
        {
            Instance = null;
        }
    }

    private void InitializePlayers()
    {
        if (players == null || players.Length == 0)
        {
            Debug.LogError("No players assigned to BoardGame!");
            return;
        }

        List<HexCell> usedCells = new List<HexCell>();
        foreach (PlayerPiece player in players)
        {
            HexCell startCell = null;
            while (startCell == null || IsCellOccupiedOrAdjacent(startCell) || usedCells.Any(c => IsAdjacentTo(c, startCell)))
            {
                startCell = hexGrid.GetRandomCell();
            }
            usedCells.Add(startCell);
            player.Initialize(new Player($"Player {player.Type}"), 100, 10, startCell, player.Type);
            player.transform.position = startCell.transform.position + Vector3.up * 0.5f;
        }
        remainingMoves = maxMovesPerTurn;
    }

    private bool IsAdjacentTo(HexCell cell1, HexCell cell2)
    {
        return cell1.Neighbors.Contains(cell2);
    }

    private bool IsCellOccupiedOrAdjacent(HexCell cell)
    {
        if (IsCellOccupied(cell))
        {
            return true;
        }

        foreach (HexCell neighbor in cell.Neighbors)
        {
            if (IsCellOccupied(neighbor))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsCellOccupied(HexCell cell)
    {
        foreach (PlayerPiece player in players)
        {
            if (player.CurrentCell == cell)
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateUI()
    {
        PlayerPiece currentPlayer = players[currentPlayerIndex];
        if (!gameEnded)
        {
            string winText = $"(Score: {players[0].Player.Wins}-{players[1].Player.Wins})";
            movesText.text = $"Player {currentPlayerIndex + 1} Moves: {remainingMoves} {winText}";
            Debug.Log($"Updated UI with scores: {winText}");
        }
        healthText.text = $"Health: {currentPlayer.Health}/100";
        powerText.text = $"Power: {currentPlayer.GetTotalAttack()} ({currentPlayer.Attack} + {currentPlayer.GetTotalAttack() - currentPlayer.Attack})";
    }

    public void DisplayGameOver(Player winner)
    {
        gameEnded = true;
        movesText.text = $"{winner.Name} WINS THE GAME! ({players[0].Player.Wins}-{players[1].Player.Wins})";
        healthText.text = "Game Over";
        powerText.text = "Press R to Restart";
    }

    private void HandleMovement(HexCell targetCell, PlayerPiece movingPlayer)
    {
        if (!IsValidMove(targetCell)) return;

        // Clear previous highlights
        hexGrid.ClearHighlights();

        // Execute move
        movingPlayer.MoveTo(targetCell);

        // Handle collectible collection
        if (targetCell.CurrentCollectable != null)
        {
            CollectCollectable(movingPlayer, targetCell.CollectCollectable());
            cellsWithCollectables.Remove(targetCell);
            CheckAndRefillCollectables();
        }

        // Check for battle after movement
        PlayerPiece adjacentEnemy = FindAdjacentEnemy(movingPlayer);
        remainingMoves--;

        UpdateUI();

        // Handle battle or turn end
        if (adjacentEnemy != null)
        {
            resolvingBattle = true;
            ResolveBattle(movingPlayer, adjacentEnemy);
        }
        else if (remainingMoves <= 0)
        {
            EndTurn();
        }
        else
        {
            // Highlight possible moves for the current player
            HighlightPossibleMoves(movingPlayer.CurrentCell);
        }
    }

    private bool ApplyKnockback(PlayerPiece knockedPiece, PlayerPiece source, int distance)
    {
        Debug.Log($"Applying knockback to {knockedPiece.name} from {source.name}, distance: {distance}");

        HexCell knockbackCell = hexGrid.GetCellInDirection(knockedPiece.CurrentCell, source.CurrentCell, distance);

        if (knockbackCell == null)
        {
            Debug.Log($"Ring out! {knockedPiece.name} knocked off board");
            HandleRoundWin(source);
            return false;
        }

        knockedPiece.MoveTo(knockbackCell);

        if (knockbackCell.CurrentCollectable != null)
        {
            CollectCollectable(knockedPiece, knockbackCell.CollectCollectable());
            cellsWithCollectables.Remove(knockbackCell);
            CheckAndRefillCollectables();
        }

        return true;
    }

    private void ResolveBattle(PlayerPiece attacker, PlayerPiece defender)
    {
        Battle.BattleResult result = Battle.ResolveBattle(attacker, defender);

        // Apply knockback first
        if (!ApplyKnockback(result.Loser, result.Winner, 1))
        {
            return; // Battle ended due to ring-out
        }

        // Apply damage after knockback
        result.Loser.TakeDamage(result.DamageDealt);
        if (result.Loser.Health <= 0)
        {
            HandleRoundWin(result.Winner);
            return;
        }

        resolvingBattle = false;
        UpdateUI();

        if (remainingMoves <= 0)
        {
            EndTurn();
        }
        else
        {
            // Highlight possible moves for the current player
            HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
        }
    }

    private void StartTurn()
    {
        PlayerPiece currentPlayer = players[currentPlayerIndex];
        PlayerPiece adjacentEnemy = FindAdjacentEnemy(currentPlayer);

        if (adjacentEnemy != null)
        {
            Debug.Log($"Start turn check: {currentPlayer.name} is adjacent to {adjacentEnemy.name}");
            if (!ApplyKnockback(currentPlayer, adjacentEnemy, 2))
            {
                return;
            }
        }

        remainingMoves = maxMovesPerTurn;
        UpdateUI();
        HighlightPossibleMoves(currentPlayer.CurrentCell);
    }

    public void MovePlayerPiece(HexCell targetCell)
    {
        if (remainingMoves > 0)
        {
            HandleMovement(targetCell, players[currentPlayerIndex]);
        }
    }

    private PlayerPiece FindAdjacentEnemy(PlayerPiece currentPlayer)
    {
        foreach (HexCell neighbor in currentPlayer.CurrentCell.Neighbors)
        {
            foreach (PlayerPiece player in players)
            {
                if (player != currentPlayer && player.CurrentCell == neighbor)
                {
                    return player;
                }
            }
        }
        return null;
    }

    private void HandleRoundWin(PlayerPiece winner)
    {
        HandleWin(winner);
    }

    public void HandleWin(PlayerPiece winner)
    {
        winner.Player.AddWin();
        Debug.Log($"Win recorded! Score is now: {players[0].Player.Wins}-{players[1].Player.Wins}");

        if (winner.Player.Wins >= 2)
        {
            GameManager.Instance.CheckWinCondition();
        }
        else
        {
            ResetRound();
        }

        UpdateUI();
    }

    private void ResetRound()
    {
        Debug.Log("=== ROUND RESET ===");
        remainingMoves = maxMovesPerTurn;

        foreach (var cell in cellsWithCollectables.ToList())
        {
            if (cell?.CurrentCollectable != null)
            {
                Destroy(cell.CurrentCollectable.gameObject);
            }
        }
        cellsWithCollectables.Clear();

        List<HexCell> startCells = new List<HexCell>();
        foreach (PlayerPiece player in players)
        {
            HexCell startCell = null;
            while (startCell == null || IsCellOccupiedOrAdjacent(startCell) || startCells.Contains(startCell))
            {
                startCell = hexGrid.GetRandomCell();
            }
            startCells.Add(startCell);

            Player currentPlayer = player.Player;
            player.Initialize(currentPlayer, 100, 10, startCell, player.Type);
            player.PlayResetAnimation();
        }

        currentPlayerIndex = 0;
        remainingMoves = maxMovesPerTurn;
        resolvingBattle = false;
        isDragging = false;
        draggingPlayerPiece = null;

        Debug.Log($"Reset moves to: {remainingMoves}");
        UpdateUI();
        UpdateCameraTarget();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
        SpawnInitialCollectables();
    }

    private bool IsValidMove(HexCell targetCell)
    {
        if (targetCell == null)
        {
            Debug.LogError("IsValidMove: targetCell is null");
            return false;
        }

        PlayerPiece currentPlayer = players[currentPlayerIndex];
        HexCell currentCell = currentPlayer.CurrentCell;

        if (currentCell == null || targetCell == currentCell || !hexGrid.IsAdjacent(currentCell, targetCell))
        {
            return false;
        }

        foreach (PlayerPiece player in players)
        {
            if (player.CurrentCell == targetCell)
            {
                return false;
            }
        }

        return true;
    }

    private void EndTurn()
    {
        players[currentPlayerIndex].OnTurnEnd();
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        remainingMoves = maxMovesPerTurn;

        StartTurn(); // Add this to check for knockback at start of next turn
        UpdateCameraTarget();
    }

    private void UpdateCameraTarget()
    {
        if (cameraController != null && players != null && players.Length > currentPlayerIndex)
        {
            cameraController.SetTarget(players[currentPlayerIndex].transform);
        }
    }

    public void HighlightPossibleMoves(HexCell currentCell)
    {
        hexGrid.ClearHighlights();
        if (currentCell != null)
        {
            hexGrid.HighlightPossibleMoves(currentCell, possibleMoveHighlightMaterial);
        }
    }

    public void ClearHighlights()
    {
        hexGrid.ClearHighlights();
    }

    public HexCell GetClosestCell(Vector3 position)
    {
        return hexGrid.GetClosestCell(position);
    }

    public void OnCellClicked(HexCell cell)
    {
        if (cell != null && IsValidMove(cell))
        {
            MovePlayerPiece(cell);
        }
    }

    private void HandleMouseDown()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlayerPiece piece = hit.collider.GetComponent<PlayerPiece>();
            if (piece != null && piece == players[currentPlayerIndex])
            {
                isDragging = true;
                draggingPlayerPiece = piece;
                dragStartPosition = piece.transform.position;
                dragOffset = dragStartPosition - GetMouseWorldPosition();
            }
            else
            {
                HexCell cell = hit.collider.GetComponent<HexCell>();
                if (cell != null && IsValidMove(cell))
                {
                    MovePlayerPiece(cell);
                }
            }
        }
    }

    private void HandleDragging()
    {
        if (draggingPlayerPiece != null)
        {
            Vector3 targetPos = GetMouseWorldPosition() + dragOffset;
            targetPos.y = dragStartPosition.y;
            draggingPlayerPiece.transform.position = targetPos;
        }
    }

    private void HandleMouseUp()
    {
        if (draggingPlayerPiece != null)
        {
            Vector3 finalPosition = GetMouseWorldPosition() + dragOffset;
            HexCell targetCell = hexGrid.GetClosestCell(finalPosition);

            if (IsValidMove(targetCell))
            {
                MovePlayerPiece(targetCell);
            }
            else
            {
                draggingPlayerPiece.transform.position = dragStartPosition;
            }

            draggingPlayerPiece = null;
        }
        isDragging = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.up * 0.5f);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    private void SpawnInitialCollectables()
    {
        int totalCells = hexGrid.Width * hexGrid.Height;
        int desiredCollectables = Mathf.RoundToInt(totalCells * collectableSpawnRate);

        while (cellsWithCollectables.Count < desiredCollectables)
        {
            HexCell cell = hexGrid.GetRandomCell();
            if (!IsCellOccupied(cell) && !IsCellOccupiedOrAdjacent(cell) && cell.CurrentCollectable == null)
            {
                SpawnCollectable(cell);
            }
        }
        totalCollectables = cellsWithCollectables.Count;
    }

    private void SpawnCollectable(HexCell cell)
    {
        if (collectableTypes == null || collectableTypes.Length == 0)
        {
            Debug.LogError("No CollectableTypes assigned to BoardGame!");
            return;
        }

        CollectableType randomType = collectableTypes[Random.Range(0, collectableTypes.Length)];
        if (randomType == null || randomType.visualPrefab == null)
        {
            Debug.LogError("Invalid CollectableType or missing visualPrefab!");
            return;
        }

        GameObject collectableObject = Instantiate(randomType.visualPrefab);
        var collectable = collectableObject.AddComponent<Collectable>();

        Collectable.Tier tier = (Collectable.Tier)Random.Range(0, 3);
        collectable.Initialize(randomType, tier);
        cell.SetCollectable(collectable);
        cellsWithCollectables.Add(cell);
    }

    private void CheckAndRefillCollectables()
    {
        float currentRatio = (float)cellsWithCollectables.Count / totalCollectables;
        if (currentRatio <= collectableRefillThreshold)
        {
            SpawnInitialCollectables();
        }
    }

    private void CollectCollectable(PlayerPiece player, Collectable collectable)
    {
        int value = collectable.GetValue();
        switch (collectable.CollectableType)
        {
            case CollectableTypeEnum.ExtraMove:
                remainingMoves += value;
                break;
            case CollectableTypeEnum.ExtraAttack:
                player.AddTemporaryAttack(value);
                break;
            case CollectableTypeEnum.Health:
                player.Heal(value);
                break;
        }
        Destroy(collectable.gameObject);
        UpdateUI();
    }

    public void ResetGame()
    {
        gameEnded = false;
        enabled = true;
        remainingMoves = maxMovesPerTurn;
        foreach (PlayerPiece player in players)
        {
            player.Player.ResetWins();
        }
        ResetRound();
    }

    public void InitializeGame(Player player1, Player player2)
    {
        if (players == null || players.Length < 2)
        {
            Debug.LogError("Not enough player pieces assigned!");
            return;
        }

        foreach (PlayerPiece player in players)
        {
            HexCell startCell = null; while (startCell == null || IsCellOccupiedOrAdjacent(startCell))
            {
                startCell = hexGrid.GetRandomCell();
            }

            Player playerRef = player.Type == PlayerPiece.PlayerType.Player1 ? player1 : player2;
            player.Initialize(playerRef, 100, 10, startCell, player.Type);
            player.transform.position += Vector3.up * 0.5f;
        }

        remainingMoves = maxMovesPerTurn;
        currentPlayerIndex = 0;
        gameEnded = false;
        resolvingBattle = false;
        isDragging = false;
        draggingPlayerPiece = null;

        Debug.Log($"Initialized moves to: {remainingMoves}");
        UpdateUI();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
        SpawnInitialCollectables();
    }
}