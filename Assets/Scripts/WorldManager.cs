using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Vector3Int _worldSize;
    [SerializeField] private float _cubeSize;
    [SerializeField] private Material _cubeMaterial;

    [Header("Center")]
    [SerializeField] private int _centerRadius;

    [Header("Rooms")]
    [SerializeField] private float _numRooms;
    [SerializeField] private List<int> _roomSizes;

    [Header("Paths")]
    [SerializeField] private List<int> _pathSizes;
    [SerializeField] [Range(0, 100)] private int _randomPathPerc;

    private byte[,,] _voxels;
    private List<Vector3Int> _roomsCenter;

    private void Start()
    {
        PopulateVoxels();
        CreateCenter();
        CreateRooms();
        CreateConnections();

        new WorldCreator(_voxels, _cubeSize, _cubeMaterial);
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
                    _voxels[x, y, z] = 1;
                }
            }
        }
    }  

    private void CreateCenter()
    {
        int cylinderCenterX = _worldSize.x / 2;
        int cylinderCenterZ = _worldSize.z / 2;
        Vector3Int a = new Vector3Int(cylinderCenterX, 0, cylinderCenterZ);
        Vector3Int b = new Vector3Int(cylinderCenterX, _worldSize.y, cylinderCenterZ);

        CreatePath(a, b, _centerRadius);
    }

    private void CreateRooms()
    {
        _roomsCenter = new List<Vector3Int>();

        for (int i = 0; i < _numRooms; i++)
        {
            int roomSizeIndex = Random.Range(0, _roomSizes.Count);
            int roomHalfSize = (int)(_roomSizes[roomSizeIndex] * 0.5f);

            int x = Random.Range(roomHalfSize, _worldSize.x - roomHalfSize);
            int y = Random.Range(roomHalfSize, _worldSize.y - roomHalfSize);
            int z = Random.Range(roomHalfSize, _worldSize.z - roomHalfSize);
            CreateRoom(new Vector3Int(x, y, z), _roomSizes[roomSizeIndex]);
        }
    }

    private void CreateRoom(Vector3Int position, float roomSize)
    {
        int roomHalfSize = (int)(roomSize * 0.5f);
        int initPosX = position.x - roomHalfSize;
        int initPosY = position.y - roomHalfSize;
        int initPosZ = position.z - roomHalfSize;

        for (int x = initPosX; x < initPosX + roomSize; x++)
        {
            for (int y = initPosY; y < initPosY + roomSize; y++)
            {
                for (int z = initPosZ; z < initPosZ + roomSize; z++)
                {
                     _voxels[x, y, z] = 0;
                }
            }
        }

        _roomsCenter.Add(position);
    }

    private void CreateConnections()
    {
        RoomCommunicator roomCommunicator = new RoomCommunicator(_roomsCenter, _randomPathPerc);
        List<(Vector3Int, Vector3Int)> connections = roomCommunicator.DetermineConnections();

        for (int i = 0; i < connections.Count; i++)
            CreatePath(connections[i].Item1, connections[i].Item2, _pathSizes[Random.Range(0, _pathSizes.Count)]);
    }

    private void CreatePath(Vector3Int a, Vector3Int b, float radius)
    {
        for (int x = 0; x < _worldSize.x; x++)
        {
            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int z = 0; z < _worldSize.z; z++)
                {
                    float distance = DistancePointLine(new Vector3Int(x, y, z), a, b);

                    if (distance <= radius)
                    {
                        _voxels[x, y, z] = 0;
                    }
                }
            }
        }
    }

    private float DistancePointLine(Vector3Int point, Vector3Int linePointA, Vector3Int linePointB)
    {
        Vector3 direction = linePointB - linePointA;

        float t = Vector3.Dot(point - linePointA, direction) / Vector3.Dot(direction, direction);
        Vector3 closestPoint = linePointA + Mathf.Clamp01(t) * direction;

        return Vector3.Distance(point, closestPoint);
    }
}
