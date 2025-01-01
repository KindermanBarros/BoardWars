using UnityEngine;
using System.Collections.Generic;

public class HexCell : MonoBehaviour
{
    public HexGrid Grid { get; set; }
    public float HexSize { get; set; }
    public Vector2 OffsetCoordinates { get; set; }
    public Vector3 CubeCoordinates { get; set; }
    public Vector2 AxialCoordinates { get; set; }
    public List<HexCell> Neighbors { get; private set; } = new List<HexCell>();
    public Material OriginalMaterial { get; set; }
    public Collectable CurrentCollectable { get; private set; }

    public void AddNeighbor(HexCell neighbor)
    {
        if (neighbor != null && !Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
        }
    }

    public void SetCollectable(Collectable collectable)
    {
        if (CurrentCollectable != null)
        {
            Destroy(CurrentCollectable.gameObject);
        }

        CurrentCollectable = collectable;
        if (collectable != null)
        {
            collectable.transform.SetParent(transform);
            collectable.transform.localPosition = Vector3.up * 0.5f;
        }
    }

    public Collectable CollectCollectable()
    {
        var collectable = CurrentCollectable;
        CurrentCollectable = null;
        return collectable;
    }
}