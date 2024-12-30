using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexGrid))]

public class HexGridEditor : Editor
{
    void OnSceneGUI()
    {
        HexGrid hexGrid = (HexGrid)target;

        for (int z = 0; z < hexGrid.Height; z++)
        {
            for (int x = 0; x < hexGrid.Width; x++)
            {
                Vector3 centerPosition = HexMetrics.Center(hexGrid.HexSize, hexGrid.Orientation, x, z) + hexGrid.transform.position;

                int centerX = x;
                int centerZ = z;

                Vector3 cubeCoord = HexMetrics.OffsetToCube(centerX, centerZ, hexGrid.Orientation);
                Handles.Label(centerPosition + Vector3.forward * 0.5f, $"[{centerX}, {centerZ}]");
                Handles.Label(centerPosition, $"({cubeCoord.x}, {cubeCoord.y}, {cubeCoord.z})");
            }

        }
    }
}

