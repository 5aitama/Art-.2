using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(64)]
public struct MapPointBuffer : IBufferElementData
{
    public float2 Position;
    public float Height;
}