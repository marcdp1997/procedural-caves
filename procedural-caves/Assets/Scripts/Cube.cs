using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public Vector3 Forward { get { return Vector3.forward; } }
    public Vector3 Back { get { return Vector3.back; } }
    public Vector3 Up { get { return Vector3.up; } }
    public Vector3 Right { get { return Vector3.right; } }
    public Vector3 Left { get { return Vector3.left; } }

    private Mesh _mesh;
    private int _lastVertex;
    private MeshFilter _meshFilter;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();

    private void Start()
    {
        _mesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();

        CreateCube();

        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.SetUVs(0, _uvs);

        _meshFilter.mesh = _mesh;
    }

    private void CreateCube()
    {
        GenerateFace(Vector3.zero, Right, Up);
        GenerateFace(Right, Forward, Up);
        GenerateFace(Forward, Back, Up);
        GenerateFace(Forward + Right, Left, Up);
        GenerateFace(Up, Right, Forward);
        GenerateFace(Forward, Right, Back);
    }

    // 2 3 | Clock-wise to 
    // 0 1 | define normals
    private void GenerateFace(Vector3 position, Vector3 right, Vector3 up)
    {
        _lastVertex = _vertices.Count;

        _vertices.Add(position); //0
        _vertices.Add(position + right); //1
        _vertices.Add(position + up); //2
        _vertices.Add(position + up + right); //3

        _triangles.Add(_lastVertex);
        _triangles.Add(_lastVertex + 2);
        _triangles.Add(_lastVertex + 3);

        _triangles.Add(_lastVertex);
        _triangles.Add(_lastVertex + 3);
        _triangles.Add(_lastVertex + 1);
    }
}
