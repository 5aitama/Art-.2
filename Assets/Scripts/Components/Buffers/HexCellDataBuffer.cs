using System;
using Unity.Entities;
using Unity.Mathematics;

public struct HexCellDataBuffer : IBufferElementData
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
    
    public float3 V0;
    
    public float3 V1;
    public float3 V2;
    public float3 V3;
    
    public float3 V4;
    public float3 V5;
    public float3 V6;

    public float3 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return V0;
                case 1: return V1;
                case 2: return V2;
                case 3: return V3;
                case 4: return V4;
                case 5: return V5;
                case 6: return V6;
                
                default: throw new Exception($"index {index} out of range...");
            }
        }
    }
}