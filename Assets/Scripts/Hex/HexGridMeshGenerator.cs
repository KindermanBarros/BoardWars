using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexGridMeshGenerator : MonoBehaviour
{
    [field: SerializeField] public LayerMask gridLayer { get; private set; }
    [field: SerializeField] public HexGrid hexGrid { get; private set; }
    [field: SerializeField] public Material gridMaterial { get; private set; }

    private void Awake()
    {
        if (hexGrid == null)
            hexGrid = GetComponentInParent<HexGrid>();
        if (hexGrid == null)
            Debug.LogError("HexGridMeshGenerator could not find a HexGrid component in its parent or itself.");
    }

    public void CreateHexMesh()
    {
        CreateHexMesh(hexGrid.Width, hexGrid.Height, hexGrid.HexSize, hexGrid.Orientation, gridLayer);
    }

    public void CreateHexMesh(HexGrid hexGrid, LayerMask layerMask)
    {
        this.hexGrid = hexGrid;
        this.gridLayer = layerMask;
        CreateHexMesh(hexGrid.Width, hexGrid.Height, hexGrid.HexSize, hexGrid.Orientation, layerMask);
    }

    public void CreateHexMesh(int width, int height, float hexSize, HexOrientation orientation, LayerMask layerMask)
    {
        ClearHexGridMesh();
        Vector3[] vertices = new Vector3[7 * width * height];
        Vector2[] uvs = new Vector2[7 * width * height];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 centrePosition = HexMetrics.Center(hexSize, x, z, orientation);
                vertices[(z * width + x) * 7] = centrePosition;
                uvs[(z * width + x) * 7] = new Vector2(0.5f, 0.5f);

                for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                {
                    vertices[(z * width + x) * 7 + s + 1] = centrePosition + HexMetrics.Corners(hexSize, orientation)[s % 6];
                    uvs[(z * width + x) * 7 + s + 1] = new Vector2((HexMetrics.Corners(hexSize, orientation)[s % 6].x / (hexSize * 2)) + 0.5f, (HexMetrics.Corners(hexSize, orientation)[s % 6].z / (hexSize * 2)) + 0.5f); // Corner UVs
                }
            }
        }

        int[] triangles = new int[3 * 6 * width * height];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                {
                    int cornerIndex = s + 2 > 6 ? s + 2 - 6 : s + 2;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 0] = (z * width + x) * 7;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 1] = (z * width + x) * 7 + s + 1;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 2] = (z * width + x) * 7 + cornerIndex;
                }
            }
        }

        Mesh mesh = new()
        {
            name = "Hex Mesh",
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateUVDistributionMetrics();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = gridMaterial;

        gameObject.layer = GetLayerIndex(layerMask);
    }

    public void ClearHexGridMesh()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
            return;
        GetComponent<MeshFilter>().sharedMesh.Clear();
        GetComponent<MeshCollider>().sharedMesh.Clear();
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerMaskValue = layerMask.value;
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & layerMaskValue) != 0)
            {
                return i;
            }
        }
        return 0;
    }
}