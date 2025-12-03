using System;

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
