using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class BoardGame : MonoBehaviour
{
    [SerializeField] private int maxMovesPerTurn = 3;
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] public PlayerPiece[] players;
    [SerializeField] private Material possibleMoveHighlightMaterial;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Canvas EndScreen;

    [SerializeField] private TMP_Text currentPlayerText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text winsText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private string scoreTextFormat = "(Score: {0}-{1})";
    [SerializeField] private TMP_Text rerollCountText;

    [SerializeField] private CollectableType[] collectableTypes;
    [SerializeField] private float collectableSpawnRate = 0.2f;
    [SerializeField] private float collectableRefillThreshold = 0.1f;
    [SerializeField] private GameObject diceRollUIPanel;
    [SerializeField] private RawImage[] player1DiceImages;
    [SerializeField] private RawImage[] player2DiceImages;
    [SerializeField] private TMP_Text[] player1DiceNumberTexts;
    [SerializeField] private TMP_Text[] player2DiceNumberTexts;
    [SerializeField] private TMP_Text rerollIndicatorText;

    private int currentPlayerIndex = 0;
    private int remainingMoves;
    private PlayerPiece draggingPlayerPiece;
    private Vector3 dragOffset;
    private Vector3 dragStartPosition;
    private Camera mainCamera;
    private bool isDragging = false;

    private List<HexCell> cellsWithCollectables = new List<HexCell>();
    private int totalCollectables;

    public bool isAttacking = false;

    public int[] playerWins;
    private bool gameEnded = false;

    public static BoardGame Instance { get; private set; }
    public Player CurrentPlayer => players[currentPlayerIndex].Player;

    private bool isDiceRolling = false;

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
        playerWins = new int[players.Length];
    }

    public void EnableCamera()
    {
        if (cameraController != null)
        {
            cameraController.enabled = true;
            UpdateCameraTarget();
        }
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

            player.Initialize(player.Player, startCell, player.Type);
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

    public bool IsCellOccupied(HexCell cell)
    {
        if (cell == null) return false;
        return players.Any(p => p.CurrentCell == cell);
    }

    public void UpdateUI()
    {
        PlayerPiece currentPlayer = players[currentPlayerIndex];
        if (!gameEnded)
        {
            string winText = string.Format(scoreTextFormat, playerWins[0], playerWins[1]);
            string variantText = $"[{currentPlayer.Player.Variant}]";
            currentPlayerText.text = $"Player {currentPlayerIndex + 1} {variantText}";
            movesText.text = $" Moves: {remainingMoves}";
            winsText.text = $"{winText}";
            healthText.text = $"Health: {currentPlayer.Health}/{currentPlayer.Player.Health}";
            powerText.text = $"Power: {currentPlayer.GetTotalAttack()}";
            rerollCountText.text = $"Rerolls: {currentPlayer.GetExtraDiceRolls()}";
        }
    }

    public void DisplayGameOver(Player winner)
    {
        gameEnded = true;
        string finalScore = string.Format(scoreTextFormat, playerWins[0], playerWins[1]);
        movesText.text = $"{winner.Name} WINS THE GAME! {finalScore}";
        healthText.text = "Game Over";
        powerText.text = "Press R to Restart";
        EndScreen.enabled = true;
    }

    private void HandleMovement(HexCell targetCell, PlayerPiece movingPlayer)
    {
        if (targetCell == null)
        {
            Debug.LogError("HandleMovement: targetCell is null");
            return;
        }

        if (movingPlayer == null)
        {
            Debug.LogError("HandleMovement: movingPlayer is null");
            return;
        }

        hexGrid.ClearHighlights();

        movingPlayer.MoveTo(targetCell);

        if (targetCell.CurrentCollectable != null)
        {
            CollectCollectable(movingPlayer, targetCell.CollectCollectable());
            cellsWithCollectables.Remove(targetCell);
            CheckAndRefillCollectables();
        }

        PlayerPiece adjacentEnemy = FindAdjacentEnemy(movingPlayer);
        remainingMoves--;

        UpdateUI();

        if (adjacentEnemy != null)
        {
            isAttacking = true;
            ResolveBattle(movingPlayer, adjacentEnemy);
            isAttacking = false;
        }
        else if (remainingMoves <= 0)
        {
            EndTurn();
        }
        else
        {
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
        AudioManager.Instance.PlayAttack();
        Battle.BattleResult result = Battle.ResolveBattle(attacker, defender);
        if (!ApplyKnockback(result.Loser, result.Winner, 1))
        {
            return;
        }
        result.Loser.TakeDamage(result.DamageDealt, attacker);
        if (result.Loser.Health <= 0)
        {
            HandleRoundWin(result.Winner);
            return;
        }
        UpdateUI();
        if (remainingMoves <= 0)
        {
            EndTurn();
        }
        else
        {
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
        remainingMoves = players[currentPlayerIndex].Player.Movement;
        UpdateUI();
        HighlightPossibleMoves(currentPlayer.CurrentCell);
    }

    public void MovePlayerPiece(HexCell targetCell)
    {
        if (remainingMoves > 0 && !isDiceRolling)
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
        int winnerIndex = System.Array.IndexOf(players, winner);
        if (winnerIndex >= 0)
        {
            playerWins[winnerIndex]++;
            winner.Player.AddWin();
            Debug.Log($"Win recorded for player {winnerIndex + 1}! Score is now: {playerWins[0]}-{playerWins[1]}");

            if (playerWins[winnerIndex] >= 2)
            {
                GameManager.Instance.CheckWinCondition(winner.Player);
            }
            else
            {
                ResetRound();
            }
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

            player.Initialize(currentPlayer, startCell, player.Type);
            player.PlayResetAnimation();
        }

        currentPlayerIndex = 0;
        remainingMoves = maxMovesPerTurn;
        isDragging = false;
        draggingPlayerPiece = null;
        EndScreen.enabled = false;


        UpdateUI();
        UpdateCameraTarget();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
        SpawnInitialCollectables();
    }

    private bool IsValidMove(HexCell targetCell)
    {
        if (targetCell == null) return false;

        PlayerPiece currentPlayer = players[currentPlayerIndex];
        HexCell currentCell = currentPlayer.CurrentCell;

        if (currentCell == null || targetCell == currentCell) return false;

        if (!hexGrid.IsAdjacent(currentCell, targetCell)) return false;

        return !players.Any(p => p.CurrentCell == targetCell);
    }

    private void EndTurn()
    {
        players[currentPlayerIndex].OnTurnEnd();
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        remainingMoves = maxMovesPerTurn;

        StartTurn();
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
        if (isDiceRolling) return;

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
        if (isDiceRolling) return;

        if (draggingPlayerPiece != null)
        {
            Vector3 targetPos = GetMouseWorldPosition() + dragOffset;
            targetPos.y = dragStartPosition.y;
            draggingPlayerPiece.transform.position = targetPos;
        }
    }

    private void HandleMouseUp()
    {
        if (isDiceRolling) return;

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
        if (mainCamera == null) return Vector3.zero;

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
            return;
        }

        CollectableType randomType = collectableTypes[Random.Range(0, collectableTypes.Length)];
        if (randomType == null || randomType.visualPrefab == null)
        {
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
        if (collectable == null) return;

        int value = collectable.GetValue();
        switch (collectable.CollectableType)
        {
            case CollectableTypeEnum.ExtraMove:
                remainingMoves += value;
                Debug.Log($"Added {value} moves. Total moves now: {remainingMoves}");
                break;
            case CollectableTypeEnum.ExtraAttack:
                player.AddTemporaryAttack(value);
                break;
            case CollectableTypeEnum.Health:
                player.Heal(value);
                break;
            case CollectableTypeEnum.ExtraDice:
                player.AddExtraDiceRoll(value);
                break;
        }

        Destroy(collectable.gameObject);
        AudioManager.Instance?.PlayCollect();
        UpdateUI();
    }

    public void ResetGame()
    {
        gameEnded = false;
        enabled = true;
        remainingMoves = maxMovesPerTurn;
        EndScreen.enabled = false;

        foreach (PlayerPiece player in players)
        {
            player.Player.ResetWins();
        }
        ResetRound();
    }

    public void InitializeGame(Player player1, Player player2)
    {
        if (players == null || players.Length < 2) return;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        foreach (PlayerPiece player in players)
        {
            HexCell startCell = null;
            while (startCell == null || IsCellOccupiedOrAdjacent(startCell))
            {
                startCell = hexGrid.GetRandomCell();
            }

            Player playerRef = player.Type == PlayerPiece.PlayerType.Player1 ? player1 : player2;
            player.Initialize(playerRef, startCell, player.Type);
        }

        remainingMoves = players[currentPlayerIndex].Player.Movement;
        currentPlayerIndex = 0;
        gameEnded = false;
        EndScreen.enabled = false;
        UpdateUI();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
        SpawnInitialCollectables();
        UpdateCameraTarget();
    }

    public void ResetAndInitialize(Player player1, Player player2)
    {
        foreach (var cell in cellsWithCollectables.ToList())
        {
            if (cell?.CurrentCollectable != null)
            {
                Destroy(cell.CurrentCollectable.gameObject);
            }
        }
        cellsWithCollectables.Clear();

        currentPlayerIndex = 0;
        gameEnded = false;
        isDragging = false;
        playerWins = new int[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            HexCell startCell = null;
            while (startCell == null || IsCellOccupiedOrAdjacent(startCell))
            {
                startCell = hexGrid.GetRandomCell();
            }

            Player playerRef = i == 0 ? player1 : player2;
            players[i].Initialize(playerRef, startCell, players[i].Type);
        }

        remainingMoves = players[currentPlayerIndex].Player.Movement;
        mainCamera = Camera.main;
        EndScreen.enabled = false;
        UpdateUI();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
        SpawnInitialCollectables();
        UpdateCameraTarget();
        EnablePlayerDragging();
    }

    private void EnablePlayerDragging()
    {
        foreach (PlayerPiece player in players)
        {
            player.enabled = true;
        }
    }

    private IEnumerator AnimateDiceRoll(int[] rolls, RawImage[] diceImages, TMP_Text[] diceNumberTexts)
    {
        isDiceRolling = true;
        diceRollUIPanel.SetActive(true);
        bool isSpinning = true;
        StartCoroutine(SpinDice(diceImages, () => isSpinning));

        float rollDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < rollDuration)
        {
            for (int i = 0; i < diceNumberTexts.Length; i++)
            {
                if (i >= 0 && i < diceNumberTexts.Length)
                {
                    diceNumberTexts[i].text = Random.Range(1, 21).ToString();
                }
                else
                {
                    Debug.LogWarning("Index out of bounds: " + i);
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < rolls.Length; i++)
        {
            if (i >= 0 && i < diceNumberTexts.Length)
            {
                diceNumberTexts[i].text = rolls[i].ToString();
                diceImages[i].transform.rotation = Quaternion.identity;
            }
            else
            {
                Debug.LogWarning("Index out of bounds: " + i);
            }
        }

        isSpinning = false;
        yield return new WaitForSeconds(2f);

        diceRollUIPanel.SetActive(false);
        isDiceRolling = false;
    }

    private IEnumerator SpinDice(RawImage[] diceImages, System.Func<bool> isSpinning)
    {
        Vector3[] rotationOffsets = new Vector3[diceImages.Length];
        for (int i = 0; i < diceImages.Length; i++)
        {
            rotationOffsets[i] = new Vector3(0, 0, Random.Range(-10f, 10f));
        }

        while (isSpinning())
        {
            for (int i = 0; i < diceImages.Length; i++)
            {
                diceImages[i].transform.Rotate(rotationOffsets[i] + Vector3.forward * 360 * Time.deltaTime);
            }
            yield return null;
        }
    }

    public void ShowDiceRolls(int[] rolls, bool isPlayer1)
    {
        if (isPlayer1)
        {
            StartCoroutine(AnimateDiceRoll(rolls, player1DiceImages, player1DiceNumberTexts));
        }
        else
        {
            StartCoroutine(AnimateDiceRoll(rolls, player2DiceImages, player2DiceNumberTexts));
        }
    }

    public void ShowRerollText(int rerollCount)
    {
        if (rerollIndicatorText != null)
        {
            rerollIndicatorText.text = $"Rerolled {rerollCount} dice{(rerollCount > 1 ? "s" : "")}";
            StartCoroutine(ClearRerollText());
        }
    }

    private IEnumerator ClearRerollText()
    {
        yield return new WaitForSeconds(2f);
        if (rerollIndicatorText != null)
        {
            rerollIndicatorText.text = "";
        }
    }
}