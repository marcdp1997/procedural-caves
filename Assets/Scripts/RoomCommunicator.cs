using System.Collections.Generic;
using UnityEngine;

public class RoomCommunicator
{
    private int _randomConnection;
    private List<Vector3Int> _roomsCenter;
    private List<(Vector3Int, Vector3Int)> _connections;

    public RoomCommunicator(List<Vector3Int> roomsCenter, int randomConnection)
    {
        _roomsCenter = roomsCenter;
        _randomConnection = randomConnection;
        _connections = new List<(Vector3Int, Vector3Int)>();
    }

    public List<(Vector3Int, Vector3Int)> DetermineConnections()
    {
        for (int i = 0; i < _roomsCenter.Count - 1; i++)
        {
            float closestDistance = 0;
            int closestRoomIndex = 0;

            for (int j = i + 1; j < _roomsCenter.Count; j++)
            {
                if (Random.Range(0, 100) < _randomConnection)
                    AddConnection(_roomsCenter[i], _roomsCenter[j]);

                float distance = Vector3.Distance(_roomsCenter[i], _roomsCenter[j]);
                if (closestDistance == 0 || distance < closestDistance)
                {
                    closestRoomIndex = j;
                    closestDistance = distance;
                }
            }

            AddConnection(_roomsCenter[i], _roomsCenter[closestRoomIndex]);
        }

        return _connections;
    }

    private void AddConnection(Vector3Int room1, Vector3Int room2)
    {
        if (!_connections.Contains((room1, room2)))
            _connections.Add((room1, room2));
    }
}
