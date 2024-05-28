using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ZoneData
{
    public int Radius;
    public int NumRooms;
    public int RoomRadius;
    public int PathRadius;
}

public struct RoomData
{
    public Vector3Int Center;
    public int ZoneId;
}

public class WorldManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Vector3Int _worldSize;
    [SerializeField] private int _cubeSize;
    [SerializeField] private Material _cubeMaterial;

    [Header("Zones")]
    [SerializeField] private int _centerRadius;
    [SerializeField] [Range(0, 100)] private int _randomConnections;
    [SerializeField] private List<ZoneData> _zonesData;

    private byte[,,] _voxels;
    private Vector3Int _worldCenter;
    private List<RoomData> _roomsData;

    private void Awake()
    {
        _voxels = new byte[_worldSize.x, _worldSize.y, _worldSize.z];
        _worldCenter = _worldSize / 2;
        _roomsData = new List<RoomData>();
    }

    private void Start()
    {
        CreateWorld();
        new WorldCreator(_voxels, _cubeSize, _cubeMaterial);
    }

    private void CreateWorld()
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
            Vector3Int a = new Vector3Int(_worldCenter.x, 0, _worldCenter.z);
            Vector3Int b = new Vector3Int(_worldCenter.x, _worldSize.y, _worldCenter.z);

            CreatePath(a, b, centerRadius);
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

        _roomsData.Add(new RoomData { Center = roomCenter, ZoneId = zoneId });
    }

    private Vector3Int GetRoomRandomPosition(int zoneId)
    {
        int prevZoneRadius = zoneId > 0 ? _zonesData[zoneId - 1].Radius : _centerRadius;
        int zoneRadius = _zonesData[zoneId].Radius - prevZoneRadius;
        int roomRadius = _zonesData[zoneId].RoomRadius;

        Vector3Int roomCenter = new Vector3Int(
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
        RoomCommunicator roomCommunicator = new RoomCommunicator(_roomsData, _randomConnections);
        List<(RoomData, RoomData)> connections = roomCommunicator.DetermineConnections();

        for (int i = 0; i < connections.Count; i++)
        {
            int room1ZoneId = connections[i].Item1.ZoneId;
            int room2ZoneId = connections[i].Item2.ZoneId;
            int pathSize = (room1ZoneId >= room2ZoneId) ? _zonesData[room1ZoneId].PathRadius : _zonesData[room2ZoneId].PathRadius;
            CreatePath(connections[i].Item1.Center, connections[i].Item2.Center, pathSize);
        }
    }

    private void CreatePath(Vector3Int a, Vector3Int b, int radius)
    {
        Vector3Int minBounds = Vector3Int.Min(a, b) - new Vector3Int(radius, radius, radius);
        Vector3Int maxBounds = Vector3Int.Max(a, b) + new Vector3Int(radius, radius, radius) ;
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

    public Vector3Int GetWorldSize() { return _worldSize; }

    public int GetCenterRadius() { return _centerRadius; }

    public List<ZoneData> GetZonesData() { return _zonesData; }

    public int GetRandomConnections() { return _randomConnections; }

    public List<RoomData> GetRoomsData() { return _roomsData; }
}
