using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation Orientation { get; private set; }
    [field: SerializeField] public int Width { get; private set; }
    [field: SerializeField] public int Height { get; private set; }
    [field: SerializeField] public float HexSize { get; private set; }
    [field: SerializeField] public Material HighlightMaterial { get; private set; }
    [field: SerializeField] public Material[] CellMaterials { get; private set; }

    private Dictionary<Vector3, HexCell> cells = new Dictionary<Vector3, HexCell>();
    private Vector3[] directions = new Vector3[]
    {
        new Vector3(1, 0, -1), new Vector3(1, -1, 0), new Vector3(0, -1, 1),
        new Vector3(-1, 0, 1), new Vector3(-1, 1, 0), new Vector3(0, 1, -1)
    };

    private void Awake()
    {
        CreateCells();
        InitializeNeighbors();
    }

    private void CreateCells()
    {
        int materialIndex = 0;
        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                CreateCell(x, z, CellMaterials[materialIndex]);
                materialIndex = (materialIndex + 1) % CellMaterials.Length;
            }
        }
    }

    private void CreateCell(int x, int z, Material material)
    {
        GameObject cellObject = new($"HexCell_{x}_{z}");
        cellObject.transform.parent = this.transform;

        HexCell cell = cellObject.AddComponent<HexCell>();
        cell.Grid = this;
        cell.HexSize = HexSize;
        cell.OffsetCoordinates = new Vector2(x, z);
        cell.CubeCoordinates = HexMetrics.OffsetToCube(x, z, Orientation);
        cell.AxialCoordinates = HexMetrics.OffsetToAxail(x, z, Orientation);

        cellObject.transform.localPosition = CalculatePosition(x, z) + new Vector3(0, 0.01f, 0);

        MeshRenderer renderer = cellObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        cell.OriginalMaterial = renderer.material;

        MeshFilter meshFilter = cellObject.AddComponent<MeshFilter>();
        Mesh mesh = CreateHexMesh();
        meshFilter.mesh = mesh;

        cells[cell.CubeCoordinates] = cell;
    }

    private void InitializeNeighbors()
    {
        foreach (var cell in cells.Values)
        {
            foreach (Vector3 direction in directions)
            {
                if (cells.TryGetValue(cell.CubeCoordinates + direction, out HexCell neighbor))
                {
                    cell.AddNeighbor(neighbor);
                }
            }
        }
    }

    private Mesh CreateHexMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[7];
        Vector2[] uvs = new Vector2[7];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < 6; i++)
        {
            vertices[i + 1] = HexMetrics.Corner(HexSize, Orientation, i);
            uvs[i + 1] = new Vector2((vertices[i + 1].x / (HexSize * 2)) + 0.5f, (vertices[i + 1].z / (HexSize * 2)) + 0.5f);
        }

        int[] triangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i == 5 ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private Vector3 CalculatePosition(int x, int z)
    {
        float width = HexSize * 2f;
        float height = Mathf.Sqrt(3f) * HexSize;

        float posX = x * width * 0.75f;
        float posZ = z * height + (x % 2 == 0 ? 0 : height / 2f);

        return new Vector3(posX, 0, posZ);
    }

    private bool IsValidCell(Vector2 cell) =>
        cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;

    public HexCell GetCell(Vector2 offsetCoordinates)
    {
        int x = (int)offsetCoordinates.x;
        int z = (int)offsetCoordinates.y;
        return IsValidCell(offsetCoordinates) ? cells[HexMetrics.OffsetToCube(x, z, Orientation)] : null;
    }

    public int GetDistance(HexCell from, HexCell to)
    {
        if (from == null || to == null) return int.MaxValue;

        Vector3 fromCube = from.CubeCoordinates;
        Vector3 toCube = to.CubeCoordinates;


        return Mathf.Max(
            Mathf.Abs((int)(fromCube.x - toCube.x)),
            Mathf.Abs((int)(fromCube.y - toCube.y)),
            Mathf.Abs((int)(fromCube.z - toCube.z))
        );
    }

    public void HighlightPossibleMoves(HexCell currentCell, Material highlightMaterial)
    {
        ClearHighlights();
        if (currentCell == null) return;

        foreach (var neighbor in currentCell.Neighbors)
        {
            if (!BoardGame.Instance.IsCellOccupied(neighbor))
            {
                MeshRenderer renderer = neighbor.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = highlightMaterial;
                }
            }
        }
    }

    private bool IsCellOccupied(HexCell cell)
    {
        return BoardGame.Instance?.players.Any(p => p.CurrentCell == cell) ?? false;
    }

    public void ClearHighlights()
    {
        foreach (HexCell cell in cells.Values)
        {
            MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = cell.OriginalMaterial;
            }
        }
    }

    public HexCell GetClosestCell(Vector3 position)
    {
        HexCell closestCell = null;
        float closestDistance = float.MaxValue;

        foreach (HexCell cell in cells.Values)
        {
            float distance = Vector3.Distance(position, cell.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }

        return closestCell;
    }

    public HexCell GetRandomCell()
    {
        int x = UnityEngine.Random.Range(0, Width);
        int z = UnityEngine.Random.Range(0, Height);
        return cells[HexMetrics.OffsetToCube(x, z, Orientation)];
    }

    public bool IsAdjacent(HexCell cell1, HexCell cell2)
    {
        return cell1 != null && cell2 != null && cell1.Neighbors.Contains(cell2);
    }

    public Vector3 GetGridCenter()
    {
        float gridWidth = Width * HexSize * 0.75f;
        float gridHeight = Height * Mathf.Sqrt(3f) * HexSize;
        return transform.position + new Vector3(gridWidth / 2, 0, gridHeight / 2);
    }

    public HexCell GetCellInDirection(HexCell startCell, HexCell referenceCell, int distance)
    {
        if (startCell == null || referenceCell == null) return null;

        Vector3 direction = startCell.CubeCoordinates - referenceCell.CubeCoordinates;
        direction.Normalize();

        Vector3 targetCoords = startCell.CubeCoordinates + (direction * distance);

        float x = Mathf.Round(targetCoords.x);
        float z = Mathf.Round(targetCoords.z);
        float y = Mathf.Round(-x - z);

        Vector3 roundedCoords = new Vector3(x, y, z);

        cells.TryGetValue(roundedCoords, out HexCell targetCell);
        return targetCell;
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}