using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomCommunicator
{
    private int _randomConnections;
    private List<RoomData> _roomsData;
    private List<(RoomData, RoomData)> _connections;

    private List<Edge> _edges;
    private List<Edge> _mst;

    public RoomCommunicator(List<RoomData> roomsData, int randomConnections)
    {
        _roomsData = roomsData;
        _randomConnections = randomConnections;

        _edges = new List<Edge>();
        _mst = new List<Edge>();
        _connections = new List<(RoomData, RoomData)>();
    }

    public List<(RoomData, RoomData)> DetermineConnections()
    {
        for (int i = 0; i < _roomsData.Count; i++)
        {
            for (int j = i + 1; j < _roomsData.Count; j++)
            {
                float distance = Vector3.Distance(_roomsData[i].Center, _roomsData[j].Center);
                _edges.Add(new Edge(i, j, distance));
            }
        }

        _edges.Sort();

        UnionFind uf = new UnionFind(_roomsData.Count);

        foreach (Edge edge in _edges)
        {
            if (uf.Find(edge.Point1) != uf.Find(edge.Point2))
            {
                uf.Union(edge.Point1, edge.Point2);
                _mst.Add(edge);
            }
        }

        foreach (Edge edge in _mst)
            AddConnection(_roomsData[edge.Point1], _roomsData[edge.Point2]);

        return _connections;
    }

    private void AddConnection(RoomData room1, RoomData room2)
    {
        _connections.Add((room1, room2));
    }
}

public class Edge : IComparable<Edge>
{
    public int Point1 { get; private set; }
    public int Point2 { get; private set; }
    public float Weight { get; private set; }

    public Edge(int point1, int point2, float weight)
    {
        Point1 = point1;
        Point2 = point2;
        Weight = weight;
    }

    public int CompareTo(Edge other)
    {
        return Weight.CompareTo(other.Weight);
    }
}

public class UnionFind
{
    private int[] _parent;
    private int[] _rank;

    public UnionFind(int size)
    {
        _parent = new int[size];
        _rank = new int[size];

        for (int i = 0; i < size; i++)
        {
            _parent[i] = i;
            _rank[i] = 0;
        }
    }

    public int Find(int x)
    {
        if (_parent[x] != x)
        {
            _parent[x] = Find(_parent[x]);
        }
        return _parent[x];
    }

    public void Union(int x, int y)
    {
        int rootX = Find(x);
        int rootY = Find(y);

        if (rootX != rootY)
        {
            if (_rank[rootX] > _rank[rootY])
            {
                _parent[rootY] = rootX;
            }
            else if (_rank[rootX] < _rank[rootY])
            {
                _parent[rootX] = rootY;
            }
            else
            {
                _parent[rootY] = rootX;
                _rank[rootX]++;
            }
        }
    }
}
