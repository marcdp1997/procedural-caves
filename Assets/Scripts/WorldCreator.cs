using System.Collections.Generic;
using UnityEngine;

public class WorldCreator
{
    private byte[,,] _voxels;
    private GameObject _worldRoot;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();

    private Vector3Int _worldSize;
    private int _lastVertex;
    private float _cubeSize;
    private Material _cubeMaterial;

    private const int MaxVertices = 40000;

    public WorldCreator(byte[,,] voxels, float cubeSize, Material cubeMaterial)
    {
        _voxels = voxels;
        _cubeSize = cubeSize;
        _cubeMaterial = cubeMaterial;
        _worldRoot = new GameObject("World Root");
        _worldSize = new Vector3Int(_voxels.GetLength(0), _voxels.GetLength(1), _voxels.GetLength(2));

        CreateWorld();
    }

    private void CreateWorld()
    {
        for (int x = 0; x < _voxels.GetLength(0); x++)
        {
            for (int y = 0; y < _voxels.GetLength(1); y++)
            {
                for (int z = 0; z < _voxels.GetLength(2); z++)
                {
                    if (_voxels[x, y, z] == 1)
                        CreateCube(new Vector3Int(x, y, z));
                }
            }
        }

        CreateMesh();
    }

    private void CreateCube(Vector3Int c)
    {
        //Front
        if (c.z - 1 > 0 && _voxels[c.x, c.y, c.z - 1] == 0)
            GenerateFace(c, Right, Up);

        //Right
        if (c.x + 1 < _voxels.GetLength(0) && _voxels[c.x + 1, c.y, c.z] == 0)
            GenerateFace(c + Right, Forward, Up);

        //Left
        if (c.x - 1 > 0 && _voxels[c.x - 1, c.y, c.z] == 0)
            GenerateFace(c + Forward, Back, Up);

        //Back
        if (c.z + 1 < _voxels.GetLength(2) && _voxels[c.x, c.y, c.z + 1] == 0)
            GenerateFace(c + Forward + Right, Left, Up);

        //Bottom
        if (c.y - 1 > 0 && _voxels[c.x, c.y - 1, c.z] == 0)
            GenerateFace(c + Forward, Right, Back);

        //Top
        if (c.y + 1 < _voxels.GetLength(1) && _voxels[c.x, c.y + 1, c.z] == 0)
            GenerateFace(c + Up, Right, Forward);
    }

    private void GenerateFace(Vector3 startPos, Vector3 right, Vector3 up)
    {
        Vector3Int offset = new Vector3Int((int)(_worldSize.x * 0.5f), _worldSize.y, (int)(_worldSize.z * 0.5f));

        startPos = (startPos - offset) * _cubeSize;
        right *= _cubeSize;
        up *= _cubeSize;

        _lastVertex = _vertices.Count;

        _vertices.Add(startPos); //0
        _vertices.Add(startPos + right); //1
        _vertices.Add(startPos + up); //2
        _vertices.Add(startPos + up + right); //3

        _triangles.Add(_lastVertex);
        _triangles.Add(_lastVertex + 2);
        _triangles.Add(_lastVertex + 3);

        _triangles.Add(_lastVertex);
        _triangles.Add(_lastVertex + 3);
        _triangles.Add(_lastVertex + 1);

        TryCreateMesh();
    }

    private void TryCreateMesh()
    {
        if (_vertices.Count >= MaxVertices)
            CreateMesh();
    }

    private void CreateMesh()
    {
        GameObject go = new GameObject("CubeMesh");
        go.transform.parent = _worldRoot.transform;

        Mesh mesh = new Mesh();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>().material = _cubeMaterial;

        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        meshFilter.mesh = mesh;

        _vertices.Clear();
        _triangles.Clear();
        _lastVertex = 0;
    }

    private Vector3 Forward { get { return Vector3.forward; } }
    private Vector3 Back { get { return Vector3.back; } }
    private Vector3 Up { get { return Vector3.up; } }
    private Vector3 Right { get { return Vector3.right; } }
    private Vector3 Left { get { return Vector3.left; } }
}
