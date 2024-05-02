using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    [SerializeField] private int _random;
    [SerializeField] private float _cubeSize;
    [SerializeField] private Vector3Int _worldSize;

    private Mesh _mesh;
    private int _lastVertex;
    private MeshFilter _meshFilter;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private byte[,,] _voxels;

    private Vector3 Forward { get { return Vector3.forward; } }
    private Vector3 Back { get { return Vector3.back; } }
    private Vector3 Up { get { return Vector3.up; } }
    private Vector3 Right { get { return Vector3.right; } }
    private Vector3 Left { get { return Vector3.left; } }

    private void Start()
    {
        PopulateVoxels();
        CreateVoxels();

        _mesh = new Mesh();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();

        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = _mesh;
    }

    private void PopulateVoxels()
    {
        _voxels = new byte[_worldSize.x, _worldSize.y, _worldSize.z];

        for (int x = 0; x < _worldSize.x; x++)
        {
            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int z = 0; z < _worldSize.z; z++)
                {
                    _voxels[x, y, z] = Random.Range(0, 100) >= _random ? (byte)1 : (byte)0;
                }
            }
        }
    }

    private void CreateVoxels()
    {
        for (int x = 0; x < _worldSize.x; x++)
        {
            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int z = 0; z < _worldSize.z; z++)
                {
                    if (_voxels[x, y, z] == 1)
                        CreateCube(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private void CreateCube(Vector3Int c)
    {
        //Front
        if (c.z - 1 < 0 || _voxels[c.x, c.y, c.z - 1] == 0)
            GenerateFace(c, Right, Up);

        //Right
        if (c.x + 1 >= _worldSize.x || _voxels[c.x + 1, c.y, c.z] == 0)
            GenerateFace(c + Right, Forward, Up);

        //Left
        if (c.x - 1 < 0 || _voxels[c.x - 1, c.y, c.z] == 0)
            GenerateFace(c + Forward, Back, Up);

        //Back
        if (c.z + 1 >= _worldSize.z || _voxels[c.x, c.y, c.z + 1] == 0)
            GenerateFace(c + Forward + Right, Left, Up);

        //Bottom
        if (c.y - 1 < 0 || _voxels[c.x, c.y - 1, c.z] == 0)
            GenerateFace(c + Forward, Right, Back);

        //Top
        if (c.y + 1 >= _worldSize.y || _voxels[c.x, c.y + 1, c.z] == 0)
            GenerateFace(c + Up, Right, Forward);
    }

    // 2 3 | Clock-wise to 
    // 0 1 | define normals
    private void GenerateFace(Vector3 startPos, Vector3 right, Vector3 up)
    {
        startPos = (startPos - ((Vector3)_worldSize * 0.5f)) * _cubeSize;
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
    }
}
