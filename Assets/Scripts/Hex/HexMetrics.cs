using System;
using UnityEngine;
using static HexGrid;


public static class HexMetrics
{
    public static float OuterRadius(float hexSize)
    {
        return hexSize;
    }
    public static float InnerRadius(float hexSize)
    {
        return hexSize * 0.866025404f;
    }

    public static Vector3[] Corners(float hexSize, HexOrientation orientation)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            corners[i] = Corner(hexSize, i, orientation);
        }
        return corners;
    }

    private static Vector3 Corner(float hexSize, int index, HexOrientation orientation)
    {
        float angle = 60f * index;
        if (orientation == HexOrientation.PointyTop)
        {
            angle += 30f;
        }
        Vector3 corner = new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad),
        0f,
        hexSize * Mathf.Sin(angle * Mathf.Deg2Rad)
    );
        return corner;
    }

    public static Vector3 Center(float hexSize, HexOrientation orientation, int x, int z)
    {
        Vector3 centerPosition;
        if (orientation == HexOrientation.PointyTop)
        {
            centerPosition.x = (x + z * 0.5f - z / 2) * (InnerRadius(hexSize) * 2f);
            centerPosition.y = 0f;
            centerPosition.z = z * (OuterRadius(hexSize) * 1.5f);
        }
        else
        {
            centerPosition.x = (x) * (OuterRadius(hexSize) * 1.5f);
            centerPosition.y = 0f;
            centerPosition.z = (z + x * 0.5f - x / 2) * (InnerRadius(hexSize) * 2f);
        }
        return centerPosition;
    }

    public static Vector3 OffsetToCube(int x, int z, HexOrientation orientation)
    {
        int cubeX, cubeY, cubeZ;
        if (orientation == HexOrientation.PointyTop)
        {
            cubeX = x - (z - (z & 1)) / 2;
            cubeZ = z;
            cubeY = -cubeX - cubeZ;
        }
        else
        {
            cubeX = x;
            cubeZ = z - (x - (x & 1)) / 2;
            cubeY = -cubeX - cubeZ;
        }
        return new Vector3(cubeX, cubeY, cubeZ);
    }
}
