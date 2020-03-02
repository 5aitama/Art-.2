using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class HexMapInitializeSystem : ComponentSystem
{
    [BurstCompile]
    private struct InitializeHexMapBufferJob : IJobParallelFor
    {
        [ReadOnly] public int MapWidth;
        [ReadOnly] public float CellSize;
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float Persistence;
        [ReadOnly] public int Octaves;
        [ReadOnly] public float2 NoisePosition;

        [WriteOnly] public NativeArray<HexCellDataBuffer> HexCellDataBuffers;
    
        public void Execute(int index)
        {
            var x = index % MapWidth;
            var y = index / MapWidth;
            
            var hexCellBuffer = new HexCellBuffer
            {
                OffsetCoord = new int2(x, y),
                Size = CellSize
            };
            
            var heights = new NativeArray<float3>(7, Allocator.Temp);
            
            for (var i = 0; i < 6; i++)
            {
                var pos = Hexagon.HexCorner(hexCellBuffer.WorldCoord, hexCellBuffer.Size, i);
                var height = OctavePerlin(pos.xz + NoisePosition, Frequency, Amplitude, Octaves, Persistence);
                height *= Amplitude;

                heights[i + 1] = new float3(pos.x, height, pos.z);
            }
            
            var heightCenter = OctavePerlin(hexCellBuffer.WorldCoord.xz + NoisePosition, Frequency, Amplitude, Octaves, Persistence);
            heightCenter *= Amplitude;
            
            heights[0] = new float3(hexCellBuffer.WorldCoord.x, heightCenter, hexCellBuffer.WorldCoord.z);
            
            HexCellDataBuffers[index] = new HexCellDataBuffer
            {
                OffsetCoord = hexCellBuffer.OffsetCoord,
                Size = hexCellBuffer.Size,
                V0 = heights[0],
                V1 = heights[1],
                V2 = heights[2],
                V3 = heights[3],
                V4 = heights[4],
                V5 = heights[5],
                V6 = heights[6],
            };
        }
    
        private static float OctavePerlin(float2 position, float frequency, float amplitude, int octaves, float persistence)
        {
            var total = 0f;
            var maxValue = 0f;
            for (var i = 0; i < octaves; i++)
            {
                var n = noise.snoise(position * frequency);
                n = (n + 1f) / 2f;
                n *= amplitude;
                total += n;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }
    
            return total / maxValue;
        }
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<TagHexMap, TagHexMapNeedInitialize>().ForEach((Entity entity, ref Translation translation) =>
        {

            var mapSize = GameSettings.MapSettingsInstance.mapSize;

            var hexCellDataBuffers = new NativeArray<HexCellDataBuffer>(mapSize * (mapSize - 1), Allocator.TempJob);
            var handle = new InitializeHexMapBufferJob
            {
                CellSize      = 1f,
                MapWidth      = mapSize,
                Amplitude     = GameSettings.MapSettingsInstance.noiseAmplitude,
                Frequency     = GameSettings.MapSettingsInstance.noiseFrequency,
                NoisePosition = GameSettings.MapSettingsInstance.noisePosition + translation.Value.xz,
                Octaves       = GameSettings.MapSettingsInstance.noiseOctaves,
                Persistence   = GameSettings.MapSettingsInstance.noisePersistence,
                HexCellDataBuffers = hexCellDataBuffers,

            }.Schedule(mapSize * (mapSize - 1), 32);
            
            handle.Complete();

            var hexCellDatabuffer = EntityManager.AddBuffer<HexCellDataBuffer>(entity);
            hexCellDatabuffer.AddRange(hexCellDataBuffers);
            hexCellDataBuffers.Dispose();
            
            EntityManager.RemoveComponent<TagHexMapNeedInitialize>(entity);
            EntityManager.AddComponent<TagHexMapNeedBuild>(entity);
        });
    }
}