using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes
{
    private const int IsoLevel = 1;
    private const int ChunkSize = 32;

    private readonly byte[,,] _voxels;
    private readonly float _cubeSize;
    private readonly Material _cubeMaterial;
    private readonly Vector3Int _worldSize;
    private readonly GameObject _worldRoot;

    private readonly float[] _cubeCorners = new float[8];
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();

    public MarchingCubes(byte[,,] voxels, float cubeSize, Material cubeMaterial)
    {
        _voxels = voxels;
        _cubeSize = cubeSize;
        _cubeMaterial = cubeMaterial;
        _worldSize = new Vector3Int(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2));
        _worldRoot = new GameObject("World");
    }

    public void MarchCubes()
    {
        for (int cx = 0; cx < _worldSize.x; cx += ChunkSize)
            for (int cy = 0; cy < _worldSize.y; cy += ChunkSize)
                for (int cz = 0; cz < _worldSize.z; cz += ChunkSize)
                    GenerateChunk(cx, cy, cz);
    }

    private void GenerateChunk(int startX, int startY, int startZ)
    {
        _vertices.Clear();
        _triangles.Clear();

        int endX = Mathf.Min(startX + ChunkSize, _worldSize.x - 1);
        int endY = Mathf.Min(startY + ChunkSize, _worldSize.y - 1);
        int endZ = Mathf.Min(startZ + ChunkSize, _worldSize.z - 1);

        for (int x = startX; x < endX; x++)
            for (int y = startY; y < endY; y++)
                for (int z = startZ; z < endZ; z++)
                    MarchCube(new Vector3Int(x, y, z));

        if (_vertices.Count > 0)
            CreateMeshChunk();
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
            Vector3 vertex = (edgeStart + edgeEnd) * 0.5f * _cubeSize;

            _vertices.Add(vertex);
            _triangles.Add(_vertices.Count - 1);
        }
    }

    private void CreateMeshChunk()
    {
        GameObject go = new("Chunk");
        go.transform.parent = _worldRoot.transform;
        go.transform.localPosition = Vector3.zero;

        Mesh mesh = new();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>().material = _cubeMaterial;

        mesh.SetVertices(_vertices);
        mesh.SetTriangles(_triangles, 0);
        mesh.RecalculateNormals();

        mf.mesh = mesh;
    }
}
