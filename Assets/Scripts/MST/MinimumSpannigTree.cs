using System.Collections.Generic;
using UnityEngine;

public class MinimumSpannigTree
{
    private readonly List<RoomData> _rooms;
    private readonly List<Connection> _connections;
    private readonly List<Edge> _edges;
    private readonly List<Edge> _mst;

    public MinimumSpannigTree(List<RoomData> rooms)
    {
        _edges = new();
        _mst = new();
        _connections = new();
        _rooms = rooms;
    }

    public List<Connection> GenerateConnections()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            for (int j = i + 1; j < _rooms.Count; j++)
            {
                float distance = Vector3.Distance(_rooms[i].Center, _rooms[j].Center);
                _edges.Add(new Edge(i, j, distance));
            }
        }

        _edges.Sort();

        UnionFind uf = new(_rooms.Count);

        foreach (Edge edge in _edges)
        {
            if (uf.Find(edge.Point1) != uf.Find(edge.Point2))
            {
                uf.Union(edge.Point1, edge.Point2);
                _mst.Add(edge);
            }
        }

        foreach (Edge edge in _mst)
            AddConnection(_rooms[edge.Point1], _rooms[edge.Point2]);

        return _connections;
    }

    private void AddConnection(RoomData roomA, RoomData roomB)
    {
        Connection connection = new()
        {
            RoomA = roomA,
            RoomB = roomB
        };

        _connections.Add(connection);
    }
}

