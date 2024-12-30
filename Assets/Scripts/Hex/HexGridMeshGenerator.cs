
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]

public class HexGridMeshGenerator : MonoBehaviour
{
    [field: SerializeField] public LayerMask GridLayer { get; private set; }
    [field: SerializeField] public HexGrid HexGrid { get; private set; }

    private void Awake()
    {
        if (HexGrid == null)
        {
            HexGrid = GetComponentInParent<HexGrid>();
        }
        if (HexGrid == null)
        {
            Debug.LogError("HexGridMeshGenerator requires a HexGrid component in the parent GameObject.");
        }
    }

    public void CreateHexMesh()
    {
        CreateHexMesh(HexGrid.Width, HexGrid.Height, HexGrid.HexSize, HexGrid.Orientation, GridLayer);
    }

    public void CreateHexMesh(HexGrid hexGrid, LayerMask layerMask)
    {
        this.HexGrid = hexGrid;
        this.GridLayer = layerMask;
        CreateHexMesh(HexGrid.Width, HexGrid.Height, HexGrid.HexSize, HexGrid.Orientation, GridLayer);
    }

    public void CreateHexMesh(int width, int height, float hexSize, HexOrientation orientation, LayerMask layerMask)
    {
        ClearHexGridMesh();
        Vector3[] vertices = new Vector3[width * height * 7];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 centerPosition = HexMetrics.Center(hexSize, orientation, x, z);
                vertices[z * width * 7 + x * 7] = centerPosition;
                for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                {
                    vertices[(z * width + x) * 7 + s + 1] = centerPosition + HexMetrics.Corners(hexSize, orientation)[s % 6];
                }
            }
        }
        int[] triangles = new int[width * height * 18];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int s = 0; s < HexMetrics.Corners(hexSize, orientation).Length; s++)
                {
                    int cornerIndex = s + 2 > 6 ? s + 2 - 6 : s + 2;
                    triangles[3 * 6 * (z * width + x) + s * 3] = (z * width + x) * 7;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 1] = (z * width + x) * 7 + s + 1;
                    triangles[3 * 6 * (z * width + x) + s * 3 + 2] = (z * width + x) * 7 + cornerIndex;
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.name = "HexGridMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.Optimize();
        mesh.RecalculateUVDistributionMetrics();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        int gridLayerIndex = GetLayerIndex(layerMask);
        Debug.Log("Layer index: " + gridLayerIndex);

        gameObject.layer = gridLayerIndex;
    }

    private int GetLayerIndex(LayerMask layerMask)
    {
        int layerMaskValue = layerMask.value;
        Debug.Log("Layer mask value: " + layerMaskValue);
        for (int i = 0; i < 32; i++)
        {
            if ((layerMaskValue & (1 << i)) != 0)
            {
                Debug.Log("Layer Index Loop: " + i);
                return i;
            }
        }
        return 0;
    }


    public void ClearHexGridMesh()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
            return;
        GetComponent<MeshFilter>().sharedMesh.Clear();
        GetComponent<MeshCollider>().sharedMesh.Clear();
    }
}
