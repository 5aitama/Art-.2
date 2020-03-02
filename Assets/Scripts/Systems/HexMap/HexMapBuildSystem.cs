using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Rendering;

public class HexMapBuildSystem : ComponentSystem
{
    
    protected override void OnUpdate()
    {
        Entities.WithAll<TagHexMap, TagHexMapNeedBuild>().ForEach((Entity entity) =>
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
            
            var i = new NativeList<int>(Allocator.TempJob);
            var v = new NativeList<float3>(Allocator.TempJob);
            var u = new NativeList<float2>(Allocator.TempJob);

            new Terraced.TerracedHexMapJob
            {
                maxHeight = (int)GameSettings.MapSettingsInstance.noiseAmplitude,

                entity = entity,
                cellDataBufferFromEntity = GetBufferFromEntity<HexCellDataBuffer>(),

                i = i,
                v = v,
                u = u,
            }.Schedule().Complete();
            
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

            EntityManager.RemoveComponent<TagHexMapNeedBuild>(entity);
        });    
    }
}