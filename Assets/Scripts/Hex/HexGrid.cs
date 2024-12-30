using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation Orientation { get; private set; }
    [field: SerializeField] public int Width { get; private set; }
    [field: SerializeField] public int Height { get; private set; }
    [field: SerializeField] public float HexSize { get; private set; }
    [field: SerializeField] public GameObject HexPrefab { get; private set; }

    private void OnDrawGizmos()
    {
        for (int z = 0; z < Height; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 centerPosition = HexMetrics.Center(HexSize, Orientation, x, z) + transform.position;
                for (int s = 0; s < HexMetrics.Corners(HexSize, Orientation).Length; s++)
                {
                    Vector3 start = HexMetrics.Corners(HexSize, Orientation)[s % 6] + centerPosition;
                    Vector3 end = HexMetrics.Corners(HexSize, Orientation)[(s + 1) % 6] + centerPosition;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}