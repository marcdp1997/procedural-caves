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
    [SerializeField] private float _roomSize;

    private byte[,,] _voxels;

    private void Start()
    {
        PopulateVoxels();
        CreateCenterCylinder();
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
        int cylinderCenterY = _worldSize.y / 2;
        int cylinderTopZ = (_worldSize.z - _worldSize.y) / 2 + _worldSize.y;

        for (int x = 0; x < _worldSize.x; x++)
        {
            for (int y = 0; y < _worldSize.y; y++)
            {
                for (int z = 0; z < _worldSize.z; z++)
                {
                    int distanceSquared = (x - cylinderCenterX) * (x - cylinderCenterX) +
                                          (y - cylinderCenterY) * (y - cylinderCenterY);

                    if (distanceSquared <= _centerRadius * _centerRadius &&
                        z >= (_worldSize.z - _worldSize.y) / 2 && z < cylinderTopZ)
                        _voxels[x, y, z] = 0;
                }
            }
        }
    }

    private void CreateRooms()
    {
        for (int i = 0; i < _numRooms; i++)
        {

        }
    }
}
