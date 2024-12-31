using UnityEngine;
using System;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation Orientation { get; private set; }
    [field: SerializeField] public int Width { get; private set; }
    [field: SerializeField] public int Height { get; private set; }
    [field: SerializeField] public float HexSize { get; private set; }

    private HexCell[,] cells;

    private void Awake()
    {
        cells = new HexCell[Width, Height];
        CreateCells();
    }

    private void CreateCells()
    {
        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                GameObject cellObject = new GameObject($"HexCell_{x}_{z}");
                cellObject.transform.parent = this.transform;

                HexCell cell = cellObject.AddComponent<HexCell>();
                cell.Grid = this;
                cell.HexSize = HexSize;
                cell.OffsetCoordinates = new Vector2(x, z);
                cell.CubeCoordinates = HexMetrics.OffsetToCube(x, z, Orientation);
                cell.AxialCoordinates = HexMetrics.OffsetToAxail(x, z, Orientation);

                cellObject.transform.localPosition = CalculatePosition(x, z);

                MeshRenderer renderer = cellObject.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard"));

                cells[x, z] = cell;
            }
        }
    }

    private Vector3 CalculatePosition(int x, int z)
    {
        float width = HexSize * 2f;
        float height = Mathf.Sqrt(3f) * HexSize;

        float posX = x * width * 0.75f;
        float posZ = z * height + (x % 2 == 0 ? 0 : height / 2f);

        return new Vector3(posX, 0, posZ);
    }

    private bool IsValidCell(Vector2 cell)
    {
        return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
    }

    public HexCell GetCell(Vector2 offsetCoordinates)
    {
        int x = (int)offsetCoordinates.x;
        int z = (int)offsetCoordinates.y;
        if (IsValidCell(offsetCoordinates))
        {
            return cells[x, z];
        }
        return null;
    }

    internal void HighlightCells(Vector2 currentCell)
    {
        int x = (int)currentCell.x;
        int z = (int)currentCell.y;
        if (IsValidCell(currentCell))
        {
            HexCell cell = cells[x, z];
            MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.blue;
            }
        }
    }

    internal void ClearHighlights()
    {
        throw new NotImplementedException();
    }

    public HexCell GetClosestCell(Vector3 position)
    {
        HexCell closestCell = null;
        float closestDistance = float.MaxValue;

        foreach (HexCell cell in cells)
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
        return cells[x, z];
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}