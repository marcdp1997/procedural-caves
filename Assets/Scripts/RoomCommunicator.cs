using System.Collections.Generic;
using UnityEngine;

public class RoomCommunicator
{
    private int _randomConnections;
    private List<RoomData> _roomsData;
    private List<(RoomData, RoomData)> _connections;

    public RoomCommunicator(List<RoomData> roomsData, int randomConnections)
    {
        _roomsData = roomsData;
        _randomConnections = randomConnections;
        _connections = new List<(RoomData, RoomData)>();
    }

    public List<(RoomData, RoomData)> DetermineConnections()
    {
        for (int i = 0; i < _roomsData.Count; i++)
        {
            float closestDistance = float.MaxValue;
            int closestRoomIndex = -1;

            for (int j = 0; j < _roomsData.Count; j++)
            {
                if (i == j) continue;

                if (Random.Range(0, 100) < _randomConnections)
                {
                    AddConnection(_roomsData[i], _roomsData[j]);
                    continue;
                }

                float distance = Vector3.Distance(_roomsData[i].Center, _roomsData[j].Center);
                if (distance < closestDistance && !ConnectionExists(_roomsData[i], _roomsData[j]))
                {
                    closestRoomIndex = j;
                    closestDistance = distance;
                }
            }

            AddConnection(_roomsData[i], _roomsData[closestRoomIndex]);
        }

        return _connections;
    }

    private bool ConnectionExists(RoomData room1, RoomData room2)
    {
        return _connections.Contains((room1, room2)) || _connections.Contains((room2, room1));
    }

    private void AddConnection(RoomData room1, RoomData room2)
    {
        _connections.Add((room1, room2));
    }
}
