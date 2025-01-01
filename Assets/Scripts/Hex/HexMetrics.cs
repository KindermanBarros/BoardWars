using UnityEngine;

public static class HexMetrics
{
    public static float OuterRadius(float hexSize) => hexSize;

    public static float InnerRadius(float hexSize) => hexSize * 0.866025404f;

    public static Vector3[] Corners(float hexSize, HexOrientation orientation)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            corners[i] = Corner(hexSize, orientation, i);
        }
        return corners;
    }

    public static Vector3 Corner(float hexSize, HexOrientation orientation, int index)
    {
        float angle = 60f * -index;
        if (orientation == HexOrientation.PointyTop)
        {
            angle -= 30f;
        }
        return new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad), 0f, hexSize * Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    public static Vector3 Center(float hexSize, int x, int z, HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return new Vector3((x + z * 0.5f - z / 2) * (InnerRadius(hexSize) * 2f), 0f, z * (OuterRadius(hexSize) * 1.5f));
        }
        else
        {
            return new Vector3(x * (OuterRadius(hexSize) * 1.5f), 0f, (z + x * 0.5f - x / 2) * (InnerRadius(hexSize) * 2f));
        }
    }

    public static Vector2[] GetAdjacentDirections(HexOrientation orientation)
    {
        if (orientation == HexOrientation.PointyTop)
        {
            return new Vector2[]
            {
                new(1, 0), new(1, -1), new(0, -1),
                new(-1, 0), new(-1, 1), new(0, 1)
            };
        }
        else
        {
            return new Vector2[]
            {
                new(1, 0), new(1, 1), new(0, 1),
                new(-1, 0), new(-1, -1), new(0, -1)
            };
        }
    }

    public static Vector3 OffsetToCube(Vector2 offsetCoord, HexOrientation orientation) =>
        OffsetToCube((int)offsetCoord.x, (int)offsetCoord.y, orientation);

    public static Vector3 OffsetToCube(int col, int row, HexOrientation orientation)
    {
        return orientation == HexOrientation.PointyTop
            ? AxialToCube(OffsetToAxialPointy(col, row))
            : AxialToCube(OffsetToAxialFlat(col, row));
    }

    public static Vector3 AxialToCube(float q, float r) => new(q, r, -q - r);

    public static Vector3 AxialToCube(int q, int r) => new(q, r, -q - r);

    public static Vector3 AxialToCube(Vector2 axialCoord) => AxialToCube(axialCoord.x, axialCoord.y);

    public static Vector2 CubeToAxial(int q, int r, int s) => new(q, r);

    public static Vector2 CubeToAxial(float q, float r, float s) => new(q, r);

    public static Vector2 CubeToAxial(Vector3 cube) => new(cube.x, cube.y);

    public static Vector2 OffsetToAxail(int x, int z, HexOrientation orientation)
    {
        return orientation == HexOrientation.PointyTop
            ? OffsetToAxialPointy(x, z)
            : OffsetToAxialFlat(x, z);
    }

    private static Vector2 OffsetToAxialFlat(int col, int row)
    {
        var q = col;
        var r = row - (col - (col & 1)) / 2;
        return new Vector2(q, r);
    }

    private static Vector2 OffsetToAxialPointy(int col, int row)
    {
        var q = col - (row - (row & 1)) / 2;
        var r = row;
        return new Vector2(q, r);
    }

    public static Vector2 CubeToOffset(int x, int y, int z, HexOrientation orientation)
    {
        return orientation == HexOrientation.PointyTop
            ? CubeToOffsetPointy(x, y, z)
            : CubeToOffsetFlat(x, y, z);
    }

    public static Vector2 CubeToOffset(Vector3 offsetCoord, HexOrientation orientation) =>
        CubeToOffset((int)offsetCoord.x, (int)offsetCoord.y, (int)offsetCoord.z, orientation);

    private static Vector2 CubeToOffsetPointy(int x, int y, int z) =>
        new(x + (y - (y & 1)) / 2, y);

    private static Vector2 CubeToOffsetFlat(int x, int y, int z) =>
        new(x, y + (x - (x & 1)) / 2);

    private static Vector3 CubeRound(Vector3 frac)
    {
        int rx = Mathf.RoundToInt(frac.x);
        int ry = Mathf.RoundToInt(frac.y);
        int rz = Mathf.RoundToInt(frac.z);
        float xDiff = Mathf.Abs(rx - frac.x);
        float yDiff = Mathf.Abs(ry - frac.y);
        float zDiff = Mathf.Abs(rz - frac.z);
        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }
        return new Vector3(rx, ry, rz);
    }

    public static Vector2 AxialRound(Vector2 coordinates) =>
        CubeToAxial(CubeRound(AxialToCube(coordinates.x, coordinates.y)));

    public static Vector2 CoordinateToAxial(float x, float z, float hexSize, HexOrientation orientation)
    {
        return orientation == HexOrientation.PointyTop
            ? CoordinateToPointyAxial(x, z, hexSize)
            : CoordinateToFlatAxial(x, z, hexSize);
    }

    private static Vector2 CoordinateToPointyAxial(float x, float z, float hexSize)
    {
        Vector2 pointyHexCoordinates = new()
        {
            x = (Mathf.Sqrt(3) / 3 * x - 1f / 3 * z) / hexSize,
            y = (2f / 3 * z) / hexSize
        };
        return AxialRound(pointyHexCoordinates);
    }

    private static Vector2 CoordinateToFlatAxial(float x, float z, float hexSize)
    {
        Vector2 flatHexCoordinates = new()
        {
            x = (2f / 3 * x) / hexSize,
            y = (-1f / 3 * x + Mathf.Sqrt(3) / 3 * z) / hexSize
        };
        return AxialRound(flatHexCoordinates);
    }

    public static Vector2 CoordinateToOffset(float x, float z, float hexSize, HexOrientation orientation) =>
        CubeToOffset(AxialToCube(CoordinateToAxial(x, z, hexSize, orientation)), orientation);
}