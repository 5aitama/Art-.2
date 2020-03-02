using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class TerracedMapInitializeSystem : JobComponentSystem
{
    [BurstCompile]
    private struct InitializeMapBufferJob : IJobParallelFor
    {
        [ReadOnly] public int MapWidth;
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float Persistence;
        [ReadOnly] public int Octaves;
        [ReadOnly] public float2 NoisePosition;

        [WriteOnly] public NativeArray<MapPointBuffer> MapPoints;

        public void Execute(int index)
        {
            var x = index % MapWidth;
            var y = index / MapWidth;
            
            var position = new float2(x, y);
            var noisePosition = position + NoisePosition;

            var noiseValue = OctavePerlin(noisePosition, Frequency, Amplitude, Octaves, Persistence);
            noiseValue *= Amplitude;

            MapPoints[index] = new MapPointBuffer
            {
                Height = noiseValue,
                Position = position,
            };
        }

        public float OctavePerlin(float2 position, float frequency, float amplitude, int octaves, float persistence) {
            var total = 0f;
            var maxValue = 0f;
            for(var i = 0; i < octaves; i++) 
            {
                var n = noise.snoise(position * frequency);
                n = (n + 1f) / 2f;
                n *= amplitude;
                n = math.round(n);
                total += n;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }
            
            return total/maxValue;
        }
    }

    private EndSimulationEntityCommandBufferSystem _endSimulation;

    protected override void OnCreate()
    {
        _endSimulation = World.DefaultGameObjectInjectionWorld
            .GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    [BurstCompile]
    private struct AddBuffersJob<T> : IJob where T : struct, IBufferElementData
    {
        public EntityCommandBuffer commandBuffer;
        [ReadOnly] public Entity entity;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<T> bufferArray;

        public void Execute()
        {
            var buffer = commandBuffer.AddBuffer<T>(entity);
            buffer.AddRange(bufferArray);
            commandBuffer.RemoveComponent<TagTerracedMapNeedInitialize>(entity);
            commandBuffer.AddComponent<TagTerracedMapNeedBuild>(entity);
        }
    }

    [BurstCompile]
    private struct DisposeAll : IJob
    {
        [DeallocateOnJobCompletion]
        public NativeArray<Entity> entities;

        public void Execute() { }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = _endSimulation.CreateCommandBuffer();

        var mapSize = GameSettings.MapSettingsInstance.mapSize;
        var mapArea = GameSettings.MapSettingsInstance.MapArea;

        var noiseFrequency      = GameSettings.MapSettingsInstance.noiseFrequency;
        var noiseAmplitude      = GameSettings.MapSettingsInstance.noiseAmplitude;
        var noisePosition       = GameSettings.MapSettingsInstance.noisePosition;
        var noisePersistence    = GameSettings.MapSettingsInstance.noisePersistence;
        var noiseOctaves        = GameSettings.MapSettingsInstance.noiseOctaves;

        var query = new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(TagTerracedMapNeedInitialize)
            }
        };

        var entities = GetEntityQuery(query).ToEntityArray(Allocator.TempJob);

        JobHandle handle = default;

        for (var i = 0; i < entities.Length; i++)
        {
            var mapPointBuffers = new NativeArray<MapPointBuffer>(mapArea,
                Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var transform = GetComponentDataFromEntity<Translation>(true)[entities[i]].Value;

            handle = new InitializeMapBufferJob
            {
                MapPoints       = mapPointBuffers,
                MapWidth        = mapSize,
                Frequency       = noiseFrequency,
                Amplitude       = noiseAmplitude,
                Persistence     = noisePersistence,
                Octaves         = noiseOctaves,
                NoisePosition   = noisePosition + transform.xz,
            }.Schedule(mapPointBuffers.Length, 32, handle);

            handle = new AddBuffersJob<MapPointBuffer>
            {
                commandBuffer   = commandBuffer,
                bufferArray     = mapPointBuffers,
                entity          = entities[i],
            }.Schedule(handle);
        }
        
        handle = new DisposeAll
        {
            entities = entities,
        }.Schedule(handle);

        handle.Complete();
        return inputDeps;
    }
}