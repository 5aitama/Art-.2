using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public static class GridGenerator
{
    [BurstCompile]
    public struct CreateGridJob : IJob
    {
        [ReadOnly] public int2 GridSize;

        [WriteOnly] public NativeArray<float3>  Vertices;
        [WriteOnly] public NativeArray<int>     Indices;
        [WriteOnly] public NativeArray<float2>  Uvs;

        public void Execute()
        {
            var totalSize = GridSize.x * GridSize.y;
            var indicesIndex = 0;
            
            for (var i = 0; i < totalSize; i++)
            {
                var p = new int2(i % GridSize.x, i / GridSize.x);
                var v = new float3(p.x, 0, p.y);

                var index = p.x + p.y * GridSize.x;

                Vertices[i] = v;
                Uvs[i] = p;

                if (p.y <= 0 || p.x <= 0) continue;
                
                Indices[indicesIndex    ] = index - (GridSize.x + 1);
                Indices[indicesIndex + 1] = index - 1;
                Indices[indicesIndex + 2] = index;

                Indices[indicesIndex + 3] = index - (GridSize.x + 1);
                Indices[indicesIndex + 4] = index;
                Indices[indicesIndex + 5] = index - GridSize.x;

                indicesIndex += 6;
            }
        }
    }
}