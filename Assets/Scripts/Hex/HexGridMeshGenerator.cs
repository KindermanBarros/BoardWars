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
        int[] triangles = new int[3 * 6 * width * height];

        Vector3[] corners = HexMetrics.Corners(hexSize, orientation);

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 centerPosition = HexMetrics.Center(hexSize, x, z, orientation);
                vertices[vertexIndex] = centerPosition;
                uvs[vertexIndex] = new Vector2(0.5f, 0.5f);

                for (int s = 0; s < corners.Length; s++)
                {
                    vertices[vertexIndex + s + 1] = centerPosition + corners[s];
                    uvs[vertexIndex + s + 1] = new Vector2((corners[s].x / (hexSize * 2)) + 0.5f, (corners[s].z / (hexSize * 2)) + 0.5f);
                }

                for (int s = 0; s < corners.Length; s++)
                {
                    int cornerIndex = s + 2 > 6 ? s + 2 - 6 : s + 2;
                    triangles[triangleIndex + s * 3] = vertexIndex;
                    triangles[triangleIndex + s * 3 + 1] = vertexIndex + s + 1;
                    triangles[triangleIndex + s * 3 + 2] = vertexIndex + cornerIndex;
                }

                vertexIndex += 7;
                triangleIndex += 18;
            }
        }

        Mesh mesh = new Mesh
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