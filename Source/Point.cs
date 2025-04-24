using System;

namespace CLI;

public readonly struct Point(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;

    public Point(float x, float y) : this((int)x, (int)y) { }

    public static bool operator ==(Point a, Point b) => a.Equals(b);
    public static bool operator !=(Point a, Point b) => !a.Equals(b);

    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    public static Point operator *(Point a, Point b) => new(a.X * b.X, a.Y * b.Y);
    public static Point operator /(Point a, Point b) => new(a.X / b.X, a.Y / b.Y);
    public static Point operator *(Point a, int b) => new(a.X * b, a.Y * b);
    public static Point operator /(Point a, int b) => new(a.X / b, a.Y / b);

    public override readonly string ToString() => $"({X}, {Y})";
    public override readonly bool Equals(object? obj) => obj is Point coord && Equals(coord);
    public readonly bool Equals(Point other) => X == other.X && Y == other.Y;
    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public static implicit operator ValueTuple<int, int>(Point size) => new(size.X, size.Y);
    public static implicit operator System.Drawing.Point(Point size) => new(size.X, size.Y);
    public static implicit operator System.Numerics.Vector2(Point size) => new(size.X, size.Y);
    public static implicit operator Maths.Vector2Int(Point v) => new(v.X, v.Y);

    public static explicit operator Point(ValueTuple<int, int> size) => new(size.Item1, size.Item2);
    public static explicit operator Point(System.Drawing.Point size) => new(size.X, size.Y);
    public static explicit operator Point(System.Numerics.Vector2 size) => new(size.X, size.Y);

    /// <exception cref="OverflowException"/>
    public static explicit operator checked Point(Maths.Vector2Int size) => new(checked(size.X), checked(size.Y));
    public static explicit operator Point(Maths.Vector2Int size) => new(size.X, size.Y);

    public static Point Max(Point a, Point b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    public static Point Min(Point a, Point b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

    public readonly void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
