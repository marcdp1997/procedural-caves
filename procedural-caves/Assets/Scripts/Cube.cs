using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    [SerializeField] private float _cubeSize;
    [SerializeField] private Vector3 _worldSize;

    private Mesh _mesh;
    private int _lastVertex;
    private MeshFilter _meshFilter;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();

    private void Start()
    {
        CreateWorld();

        _mesh = new Mesh();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.SetUVs(0, _uvs);

        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = _mesh;
    }

    private void CreateWorld()
    {
        for (int x = 0; x < _worldSize.x; x++)
        {
            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int z = 0; z < _worldSize.z; z++)
                {
                    CreateCube(new Vector3(x - _worldSize.x * 0.5f, y - _worldSize.y * 0.5f, z - _worldSize.z * 0.5f));
                }
            }
        }
    }

    private void CreateCube(Vector3 worldPosition)
    {
        Vector3 start = worldPosition * _cubeSize;
        Vector3 forward = Vector3.forward * _cubeSize;
        Vector3 back = Vector3.back * _cubeSize;
        Vector3 up = Vector3.up * _cubeSize;
        Vector3 right = Vector3.right * _cubeSize;
        Vector3 left = Vector3.left * _cubeSize;

        GenerateFace(start, right, up);
        GenerateFace(start + right, forward, up);
        GenerateFace(start + forward, back, up);
        GenerateFace(start + forward + right, left, up);
        GenerateFace(start + up, right, forward);
        GenerateFace(start + forward, right, back);
    }

    // 2 3 | Clock-wise to 
    // 0 1 | define normals
    private void GenerateFace(Vector3 startPos, Vector3 right, Vector3 up)
    {
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
