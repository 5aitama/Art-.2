using Unity.Entities;
using Unity.Mathematics;

public struct HexCellBuffer : IBufferElementData
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
    public float3 WorldCoord => 
        Hexagon.ToHexPosition(new float3(OffsetCoord.x, 0, OffsetCoord.y + (OffsetCoord.x % 2 != 0 ? 0.5f : 0f)), Size);

    /// <summary>
    /// Cube coordinate of hexagon.
    /// </summary>
    public int3 CubeCoord  => Hexagon.CoordToCube(OffsetCoord);
}
