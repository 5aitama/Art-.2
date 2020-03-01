using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

public class EndlessSystem : ComponentSystem
{
    private NativeHashMap<float3, Entity> _chunksA;
    private bool _initialized;

    protected override void OnDestroy()
    {
        if (_chunksA.IsCreated) _chunksA.Dispose();
    }

    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;

        var mapSize = GameSettings.MapSettingsInstance.mapSize - 1;
        var cameraPosition = mainCamera != null ? (float3)mainCamera.transform.position : new float3(0);

        // Convert camera position
        var cameraChunkPosition = ToChunkPosition(cameraPosition, mapSize);

        // View camera position
        Debug.DrawLine(cameraPosition, cameraChunkPosition, Color.blue);

        var chunkPerAxis = GameSettings.EndlessSettingsInstance.chunkPerAxis;
        // Generate neighbours chunk position...
        var neighbours = GenerateNeighbourChunksPosition(cameraChunkPosition, mapSize, chunkPerAxis, Allocator.Temp);

        var chunksB = new NativeHashMap<float3, Entity>(neighbours.Length, Allocator.Temp);

        if(!_initialized)
        {
            _chunksA = new NativeHashMap<float3, Entity>(neighbours.Length, Allocator.Persistent);
        }

        // View neighbours position...
        for(var i = 0; i < neighbours.Length; i++)
            Debug.DrawLine(neighbours[i], cameraPosition, Color.green);

        for(var i = 0; i < neighbours.Length; i++)
        {
            var key = neighbours[i];

            if (!_initialized)
            {
                _chunksA.Add(key, CreateChunkEntity(key));
            }
            else if(_chunksA.ContainsKey(key))
            {
                chunksB.Add(key, _chunksA[key]);
                _chunksA.Remove(key);
            }
            else
            {
                chunksB.Add(key, CreateChunkEntity(key));
            }
        }

        if (!_initialized) _initialized = true;

        // Destroy all entities in chunks A
        EntityManager.DestroyEntity(_chunksA.GetValueArray(Allocator.Temp));

        // Clear chunksA
        _chunksA.Clear();

        // Copy all key value from chunksB to chunkA
        var keyValue = chunksB.GetKeyValueArrays(Allocator.Temp);
        
        for(var i = 0; i < keyValue.Keys.Length; i++)
        {
            _chunksA.Add(keyValue.Keys[i], keyValue.Values[i]);
        }
    }

    /// <summary>
    /// Convert position to chunk position
    /// </summary>
    /// <param name="position">Position to convert</param>
    /// <param name="chunkSize">Size of single chunk</param>
    /// <returns>Position converted to chunk position</returns>
    private static float3 ToChunkPosition(float3 position, float3 chunkSize)
    {
        position.y = 0;
        return math.round(position / chunkSize) * chunkSize;
    }

    /// <summary>
    /// Generate neighbour chunk position.
    /// </summary>
    /// <param name="center">The center position</param>
    /// <param name="chunkSize">SIze of single chunk</param>
    /// <param name="chunkAmountPerAxis">Amount of chunk for each axis (x & z)</param>
    /// <param name="allocator">Allocator of returned NativeArray</param>
    private NativeArray<float3> GenerateNeighbourChunksPosition(float3 center, float3 chunkSize, int chunkAmountPerAxis, Allocator allocator)
    {
        var offset = chunkAmountPerAxis * chunkSize / 2f;
        offset.y = 0;

        var chunkArea = chunkAmountPerAxis * chunkAmountPerAxis;
        var neighbourChunksPosition = new NativeArray<float3>(chunkArea, allocator);

        for (var i = 0; i < chunkArea; i++)
        {
            var x = i % chunkAmountPerAxis;
            var y = i / chunkAmountPerAxis;
            
            var chunkPosition = new float3(x, 0, y) * chunkSize + center - offset;
            neighbourChunksPosition[i] = chunkPosition;
        }

        return neighbourChunksPosition;
    }
    
    /// <summary>
    /// Create Map entity.
    /// </summary>
    /// <param name="position">Position of map</param>
    /// <returns></returns>
    private Entity CreateChunkEntity(float3 position)
    {
        var entity = EntityManager.CreateEntity(typeof(Translation), typeof(Rotation), typeof(TagTerrassingMapNeedInitialize));
        
        EntityManager.SetComponentData(entity, new Translation { Value = position });
        EntityManager.SetComponentData(entity, new Rotation { Value = quaternion.Euler(0) });

        // EntityManager.SetName(entity, $"Chunk [{position}]");

        return entity;
    }
    
}