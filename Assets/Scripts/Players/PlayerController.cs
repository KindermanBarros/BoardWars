using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public HexGrid hexGrid;
    public HexCell currentCell;
    public float moveSpeed = 5f;
    public int maxMovesPerTurn = 3;
    public Text movesText;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastValidPosition;
    private int availableMoves;
    private bool isTurn = false;

    private void Start()
    {
        if (hexGrid == null)
        {
            Debug.LogError("HexGrid is not assigned in the PlayerController script.");
            return;
        }

        InitializePlayerPosition();
        AddColliderIfMissing();
        MouseController.Instance.OnLeftMouseClick += HandleLeftMouseClick;

        availableMoves = maxMovesPerTurn;
        UpdateMovesText();
    }

    private void OnDestroy()
    {
        MouseController.Instance.OnLeftMouseClick -= HandleLeftMouseClick;
    }

    private void Update()
    {
        if (isDragging && isTurn)
        {
            DragPlayer();
        }
    }

    private void HandleLeftMouseClick(RaycastHit hit)
    {
        if (hit.collider.gameObject == gameObject && isTurn)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();
        }
    }

    private void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            SnapToClosestGridCell();
        }
    }

    private void DragPlayer()
    {
        Vector3 mousePosition = GetMouseWorldPosition() + offset;
        transform.position = new Vector3(mousePosition.x, transform.position.y, mousePosition.z);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void SnapToClosestGridCell()
    {
        if (hexGrid == null) return;

        HexCell closestCell = hexGrid.GetClosestCell(transform.position);
        if (closestCell != null && IsAdjacent(currentCell, closestCell) && FindPlayerAtCell(closestCell) == null)
        {
            MoveToCell(closestCell);
        }
        else
        {
            transform.position = lastValidPosition;
        }
    }

    private void MoveToCell(HexCell cell)
    {
        transform.position = cell.transform.position;
        currentCell = cell;
        lastValidPosition = transform.position;
        availableMoves--;
        UpdateMovesText();

        if (availableMoves <= 0)
        {
            GameManager.Instance.EndTurn();
        }
    }

    private PlayerController FindPlayerAtCell(HexCell cell)
    {
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (player.currentCell == cell)
            {
                return player;
            }
        }
        return null;
    }

    private void StartFight(PlayerController otherPlayer)
    {
        PlayerPiece thisPiece = GetComponent<PlayerPiece>();
        PlayerPiece otherPiece = otherPlayer.GetComponent<PlayerPiece>();

        if (thisPiece != null && otherPiece != null)
        {
            thisPiece.Attack(otherPiece);
            otherPiece.Attack(thisPiece);

            ResetPosition();
            otherPlayer.ResetPosition();
        }
    }

    public void ResetPosition()
    {
        HexCell randomCell = GetRandomUnoccupiedCell();
        transform.position = randomCell.transform.position;
        currentCell = randomCell;
        lastValidPosition = transform.position;
    }

    private HexCell GetRandomUnoccupiedCell()
    {
        HexCell randomCell;
        do
        {
            randomCell = hexGrid.GetRandomCell();
        } while (FindPlayerAtCell(randomCell) != null);

        return randomCell;
    }

    private bool IsAdjacent(HexCell cell1, HexCell cell2)
    {
        Vector3[] directions = new Vector3[]
        {
            new Vector3(1, -1, 0), new Vector3(1, 0, -1), new Vector3(0, 1, -1),
            new Vector3(-1, 1, 0), new Vector3(-1, 0, 1), new Vector3(0, -1, 1)
        };

        foreach (Vector3 direction in directions)
        {
            if (cell1.CubeCoordinates + direction == cell2.CubeCoordinates)
            {
                return true;
            }
        }
        return false;
    }

    private void HighlightPossibleMovements()
    {
        hexGrid.HighlightCells(currentCell);
    }

    private void UpdateMovesText()
    {
        if (movesText != null)
        {
            movesText.text = $"Moves: {availableMoves}";
        }
    }

    public void EndTurn()
    {
        isTurn = false;
        hexGrid.ClearHighlights();
    }

    public void StartTurn()
    {
        isTurn = true;
        availableMoves = maxMovesPerTurn;
        UpdateMovesText();
        HighlightPossibleMovements();
    }

    private void InitializePlayerPosition()
    {
        currentCell = GetRandomUnoccupiedCell();
        if (currentCell != null)
        {
            transform.position = currentCell.transform.position;
            lastValidPosition = transform.position;
        }
    }

    private void AddColliderIfMissing()
    {
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
}