using Unity.Mathematics;

public static class HexDirection
{
    public static readonly int3 Top           = new int3( 0,  1, -1);
    public static readonly int3 TopLeft       = new int3(-1,  1,  0);
    public static readonly int3 TopRight      = new int3( 1,  0, -1);
    public static readonly int3 Bottom        = new int3( 0, -1,  1);
    public static readonly int3 BottomLeft    = new int3(-1,  0,  1);
    public static readonly int3 BottomRight   = new int3( 1, -1,  0);
}

public static class Hexagon
{
    public struct Hex
    {
        /// <summary>
        /// Size of hexagon.
        /// </summary>
        public float Size;

        /// <summary>
        /// Offset index of hexagon.
        /// </summary>
        public int2 OffsetCoord;
        
        /// <summary>
        /// World coordinate of hexagon.
        /// </summary>
        public float3 WorldCoord => ToHexPosition(new float3(OffsetCoord.x, 0, OffsetCoord.y + (OffsetCoord.x % 2 != 0 ? 0.5f : 0f)), Size);

        /// <summary>
        /// Cube coordinate of hexagon.
        /// </summary>
        public int3 CubeCoord  => CoordToCube(OffsetCoord);
    }

    public static int3 CoordToCube(int2 coord)
    {
        coord.y *= -1;
        var x = coord.x;
        var z = coord.y - (coord.x + (coord.x & 1)) / 2;
        var y = -x - z;
        return new int3(x, y, z);
    }

    public static int2 CubeToCoord(float3 cube)
    {
        var x = (int)cube.x;
        var z = (int)cube.z + ((int)cube.x + ((int)cube.x & 1)) / 2;
        return new int2(x, z);
    }

    public static float3 ToHexPosition(float3 position, float size)
    {
        var w = 2f * size;
        var h = math.sqrt(3f) * size;

        return position * new float3(w * (3f / 4f), 0, h);
    }

    public static float3 HexCorner(float3 center, float size, int i)
    {
        var angleDeg = 60f * i;
        var angleRad = math.radians(angleDeg);

        var x = center.x + size * math.cos(angleRad);
        var z = center.z + size * math.sin(angleRad);

        return new float3(x, 0, z);
    }
}
