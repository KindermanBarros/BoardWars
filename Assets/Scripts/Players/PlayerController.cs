using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public HexGrid hexGrid;
    public HexCell currentCell;
    public float moveSpeed = 5f;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastValidPosition;

    private void Start()
    {
        if (hexGrid == null)
        {
            Debug.LogError("HexGrid is not assigned in the PlayerController script.");
            return;
        }

        currentCell = GetRandomUnoccupiedCell();
        if (currentCell != null)
        {
            transform.position = currentCell.transform.position;
            lastValidPosition = transform.position;
        }

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        MouseController.Instance.OnLeftMouseClick += HandleLeftMouseClick;
    }

    private void OnDestroy()
    {
        MouseController.Instance.OnLeftMouseClick -= HandleLeftMouseClick;
    }

    private void Update()
    {
        if (isDragging)
        {
            DragPlayer();
        }
    }

    private void HandleLeftMouseClick(RaycastHit hit)
    {
        if (hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        SnapToClosestGridCell();
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
        if (closestCell != null)
        {
            PlayerController otherPlayer = FindPlayerAtCell(closestCell);
            if (otherPlayer != null && otherPlayer != this)
            {
                StartFight(otherPlayer);
            }
            else
            {
                transform.position = closestCell.transform.position;
                currentCell = closestCell;
                lastValidPosition = transform.position;
            }
        }
        else
        {
            transform.position = lastValidPosition;
        }
    }

    private PlayerController FindPlayerAtCell(HexCell cell)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
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
}