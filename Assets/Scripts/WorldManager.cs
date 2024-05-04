using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Vector3Int _worldSize;
    [SerializeField] private float _cubeSize;
    [SerializeField] private Material _cubeMaterial;

    [Header("Center")]
    [SerializeField] private float _centerRadius;

    [Header("Rooms")]
    [SerializeField] private float _numRooms;
    [SerializeField] private List<float> _roomSizes;

    private byte[,,] _voxels;

    private void Start()
    {
        PopulateVoxels();
        CreateCenterCylinder();
        CreateRooms();
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

    private void CreateCenterCylinder()
    {
        int cylinderCenterX = _worldSize.x / 2;
        int cylinderTopY = _worldSize.y;
        int cylinderCenterZ = _worldSize.z / 2;

        for (int x = 0; x < _worldSize.y; x++)
        {
            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int z = 0; z < _worldSize.z; z++)
                {
                    int distanceSquared = (int)(Mathf.Pow(x - cylinderCenterX, 2) + Mathf.Pow(z - cylinderCenterZ, 2));

                    if (distanceSquared <= Mathf.Pow(_centerRadius, 2) && y < cylinderTopY)
                        _voxels[x, y, z] = 0;
                }
            }
        }
    }

    private void CreateRooms()
    {
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
    }
}
