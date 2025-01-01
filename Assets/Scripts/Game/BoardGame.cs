using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BoardGame : MonoBehaviour
{
    [SerializeField] private int maxMovesPerTurn = 3;
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private PlayerPiece[] players;
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

        foreach (PlayerPiece player in players)
        {
            HexCell startCell = null;
            while (startCell == null || IsCellOccupiedOrAdjacent(startCell))
            {
                startCell = hexGrid.GetRandomCell();
            }
            player.Initialize(new Player($"Player {player.Type}"), 100, 10, startCell, player.Type);
            // Ensure initial position is above the hex
            player.transform.position += Vector3.up * 0.5f;
        }
        remainingMoves = maxMovesPerTurn;
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
        movesText.text = $"Player {currentPlayerIndex + 1} Moves: {remainingMoves}";
        healthText.text = $"Health: {currentPlayer.Health}/100";
        powerText.text = $"Power: {currentPlayer.GetTotalAttack()} ({currentPlayer.Attack} + {currentPlayer.GetTotalAttack() - currentPlayer.Attack})";
    }

    public void MovePlayerPiece(HexCell targetCell)
    {
        if (remainingMoves > 0 && IsValidMove(targetCell))
        {
            PlayerPiece currentPlayer = players[currentPlayerIndex];
            currentPlayer.MoveTo(targetCell);

            if (targetCell.CurrentCollectable != null)
            {
                CollectCollectable(currentPlayer, targetCell.CollectCollectable());
                cellsWithCollectables.Remove(targetCell);
                CheckAndRefillCollectables();
            }

            remainingMoves--;
            UpdateUI();
            HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);

            if (remainingMoves == 0)
            {
                EndTurn();
            }
        }
    }

    public bool IsValidMove(HexCell targetCell)
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
        // Remove temporary effects from current player
        players[currentPlayerIndex].OnTurnEnd();

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        remainingMoves = maxMovesPerTurn;
        UpdateUI();
        HighlightPossibleMoves(players[currentPlayerIndex].CurrentCell);
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
            if (!IsCellOccupied(cell) && cell.CurrentCollectable == null)
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
}