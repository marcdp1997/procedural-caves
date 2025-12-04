using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes
{
    private const int IsoLevel = 1;
    private const int ChunkSize = 18;

    private byte[,,] _voxels;
    private readonly float _voxelScale;
    private readonly Vector3Int _worldSize;
    private readonly List<Vector3> _vertices;
    private readonly List<int> _triangles;
    private readonly float[] _cubeCorners;
    private readonly Material _cubeMaterial;
    private readonly GameObject _worldRoot;
    private readonly Dictionary<Vector3Int, ChunkData> _chunks;

    private class ChunkData
    {
        public Object ChunkObject;
        public int Hash;
    }

    public MarchingCubes(Vector3Int worldSize, float voxelScale, Material cubeMaterial)
    {
        _voxels = new byte[worldSize.x, worldSize.y, worldSize.z];
        _voxelScale = voxelScale;
        _worldSize = worldSize;

        _vertices = new();
        _triangles = new();
        _cubeCorners = new float[8];
        _chunks = new();

        _cubeMaterial = cubeMaterial;
        _worldRoot = new GameObject("World");
    }

    public void MarchCubes(byte[,,] voxels)
    {
        _voxels = voxels;

        for (int cx = 0; cx < _worldSize.x; cx += ChunkSize)
        {
            for (int cy = 0; cy < _worldSize.y; cy += ChunkSize)
            {
                for (int cz = 0; cz < _worldSize.z; cz += ChunkSize)
                {
                    GenerateChunk(cx, cy, cz);
                }
            }
        }
    }

    public void MarchCubes(byte[,,] voxels, Vector3Int voxelPos)
    {
        _voxels = voxels;

        Vector3Int voxelChunk = new(
            (voxelPos.x / ChunkSize) * ChunkSize,
            (voxelPos.y / ChunkSize) * ChunkSize,
            (voxelPos.z / ChunkSize) * ChunkSize
        );

        for (int cx = 0; cx < _worldSize.x; cx += ChunkSize)
        {
            for (int cy = 0; cy < _worldSize.y; cy += ChunkSize)
            {
                for (int cz = 0; cz < _worldSize.z; cz += ChunkSize)
                {
                    // Affected chunks → chunk of the voxel position and its 26 neighbours
                    if (Mathf.Abs(cx - voxelChunk.x) > ChunkSize ||
                        Mathf.Abs(cy - voxelChunk.y) > ChunkSize ||
                        Mathf.Abs(cz - voxelChunk.z) > ChunkSize)
                        continue;

                    Vector3Int chunkCoord = new(cx, cy, cz);
                    if (!_chunks.TryGetValue(chunkCoord, out ChunkData chunkData))
                        continue;

                    // Only update chunks with different voxel values
                    int currHash = CalculateChunkHash(cx, cy, cz);
                    if (currHash == chunkData.Hash) continue;

                    Object.Destroy(chunkData.ChunkObject);
                    GenerateChunk(cx, cy, cz);
                }
            }
        }
    }

    private void GenerateChunk(int startX, int startY, int startZ)
    {
        _vertices.Clear();
        _triangles.Clear();

        int endX = Mathf.Min(startX + ChunkSize + 1, _worldSize.x);
        int endY = Mathf.Min(startY + ChunkSize + 1, _worldSize.y);
        int endZ = Mathf.Min(startZ + ChunkSize + 1, _worldSize.z);

        for (int x = startX; x < endX; x++)
            for (int y = startY; y < endY; y++)
                for (int z = startZ; z < endZ; z++)
                    MarchCube(new Vector3Int(x, y, z));

        if (_vertices.Count > 0)
            AddChunk(startX, startY, startZ);
    }

    private int CalculateChunkHash(int startX, int startY, int startZ)
    {
        int hash = 17;
        int endX = Mathf.Min(startX + ChunkSize, _worldSize.x);
        int endY = Mathf.Min(startY + ChunkSize, _worldSize.y);
        int endZ = Mathf.Min(startZ + ChunkSize, _worldSize.z);

        for (int x = startX; x < endX; x++)
            for (int y = startY; y < endY; y++)
                for (int z = startZ; z < endZ; z++)
                    hash = hash * 31 + _voxels[x, y, z];

        return hash;
    }

    private void MarchCube(Vector3Int position)
    {
        SampleCubeCorners(position);
        int configIndex = GetConfigIndex();
        if (configIndex == 0 || configIndex == 255) return;
        GenerateVerticesAndTriangles(configIndex, position);
    }

    private void SampleCubeCorners(Vector3Int position)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = position + MarchingTable.Corners[i];
            corner.x = Mathf.Clamp(corner.x, 0, _worldSize.x - 1);
            corner.y = Mathf.Clamp(corner.y, 0, _worldSize.y - 1);
            corner.z = Mathf.Clamp(corner.z, 0, _worldSize.z - 1);
            _cubeCorners[i] = _voxels[corner.x, corner.y, corner.z];
        }
    }

    private int GetConfigIndex()
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
            if (_cubeCorners[i] == IsoLevel)
                configIndex |= 1 << i;

        return configIndex;
    }

    private void GenerateVerticesAndTriangles(int configIndex, Vector3 cubePos) 
    {
        for (int i = 0; i < MarchingTable.Triangles.GetLength(1); i++)
        {
            int edgeIndex = MarchingTable.Triangles[configIndex, i];
            if (edgeIndex == -1) break;

            Vector3 edgeStart = cubePos + MarchingTable.Edges[edgeIndex, 0];
            Vector3 edgeEnd = cubePos + MarchingTable.Edges[edgeIndex, 1];
            Vector3 vertex = _voxelScale * 0.5f * (edgeStart + edgeEnd);

            _vertices.Add(vertex);
            _triangles.Add(_vertices.Count - 1);
        }
    }

    private void AddChunk(int startX, int startY, int startZ)
    {
        Vector3Int chunkCoord = new(startX, startY, startZ);
        GameObject chunkObject = CreateMesh($"Chunk({startX}, {startY}, {startZ})");
        int hash = CalculateChunkHash(startX, startY, startZ);

        _chunks[chunkCoord] = new ChunkData { ChunkObject = chunkObject, Hash = hash };
    }

    private GameObject CreateMesh(string name)
    {
        GameObject go = new(name);
        go.transform.parent = _worldRoot.transform;
        go.transform.localPosition = Vector3.zero;
        go.layer = LayerMask.NameToLayer("Terrain");

        Mesh mesh = new();
        mesh.SetVertices(_vertices);
        mesh.SetTriangles(_triangles, 0);
        mesh.RecalculateNormals();

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = _cubeMaterial;

        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        return go;
    }
}
