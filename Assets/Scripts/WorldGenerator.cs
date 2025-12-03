using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool _drawGizmos;
    [SerializeField] private Vector3Int _worldSize;
    [SerializeField] private int _cubeSize;
    [SerializeField] private Material _cubeMaterial;

    [Header("Zones")]
    [SerializeField] private int _centerRadius;
    [SerializeField] private List<ZoneData> _zonesData;

    private byte[,,] _voxels;
    private Vector3Int _worldCenter;
    private List<RoomData> _rooms;

    private void Awake()
    {
        _voxels = new byte[_worldSize.x, _worldSize.y, _worldSize.z];
        _worldCenter = _worldSize / 2;
        _rooms = new List<RoomData>();
    }

    private void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        PopulateVoxels();
        MarchingCubes marchingCubes = new(_voxels, _cubeSize, _cubeMaterial);
        marchingCubes.MarchCubes();
    }

    private void PopulateVoxels()
    {
        CreateCenter();

        for (int i = 0; i < _zonesData.Count; i++)
            CreateRooms(i);

        CreateConnections();
    }

    private void CreateCenter()
    {
        int centerRadius = _centerRadius;

        if (centerRadius > 0)
        {
            Vector3Int a = new(_worldCenter.x, 0, _worldCenter.z);
            Vector3Int b = new(_worldCenter.x, _worldSize.y, _worldCenter.z);

            CreatePath(a, b, centerRadius);
            _rooms.Add(new RoomData { Center = _worldCenter, ZoneId = -1 });
        }
    }

    private void CreateRooms(int zoneId)
    {
        int numRooms = _zonesData[zoneId].NumRooms;

        for (int i = 0; i < numRooms; i++)
            CreateRoom(zoneId);
    }

    private void CreateRoom(int zoneId)
    {
        Vector3Int roomCenter = GetRoomRandomPosition(zoneId);

        int radius = _zonesData[zoneId].RoomRadius;
        Vector3Int minBounds = roomCenter - new Vector3Int(radius, radius, radius);
        Vector3Int maxBounds = roomCenter + new Vector3Int(radius, radius, radius);

        for (int x = minBounds.x; x < maxBounds.x; x++)
        {
            for (int y = minBounds.y; y < maxBounds.y; y++)
            {
                for (int z = minBounds.z; z < maxBounds.z; z++)
                {
                    if (Vector3.Distance(new Vector3Int(x, y, z), roomCenter) <= radius)
                        _voxels[x, y, z] = 1;
                }
            }
        }

        _rooms.Add(new RoomData { Center = roomCenter, ZoneId = zoneId });
    }

    private Vector3Int GetRoomRandomPosition(int zoneId)
    {
        int prevZoneRadius = zoneId > 0 ? _zonesData[zoneId - 1].Radius : _centerRadius;
        int zoneRadius = _zonesData[zoneId].Radius - prevZoneRadius;
        int roomRadius = _zonesData[zoneId].RoomRadius;

        Vector3Int roomCenter = new(
            Random.Range(_worldCenter.x - zoneRadius + roomRadius, _worldCenter.x + zoneRadius - roomRadius),
            Random.Range(roomRadius, _worldSize.y - roomRadius),
            Random.Range(_worldCenter.z - zoneRadius + roomRadius, _worldCenter.z + zoneRadius - roomRadius)
        );

        Vector3 roomToCenterLine = (roomCenter - new Vector3(_worldCenter.x, roomCenter.y, _worldCenter.z));
        roomCenter += Vector3Int.RoundToInt(roomToCenterLine.normalized * prevZoneRadius);

        return roomCenter;
    }

    private void CreateConnections()
    {
        MinimumSpannigTree mst = new(_rooms);
        List<Connection> connections = mst.GenerateConnections();

        for (int i = 0; i < connections.Count; i++)
        {
            int room1ZoneId = connections[i].RoomA.ZoneId;
            int room2ZoneId = connections[i].RoomB.ZoneId;
            int pathSize = (room1ZoneId >= room2ZoneId) ? _zonesData[room1ZoneId].PathRadius : _zonesData[room2ZoneId].PathRadius;
            CreatePath(connections[i].RoomA.Center, connections[i].RoomB.Center, pathSize);
        }
    }

    private void CreatePath(Vector3Int a, Vector3Int b, int radius)
    {
        Vector3Int minBounds = Vector3Int.Min(a, b) - new Vector3Int(radius, radius, radius);
        Vector3Int maxBounds = Vector3Int.Max(a, b) + new Vector3Int(radius, radius, radius);
        Vector3 line = b - a;

        minBounds = Vector3Int.Max(minBounds, Vector3Int.zero);
        maxBounds.x = Mathf.Min(maxBounds.x, _worldSize.x - 1);
        maxBounds.y = Mathf.Min(maxBounds.y, _worldSize.y - 1);
        maxBounds.z = Mathf.Min(maxBounds.z, _worldSize.z - 1);

        for (int x = minBounds.x; x <= maxBounds.x; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                for (int z = minBounds.z; z <= maxBounds.z; z++)
                {
                    if (DistancePointLine(new Vector3Int(x, y, z), a, line) <= radius)
                        _voxels[x, y, z] = 1;
                }
            }
        }
    }

    private float DistancePointLine(Vector3Int currPos, Vector3Int origin, Vector3 line)
    {
        Vector3 projectedPoint = origin + ((currPos - origin).magnitude * line.normalized);
        return Vector3.Distance(currPos, projectedPoint);
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        Vector3 worldCenter = (_worldSize / 2) * _cubeSize;

        // World
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldCenter, _worldSize * _cubeSize);

        // Center
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldCenter, new Vector3(_centerRadius * 2, _worldSize.y, _centerRadius * 2) * _cubeSize);

        // Zones
        Gizmos.color = Color.blue;
        for (int i = 0; i < _zonesData.Count; i++)
        {
            int size = _zonesData[i].Radius;
            Gizmos.DrawWireCube(worldCenter, new Vector3(size * 2, _worldSize.y, size * 2) * _cubeSize);
        }
    }
}
