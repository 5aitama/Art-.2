using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexagonTest : MonoBehaviour
{
    public int2 mapSize   = 3;
    public float3 center  = 0f;
    public float size     = 1f;

    public int3 cellCubePosition;

    private NativeHashMap<int3, Hexagon.Hex> _map;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private void Awake()
    {
        _mesh           = new Mesh();
        _meshFilter     = GetComponent<MeshFilter>();
        _meshCollider   = GetComponent<MeshCollider>();

        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
    }

    private void Start()
    {
        _map = new NativeHashMap<int3, Hexagon.Hex>(mapSize.x * mapSize.y, Allocator.Persistent);

        var vertices = new NativeList<float3>(Allocator.Temp);
        var indices  = new NativeList<int>(Allocator.Temp);
        var uvs      = new NativeList<float2>(Allocator.Temp);

        for(var i = 0; i < mapSize.x * mapSize.y; i++)
        {
            var x = i % mapSize.x;
            var z = i / mapSize.x;

            var hex = new Hexagon.Hex
            {
                Size = size,
                OffsetCoord = new int2(x, z),
            };

            var vLength = vertices.Length;

            vertices.Add(hex.WorldCoord);
            uvs.Add(new float2(0f));

            for(var j = 6; j >= 0; j--)
            {
                vertices.Add(Hexagon.HexCorner(hex.WorldCoord, hex.Size, j));
                uvs.Add(new float2(0f));
            }

            for(var j = 0; j < 6; j++)
            {
                var a = (1 + j) % 7;
                var b = (2 + j) % 7;
                
                b = b < 1 ? 1 : b;

                indices.Add(vLength);
                indices.Add(vLength + a);
                indices.Add(vLength + b);
            }

            Debug.Log($"Offset : {hex.OffsetCoord}\nCubeCoord : {hex.CubeCoord}\nWorldPosition : {hex.WorldCoord}");

            _map.Add(hex.CubeCoord, hex);
        }

        _mesh.Clear();
        _mesh.SetVertices<float3>(vertices);
        _mesh.SetIndices<int>(indices, MeshTopology.Triangles, 0);
        _mesh.SetUVs<float2>(0, uvs);
        _mesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        _map.Dispose();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad8))
            cellCubePosition += HexDirection.Top;

        if(Input.GetKeyDown(KeyCode.Keypad7))
            cellCubePosition += HexDirection.TopLeft;

        if(Input.GetKeyDown(KeyCode.Keypad9))
            cellCubePosition += HexDirection.TopRight;

        if(Input.GetKeyDown(KeyCode.Keypad2))
            cellCubePosition += HexDirection.Bottom;

        if(Input.GetKeyDown(KeyCode.Keypad1))
            cellCubePosition += HexDirection.BottomLeft;

        if(Input.GetKeyDown(KeyCode.Keypad3))
            cellCubePosition += HexDirection.BottomRight;
    }

    private void OnDrawGizmos()
    {
        if(!_map.IsCreated) return;

        var keys = _map.GetKeyArray(Allocator.Temp);

        for(var i = 0; i < keys.Length; i++)
        {
            var selected = keys[i].x == cellCubePosition.x 
                            && keys[i].y == cellCubePosition.y 
                            && keys[i].z == cellCubePosition.z;

            // Draw center of hex
            Gizmos.color = selected ? Color.yellow : Color.white;
            Gizmos.DrawSphere(_map[keys[i]].WorldCoord, 0.25f);

            var hexagonCorners = new NativeArray<float3>(6, Allocator.Temp);

            // Draw corner of hex
            Gizmos.color = Color.white;
            for(var j = 0; j < 6; j++)
            {
                hexagonCorners[j] = Hexagon.HexCorner(_map[keys[i]].WorldCoord, _map[keys[i]].Size, j);
                Gizmos.DrawSphere(Hexagon.HexCorner(_map[keys[i]].WorldCoord, _map[keys[i]].Size, j), 0.1f);
            }

            for(var j = 0; j < 6; j++)
            {
                var a = _map[keys[i]].WorldCoord;
                var b = hexagonCorners[(j + 1) % hexagonCorners.Length];
                var c = hexagonCorners[(j + 2) % hexagonCorners.Length];

                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, a);
            }
        }
    }
}