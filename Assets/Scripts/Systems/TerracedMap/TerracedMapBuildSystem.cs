using Unity.Jobs;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

public class TerracedMapBuildSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<TagTerracedMapNeedBuild>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<LocalToWorld>(entity);

            EntityManager.AddComponentData(entity, new RenderBounds
            {
                Value = new AABB
                {
                    Center  = new float3(GameSettings.MapSettingsInstance.mapSize / 2f),
                    Extents = new float3(GameSettings.MapSettingsInstance.mapSize / 2f),
                }
            });

            var msize = GameSettings.MapSettingsInstance.mapSize - 1;
            var totalIndices = msize * msize * 6;
            
            var indices     = new NativeArray<int>(totalIndices, Allocator.TempJob);
            var vertices    = new NativeArray<float3>(GameSettings.MapSettingsInstance.MapArea, Allocator.TempJob);
            var uvs         = new NativeArray<float2>(GameSettings.MapSettingsInstance.MapArea, Allocator.TempJob);

            var i = new NativeList<int>(Allocator.TempJob);
            var v = new NativeList<float3>(Allocator.TempJob);
            var u = new NativeList<float2>(Allocator.TempJob);

            var handle = new GridGenerator.CreateGridJob
            {
                Vertices    = vertices,
                Indices     = indices,
                Uvs         = uvs,
                GridSize    = GameSettings.MapSettingsInstance.mapSize,
            }.Schedule();

            handle = new Terraced.TerracedJob
            {
                uvs      = uvs,
                indices  = indices,
                vertices = vertices,
                
                entity = entity,
                bufferFromEntity = GetBufferFromEntity<MapPointBuffer>(),

                i = i,
                v = v,
                u = u,
            }.Schedule(handle);

            handle.Complete();
            
            var mesh = new Mesh();
            
            mesh.SetVertices<float3>(v);
            mesh.SetIndices<int>(i, MeshTopology.Triangles, 0);
            mesh.SetUVs<float2>(0, u);
            mesh.RecalculateNormals();
            
            v.Dispose();
            i.Dispose();
            u.Dispose();
            
            EntityManager.AddSharedComponentData(entity, new RenderMesh
            {
                material        = GameSettings.MapMaterialInstance,
                castShadows     = ShadowCastingMode.On,
                receiveShadows  = true,
                mesh            = mesh,
                subMesh         = 0,
                layer           = 0
            });

            EntityManager.RemoveComponent<TagTerracedMapNeedBuild>(entity);
        });
    }
}