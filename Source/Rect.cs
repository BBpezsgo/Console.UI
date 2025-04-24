using System;
using System.Diagnostics;
using System.Numerics;

namespace CLI;

[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public struct Rect :
    IEquatable<Rect>
{
    int left;
    int top;
    int right;
    int bottom;

    public int X
    {
        readonly get => left;
        set
        {
            int width = Width;
            left = value;
            right = (int)(value + width);
        }
    }
    public int Y
    {
        readonly get => top;
        set
        {
            int height = Height;
            top = value;
            bottom = (int)(value + height);
        }
    }

    public int Width
    {
        readonly get => (int)(right - left);
        set => right = (int)(left + value);
    }
    public int Height
    {
        readonly get => (int)(bottom - top);
        set => bottom = (int)(top + value);
    }

    public int Top
    {
        readonly get => top;
        set
        {
            top = value;
            bottom = Math.Max(top, bottom);
        }
    }
    public int Left
    {
        readonly get => left;
        set
        {
            left = value;
            right = Math.Max(left, right);
        }
    }
    public int Bottom
    {
        readonly get => bottom;
        set
        {
            bottom = value;
            top = Math.Min(top, bottom);
        }
    }
    public int Right
    {
        readonly get => right;
        set
        {
            right = value;
            left = Math.Min(left, right);
        }
    }

    public Point Position
    {
        readonly get => new(left, top);
        set
        {
            int offsetX = value.X - left;
            int offsetY = value.Y - top;
            left = value.X;
            top = value.Y;
            bottom = (int)(bottom + offsetY);
            right = (int)(right + offsetX);
        }
    }
    public Point Size
    {
        readonly get => new(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public Point Center
    {
        readonly get => new((left + right) / 2, (top + bottom) / 2);
        set
        {
            int width = Width;
            int height = Height;

            top = (int)(value.Y - (height / 2));
            bottom = (int)(top + height);

            left = (int)(value.X - (width / 2));
            right = (int)(left + width);
        }
    }

    public Rect(Point position, Point size)
    {
        top = position.Y;
        left = position.X;
        bottom = (int)(position.Y + size.Y);
        right = (int)(position.X + size.X);
    }

    public Rect(int x, int y, int width, int height)
    {
        top = y;
        left = x;
        bottom = (int)(y + height);
        right = (int)(x + width);
    }

    public static implicit operator System.Drawing.Rectangle(Rect rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    public static implicit operator System.Drawing.RectangleF(Rect rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    /// <exception cref="OverflowException"/>
    public static explicit operator checked Rect(System.Drawing.Rectangle rectangle) => new(checked((int)rectangle.X), checked((int)rectangle.Y), checked((int)rectangle.Width), checked((int)rectangle.Height));
    public static explicit operator Rect(System.Drawing.Rectangle rectangle) => new((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);

    public static bool operator ==(Rect a, Rect b) => a.Equals(b);
    public static bool operator !=(Rect a, Rect b) => !a.Equals(b);

    public override readonly string ToString() => $"{{ X: {X} Y: {Y} W: {Width} H: {Height} }}";
    public override readonly bool Equals(object? obj) => obj is Rect rect && Equals(rect);
    public readonly bool Equals(Rect other) =>
        left == other.left &&
        top == other.top &&
        right == other.right &&
        bottom == other.bottom;
    public override readonly int GetHashCode() => HashCode.Combine(left, top, right, bottom);

    #region Contains()

    public readonly bool Contains(Vector2 point) =>
        point.X >= left &&
        point.Y >= top &&
        point.X < right &&
        point.Y < bottom;

    public readonly bool Contains(Point point) =>
        point.X >= left &&
        point.Y >= top &&
        point.X <= right &&
        point.Y <= bottom;

    public readonly bool Contains(System.Drawing.Point point) =>
        point.X >= left &&
        point.Y >= top &&
        point.X < right &&
        point.Y < bottom;

    public readonly bool Contains(System.Drawing.PointF point) =>
        point.X >= left &&
        point.Y >= top &&
        point.X < right &&
        point.Y < bottom;

    public readonly bool Contains(int x, int y) =>
        x >= left &&
        y >= top &&
        x < right &&
        y < bottom;

    public readonly bool Contains(float x, float y) =>
        x >= left &&
        y >= top &&
        x < right &&
        y < bottom;

    #endregion

    #region Margin()

    public readonly Rect Margin(int all) => Margin((short)all);
    public readonly Rect Margin(short all)
    {
        Rect result = this;

        result.top += all;
        result.left += all;
        result.bottom -= all;
        result.right -= all;

        Rect.Fix(ref result);

        return result;
    }

    public readonly Rect Margin(int vertical, int horizontal) => Margin((short)vertical, (short)horizontal);
    public readonly Rect Margin(short vertical, short horizontal)
    {
        Rect result = this;

        result.top += vertical;
        result.left += horizontal;
        result.bottom -= vertical;
        result.right -= horizontal;

        Rect.Fix(ref result);

        return result;
    }

    #endregion

    #region Fix()

    static void Fix(ref Rect rect)
    {
        if (rect.left > rect.right)
        {
            short middle = (short)(((int)rect.left + (int)rect.right) / 2);
            rect.left = middle;
            rect.right = middle;
        }

        if (rect.top > rect.bottom)
        {
            short middle = (short)(((int)rect.top + (int)rect.bottom) / 2);
            rect.top = middle;
            rect.bottom = middle;
        }
    }

    #endregion

    public readonly void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }

    public readonly void Deconstruct(out Point position, out Point size)
    {
        position = Position;
        size = Size;
    }

    public readonly Rect Union(Rect other)
    {
        Rect result = this;
        result.top = Math.Min(result.top, other.top);
        result.left = Math.Min(result.left, other.left);
        result.bottom = Math.Max(result.bottom, other.bottom);
        result.right = Math.Max(result.right, other.right);
        return result;
    }

    public static implicit operator Rect(Maths.RectInt v) => new(v.X, v.Y, v.Width, v.Height);
    public static implicit operator Maths.RectInt(Rect v) => new(v.X, v.Y, v.Width, v.Height);
}

