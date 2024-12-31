using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexGrid Grid { get; set; }
    public float HexSize { get; set; }
    public Vector2 OffsetCoordinates { get; set; }
    public Vector3 CubeCoordinates { get; set; }
    public Vector2 AxialCoordinates { get; set; }

    private void Awake()
    {
        gameObject.AddComponent<BoxCollider>();
    }
}