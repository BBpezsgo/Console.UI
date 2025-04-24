using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace CLI;

public readonly ref struct Span2D<T>
{
    public readonly Span<T> Span;
    public readonly int Width;
    public readonly int Height;

    public ref T this[int x, int y] => ref Span[x + (y * Width)];
    public ref T this[Point point] => ref Span[point.X + (point.Y * Width)];

    public Span2D(Span<T> span, int width, int height)
    {
        Span = span;
        Width = width;
        Height = height;
    }

    public Span2D(Span<T> span, int width)
    {
        Span = span;
        Width = width;
        Height = span.Length / width;
    }

    public bool Equals(Span2D<T> other) =>
        Width == other.Width &&
        Height == other.Height &&
        Span == other.Span;

    [DoesNotReturn]
    public override int GetHashCode() => throw new NotSupportedException($"Cannot call {nameof(GetHashCode)} on {nameof(Span<T>)}");

    [DoesNotReturn]
    public override bool Equals([NotNullWhen(true)] object? obj) => throw new NotSupportedException($"Cannot call {nameof(Equals)} on {nameof(Span<T>)}");

    public static bool operator ==(Span2D<T> left, Span2D<T> right) => left.Equals(right);
    public static bool operator !=(Span2D<T> left, Span2D<T> right) => !left.Equals(right);

    public override string ToString() => $"( {Width}x{Height} {Span.ToString()} )";

    public bool IsVisible(Vector2 point) => point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
    public bool IsVisible(Point point) => point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
}

public readonly ref struct ReadOnlySpan2D<T>
{
    public readonly ReadOnlySpan<T> Span;
    public readonly int Width;
    public readonly int Height;

    public ReadOnlySpan2D(ReadOnlySpan<T> span, int width, int height)
    {
        Span = span;
        Width = width;
        Height = height;
    }

    public ReadOnlySpan2D(ReadOnlySpan<T> span, int width)
    {
        Span = span;
        Width = width;
        Height = span.Length / width;
    }

    public bool Equals(ReadOnlySpan2D<T> other) =>
        Width == other.Width &&
        Height == other.Height &&
        Span == other.Span;

    [DoesNotReturn]
    public override int GetHashCode() => throw new NotSupportedException($"Cannot call {nameof(GetHashCode)} on {nameof(Span<T>)}");

    [DoesNotReturn]
    public override bool Equals([NotNullWhen(true)] object? obj) => throw new NotSupportedException($"Cannot call {nameof(Equals)} on {nameof(Span<T>)}");

    public static bool operator ==(ReadOnlySpan2D<T> left, ReadOnlySpan2D<T> right) => left.Equals(right);
    public static bool operator !=(ReadOnlySpan2D<T> left, ReadOnlySpan2D<T> right) => !left.Equals(right);

    public override string ToString() => $"( {Width}x{Height} {Span.ToString()} )";
}

public static class BufferUtils
{
    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Clear<T>(Span2D<T> buffer, Rect rect)
    {
        for (int offsetY = 0; offsetY < rect.Height; offsetY++)
        {
            int y = rect.Y + offsetY;
            if (y >= buffer.Height) break;
            if (y < 0) continue;

            int startIndex = (y * buffer.Width) + Math.Max((short)0, rect.Left);
            int endIndex = (y * buffer.Width) + Math.Min(buffer.Width - 1, rect.Right);
            int length = Math.Max(0, endIndex - startIndex);

            buffer.Span.Slice(startIndex, length).Clear();
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Clear<T>(Span<T> buffer, int bufferWidth, int bufferHeight, Rect rect)
    {
        for (int offsetY = 0; offsetY < rect.Height; offsetY++)
        {
            int y = rect.Y + offsetY;
            if (y >= bufferHeight) break;
            if (y < 0) continue;

            int startIndex = (y * bufferWidth) + Math.Max((short)0, rect.Left);
            int endIndex = (y * bufferWidth) + Math.Min(bufferWidth - 1, rect.Right);
            int length = Math.Max(0, endIndex - startIndex);

            buffer.Slice(startIndex, length).Clear();
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Put<T>(Span2D<T> buffer, int destinationX, int destinationY, ReadOnlySpan2D<T> data)
    {
        for (int offsetY = 0; offsetY < data.Height; offsetY++)
        {
            if (destinationY + offsetY < 0) continue;
            if (destinationY + offsetY >= buffer.Height) break;

            ReadOnlySpan<T> row = data.Span.Slice(offsetY * data.Width, data.Width);
            BufferUtils.PutRow(buffer.Span, buffer.Width, destinationX, destinationY + offsetY, row);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Put<T>(Span<T> buffer, int bufferWidth, int bufferHeight, int destinationX, int destinationY, ReadOnlySpan<T> data, int dataWidth, int dataHeight)
    {
        for (int offsetY = 0; offsetY < dataHeight; offsetY++)
        {
            if (destinationY + offsetY < 0) continue;
            if (destinationY + offsetY >= bufferHeight) break;

            ReadOnlySpan<T> row = data.Slice(offsetY * dataWidth, dataWidth);
            BufferUtils.PutRow(buffer, bufferWidth, destinationX, destinationY + offsetY, row);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void PutRow<T>(Span<T> buffer, int bufferWidth, int destinationX, int destinationY, ReadOnlySpan<T> data)
    {
        ReadOnlySpan<T> source = data;

        if (destinationX < 0)
        {
            if (-destinationX >= data.Length) return;
            source = source[-destinationX..];
            destinationX = 0;
        }

        if (destinationX + source.Length > bufferWidth)
        {
            if (bufferWidth <= destinationX) return;
            source = source[..(bufferWidth - destinationX)];
        }

        Span<T> destination = buffer[(destinationX + (destinationY * bufferWidth))..];

        source.CopyTo(destination);
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Fill<T>(Span2D<T> buffer, Rect rect, T value)
    {
        int rectHeight = rect.Height;
        int rectWidth = rect.Width;
        int rectX = rect.X;
        int rectY = rect.Y;

        for (int offsetY = 0; offsetY < rectHeight; offsetY++)
        {
            int y = rectY + offsetY;
            if (y >= buffer.Height) break;
            if (y < 0) continue;

            BufferUtils.FillRow(buffer.Span, buffer.Width, rectX, y, value, rectWidth);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Fill<T>(Span<T> buffer, int bufferWidth, int bufferHeight, Rect rect, T value)
    {
        int rectHeight = rect.Height;
        int rectWidth = rect.Width;
        int rectX = rect.X;
        int rectY = rect.Y;

        for (int offsetY = 0; offsetY < rectHeight; offsetY++)
        {
            int y = rectY + offsetY;
            if (y >= bufferHeight) break;
            if (y < 0) continue;

            BufferUtils.FillRow(buffer, bufferWidth, rectX, y, value, rectWidth);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void FillRow<T>(Span<T> buffer, int bufferWidth, int destinationX, int destinationY, T data, int dataLength)
    {
        int startIndex = (destinationY * bufferWidth) + Math.Max(0, destinationX);
        int endIndex = (destinationY * bufferWidth) + Math.Min(bufferWidth - 1, destinationX + dataLength);

        if (startIndex >= endIndex) return;

        dataLength = Math.Max(0, endIndex - startIndex);

        buffer.Slice(startIndex, dataLength).Fill(data);
    }
}
